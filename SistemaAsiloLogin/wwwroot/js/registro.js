let catalogoMedicamentos = [];
let pacienteRegistradoId = null;
let medicacionesPendientes = null;
let guardandoRegistro = false;

document.addEventListener("DOMContentLoaded", async () => {
    const botonAgregar = document.getElementById("agregarMedicacionHabitual");

    if (botonAgregar) {
        botonAgregar.addEventListener("click", agregarFilaMedicacion);
        agregarFilaMedicacion();
        await cargarCatalogoMedicamentos();
    }
});

async function cargarCatalogoMedicamentos() {
    const mensaje = document.getElementById("mensaje");

    try {
        const respuesta = await fetch("/api/medicaciones-habituales/catalogo", {
            method: "GET",
            credentials: "same-origin"
        });

        if (!respuesta.ok) {
            throw new Error("No se pudo consultar el catálogo de farmacia.");
        }

        catalogoMedicamentos = await respuesta.json();

        document.querySelectorAll(".fila-medicacion-habitual").forEach(fila => {
            llenarSelectorMedicamentos(fila);
        });
    } catch (error) {
        console.error("Error al cargar medicamentos:", error);
        mensaje.textContent =
            "No se pudo cargar el catálogo. Puedes ingresar el nombre informado y solicitar su verificación.";
    }
}

function agregarFilaMedicacion() {
    const contenedor = document.getElementById("listaMedicacionesHabituales");
    const fila = document.createElement("div");

    fila.className = "fila-medicacion-habitual";
    fila.style.border = "1px solid #cbd5e1";
    fila.style.borderRadius = "8px";
    fila.style.padding = "14px";
    fila.style.marginBottom = "12px";

    fila.innerHTML = `
        <h3>Medicación habitual</h3>

        <label>Medicamento del catálogo</label>
        <select class="medicamento-catalogo">
            <option value="">Cargando catálogo...</option>
        </select>

        <label>Nombre informado (si no aparece en el catálogo)</label>
        <input class="medicamento-informado" type="text"
               maxlength="150" placeholder="Nombre indicado por el paciente o familiar">

        <label>Dosis informada</label>
        <input class="medicacion-dosis" type="text" maxlength="100"
               placeholder="Ej. según receta o envase">

        <label>Frecuencia informada</label>
        <input class="medicacion-frecuencia" type="text" maxlength="100"
               placeholder="Ej. según receta o envase">

        <label>Indicaciones</label>
        <textarea class="medicacion-indicaciones" maxlength="500"
                  placeholder="Observaciones o instrucciones informadas"></textarea>

        <label>Fecha de inicio (opcional)</label>
        <input class="medicacion-fecha-inicio" type="date">

        <label>Fecha de finalización (opcional)</label>
        <input class="medicacion-fecha-fin" type="date">

        <label>Estado</label>
        <select class="medicacion-estado">
            <option value="true" selected>Vigente</option>
            <option value="false">Histórica / inactiva</option>
        </select>

        <button type="button" class="quitar-medicacion">
            Quitar esta medicación
        </button>
    `;

    contenedor.appendChild(fila);
    llenarSelectorMedicamentos(fila);

    fila.querySelector(".quitar-medicacion").addEventListener("click", () => {
        fila.remove();
    });
}

function llenarSelectorMedicamentos(fila) {
    const selector = fila.querySelector(".medicamento-catalogo");
    if (!selector) return;

    const seleccionado = selector.value;

    selector.replaceChildren();

    const opcionInicial = document.createElement("option");
    opcionInicial.value = "";
    opcionInicial.textContent = "Seleccione un medicamento o use el nombre informado";
    selector.appendChild(opcionInicial);

    catalogoMedicamentos.forEach(medicamento => {
        const opcion = document.createElement("option");
        opcion.value = String(medicamento.idMedicamento ?? medicamento.IdMedicamento);
        opcion.textContent = medicamento.nombre ?? medicamento.Nombre;
        selector.appendChild(opcion);
    });

    if (seleccionado && [...selector.options].some(o => o.value === seleccionado)) {
        selector.value = seleccionado;
    }
}

function obtenerMedicacionesFormulario() {
    const filas = [
        ...document.querySelectorAll(".fila-medicacion-habitual")
    ];

    const medicaciones = [];

    for (const fila of filas) {
        const idSeleccionado = fila.querySelector(".medicamento-catalogo").value;
        const nombreInformado = fila.querySelector(".medicamento-informado").value.trim();
        const dosis = fila.querySelector(".medicacion-dosis").value.trim();
        const frecuencia = fila.querySelector(".medicacion-frecuencia").value.trim();
        const indicaciones = fila.querySelector(".medicacion-indicaciones").value.trim();
        const fechaInicio = fila.querySelector(".medicacion-fecha-inicio").value;
        const fechaFin = fila.querySelector(".medicacion-fecha-fin").value;
        const estado = fila.querySelector(".medicacion-estado").value === "true";

        const tieneDatos = Boolean(
            idSeleccionado || nombreInformado || dosis || frecuencia ||
            indicaciones || fechaInicio || fechaFin
        );

        if (!tieneDatos) continue;

        if (Boolean(idSeleccionado) === Boolean(nombreInformado)) {
            throw new Error(
                "En cada medicación, seleccione un medicamento del catálogo o escriba un nombre informado, pero no ambos."
            );
        }

        if (fechaInicio && fechaFin && fechaFin < fechaInicio) {
            throw new Error(
                "La fecha de finalización no puede ser anterior a la fecha de inicio."
            );
        }

        medicaciones.push({
            idMedicamento: idSeleccionado ? Number(idSeleccionado) : null,
            nombreMedicamentoInformado: nombreInformado || null,
            dosis: dosis || null,
            frecuencia: frecuencia || null,
            indicaciones: indicaciones || null,
            fechaInicio: fechaInicio || null,
            fechaFin: fechaFin || null,
            estado: estado
        });
    }

    return medicaciones;
}

async function guardarMedicacionesPaciente() {
    if (!medicacionesPendientes || medicacionesPendientes.length === 0) {
        return true;
    }

    const respuesta = await fetch(
        `/api/medicaciones-habituales/${pacienteRegistradoId}`,
        {
            method: "POST",
            credentials: "same-origin",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(medicacionesPendientes)
        }
    );

    let resultado = {};
    try {
        resultado = await respuesta.json();
    } catch {
        // La respuesta puede no contener JSON.
    }

    if (!respuesta.ok) {
        throw new Error(
            resultado.mensaje ||
            "El paciente ya fue registrado, pero no se pudieron guardar sus medicaciones."
        );
    }

    medicacionesPendientes = null;
    return true;
}

async function registrarAnciano() {
    const mensaje = document.getElementById("mensaje");

    if (guardandoRegistro) return;
    guardandoRegistro = true;

    const botonGuardar = document.querySelector(
        'button[onclick="registrarAnciano()"]'
    );

    if (botonGuardar) botonGuardar.disabled = true;

    try {
        // Si el paciente ya se creó, reintentar solo las medicaciones.
        if (pacienteRegistradoId !== null) {
            await guardarMedicacionesPaciente();
            mensaje.textContent =
                "Las medicaciones habituales se guardaron correctamente para el paciente " +
                pacienteRegistradoId + ".";
            return;
        }

        const nombres = document.getElementById("nombres").value.trim();
        const apellidos = document.getElementById("apellidos").value.trim();
        const fechaNacimiento = document.getElementById("fechaNacimiento").value;
        const dpi = document.getElementById("dpi").value.trim();
        const sexo = document.getElementById("sexo").value;
        const direccion = document.getElementById("direccion").value.trim();
        const telefono = document.getElementById("telefono").value.trim();
        const motivoIngreso = document.getElementById("motivoIngreso").value.trim();
        const psicopatologia = document.getElementById("psicopatologia").value.trim();
        const fechaIngreso = document.getElementById("fechaIngreso").value;
        const estado = document.getElementById("estado").value;

        if (
            !nombres || !apellidos || !fechaNacimiento || !dpi ||
            !sexo || !direccion || !telefono || !motivoIngreso || !fechaIngreso
        ) {
            mensaje.textContent = "Por favor, complete todos los campos obligatorios.";
            return;
        }

        if (!/^\d{13}$/.test(dpi)) {
            mensaje.textContent =
                "El DPI debe contener exactamente 13 dígitos numéricos.";
            return;
        }

        const medicaciones = obtenerMedicacionesFormulario();

        const paciente = {
            nombres,
            apellidos,
            fechaNacimiento,
            dpi,
            sexo,
            direccion,
            telefono,
            motivoIngreso: motivoIngreso || null,
            psicopatologia: psicopatologia || null,
            fechaIngreso,
            estado: estado === "1"
        };

        mensaje.textContent = "Registrando paciente...";

        const respuesta = await fetch("http://localhost:32768/api/Ancianos", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(paciente)
        });

        let resultado = {};
        try {
            resultado = await respuesta.json();
        } catch {
            // La respuesta puede no contener JSON.
        }

        if (!respuesta.ok) {
            throw new Error(
                resultado.mensaje || resultado.title ||
                "Error al registrar el paciente."
            );
        }

        const idPaciente = resultado.paciente?.idPaciente ??
                           resultado.paciente?.IdPaciente;

        if (!idPaciente) {
            mensaje.textContent =
                "El servidor respondió al registro, pero no devolvió el identificador del paciente. Verifica el registro antes de volver a enviarlo.";
            return;
        }

        pacienteRegistradoId = Number(idPaciente);
        medicacionesPendientes = medicaciones;

        if (medicacionesPendientes.length > 0) {
            try {
                await guardarMedicacionesPaciente();
                mensaje.textContent =
                    "Paciente y medicaciones habituales registrados correctamente.";
            } catch (error) {
                console.error("Error al guardar medicaciones:", error);
                mensaje.textContent =
                    "El paciente se registró correctamente (ID " +
                    pacienteRegistradoId +
                    "), pero falló el guardado de las medicaciones. " +
                    "Corrige el problema y presiona el botón otra vez para reintentar solo las medicaciones. " +
                    error.message;
            }
        } else {
            mensaje.textContent = resultado.mensaje ||
                "Paciente registrado correctamente. No se ingresaron medicaciones habituales.";
        }

        console.log("Respuesta del registro:", resultado);
    } catch (error) {
        console.error("Error completo:", error);
        mensaje.textContent = "Error: " + error.message;
    } finally {
        guardandoRegistro = false;
        if (botonGuardar) botonGuardar.disabled = false;
    }
}
