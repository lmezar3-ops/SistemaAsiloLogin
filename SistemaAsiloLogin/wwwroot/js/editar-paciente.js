
document.addEventListener("DOMContentLoaded", async () => {
    const formulario = document.getElementById("formEditarPaciente");
    const lista = document.getElementById("listaMedicacionesHabituales");
    const botonAgregar = document.getElementById("agregarMedicacionHabitual");
    const botonGuardar = document.getElementById("guardarCambiosPaciente");
    const estadoCarga = document.getElementById("estadoCargaMedicaciones");
    const mensaje = document.getElementById("mensajeMedicaciones");

    if (!formulario || !lista || !botonAgregar || !botonGuardar) {
        return;
    }

    const pacienteId = Number(formulario.dataset.pacienteId);
    let catalogoMedicamentos = [];

    function mostrarMensaje(texto, error = false) {
        mensaje.textContent = texto;
        mensaje.style.color = error ? "#b91c1c" : "#166534";
    }

    async function leerRespuesta(respuesta) {
        const texto = await respuesta.text();

        if (!texto) {
            return {};
        }

        try {
            return JSON.parse(texto);
        } catch {
            return { mensaje: texto };
        }
    }

    function valor(objeto, ...nombres) {
        for (const nombre of nombres) {
            if (objeto[nombre] !== undefined && objeto[nombre] !== null) {
                return objeto[nombre];
            }
        }

        return null;
    }

    function fechaParaInput(fecha) {
        if (!fecha) return "";

        const texto = String(fecha);
        return texto.length >= 10 ? texto.substring(0, 10) : texto;
    }

    function crearCampo(etiqueta, tipo, valorInicial = "", opciones = {}) {
        const contenedor = document.createElement("div");
        const label = document.createElement("label");
        const control = tipo === "select"
            ? document.createElement("select")
            : document.createElement("input");

        label.textContent = etiqueta;
        control.className = "campo-medicacion";
        control.dataset.campo = opciones.campo || "";

        if (tipo === "select") {
            const inicial = document.createElement("option");
            inicial.value = "";
            inicial.textContent = "Seleccione del catálogo...";
            control.appendChild(inicial);

            for (const medicamento of catalogoMedicamentos) {
                const id = valor(
                    medicamento,
                    "idMedicamento",
                    "IdMedicamento",
                    "id"
                );

                const nombre = valor(
                    medicamento,
                    "nombreMedicamento",
                    "NombreMedicamento",
                    "nombre",
                    "Nombre"
                );

                if (id == null || nombre == null) continue;

                const opcion = document.createElement("option");
                opcion.value = String(id);
                opcion.textContent = String(nombre);
                control.appendChild(opcion);
            }

            control.value = valorInicial == null ? "" : String(valorInicial);
        } else {
            control.type = tipo;
            control.value = valorInicial ?? "";
        }

        contenedor.appendChild(label);
        contenedor.appendChild(control);

        return { contenedor, control };
    }

    function crearFila(medicacion = null) {
        const idExistente = medicacion
            ? Number(valor(
                medicacion,
                "idMedicacionHabitual",
                "IdMedicacionHabitual"
            ))
            : 0;

        const fila = document.createElement("div");
        fila.className = "fila-medicacion-habitual";
        fila.dataset.id = String(idExistente || 0);

        const titulo = document.createElement("h3");
        titulo.textContent = idExistente
            ? `Medicación registrada #${idExistente}`
            : "Nueva medicación";
        fila.appendChild(titulo);

        const catalogo = crearCampo(
            "Medicamento del catálogo",
            "select",
            valor(medicacion, "idMedicamento", "IdMedicamento"),
            { campo: "idMedicamento" }
        );

        const nombre = crearCampo(
            "Nombre informado (si no está en el catálogo)",
            "text",
            valor(
                medicacion,
                "nombreMedicamentoInformado",
                "NombreMedicamentoInformado"
            ) || "",
            { campo: "nombreMedicamentoInformado" }
        );
        nombre.control.maxLength = 150;

        const dosis = crearCampo(
            "Dosis",
            "text",
            valor(medicacion, "dosis", "Dosis") || "",
            { campo: "dosis" }
        );
        dosis.control.maxLength = 100;

        const frecuencia = crearCampo(
            "Frecuencia",
            "text",
            valor(medicacion, "frecuencia", "Frecuencia") || "",
            { campo: "frecuencia" }
        );
        frecuencia.control.maxLength = 100;

        const indicaciones = crearCampo(
            "Indicaciones",
            "text",
            valor(medicacion, "indicaciones", "Indicaciones") || "",
            { campo: "indicaciones" }
        );
        indicaciones.control.maxLength = 500;

        const inicio = crearCampo(
            "Fecha de inicio",
            "date",
            fechaParaInput(valor(medicacion, "fechaInicio", "FechaInicio")),
            { campo: "fechaInicio" }
        );

        const fin = crearCampo(
            "Fecha de finalización",
            "date",
            fechaParaInput(valor(medicacion, "fechaFin", "FechaFin")),
            { campo: "fechaFin" }
        );

        const estadoContenedor = document.createElement("div");
        const estadoLabel = document.createElement("label");
        const estado = document.createElement("input");

        estado.type = "checkbox";
        estado.dataset.campo = "estado";
        estado.checked = medicacion
            ? valor(medicacion, "estado", "Estado") !== false
                && valor(medicacion, "estado", "Estado") !== 0
            : true;

        estadoLabel.appendChild(estado);
        estadoLabel.append(" Medicación activa");
        estadoContenedor.appendChild(estadoLabel);

        const verificacion = document.createElement("p");
        if (medicacion &&
            valor(medicacion, "requiereVerificacion", "RequiereVerificacion")) {
            verificacion.textContent =
                "Este medicamento requiere verificación profesional.";
            verificacion.className = "alert alert-warning";
        }

        fila.append(
            catalogo.contenedor,
            nombre.contenedor,
            dosis.contenedor,
            frecuencia.contenedor,
            indicaciones.contenedor,
            inicio.contenedor,
            fin.contenedor,
            estadoContenedor
        );

        if (verificacion.textContent) {
            fila.appendChild(verificacion);
        }

        // Solo las filas nuevas se pueden quitar sin guardar.
        if (!idExistente) {
            const quitar = document.createElement("button");
            quitar.type = "button";
            quitar.textContent = "Quitar fila";
            quitar.addEventListener("click", () => fila.remove());
            fila.appendChild(quitar);
        }

        catalogo.control.addEventListener("change", () => {
            if (catalogo.control.value) {
                nombre.control.value = "";
            }
        });

        nombre.control.addEventListener("input", () => {
            if (nombre.control.value.trim()) {
                catalogo.control.value = "";
            }
        });

        lista.appendChild(fila);
    }

    function leerFila(fila) {
        const campo = nombre => fila.querySelector(
            `[data-campo="${nombre}"]`
        );

        return {
            idExistente: Number(fila.dataset.id || 0),
            idMedicamento: campo("idMedicamento").value
                ? Number(campo("idMedicamento").value)
                : null,
            nombreMedicamentoInformado:
                campo("nombreMedicamentoInformado").value.trim(),
            dosis: campo("dosis").value.trim(),
            frecuencia: campo("frecuencia").value.trim(),
            indicaciones: campo("indicaciones").value.trim(),
            fechaInicio: campo("fechaInicio").value || null,
            fechaFin: campo("fechaFin").value || null,
            estado: campo("estado").checked
        };
    }

    async function cargarMedicaciones() {
        try {
            const [respuestaCatalogo, respuestaMedicaciones] = await Promise.all([
                fetch("/api/medicaciones-habituales/catalogo"),
                fetch(`/api/medicaciones-habituales/${pacienteId}`)
            ]);

            const datosCatalogo = await leerRespuesta(respuestaCatalogo);
            const datosMedicaciones = await leerRespuesta(respuestaMedicaciones);

            if (!respuestaCatalogo.ok) {
                throw new Error(
                    datosCatalogo.mensaje || "No se pudo cargar el catálogo."
                );
            }

            if (!respuestaMedicaciones.ok) {
                throw new Error(
                    datosMedicaciones.mensaje ||
                    "No se pudieron consultar las medicaciones del paciente."
                );
            }

            catalogoMedicamentos = Array.isArray(datosCatalogo)
                ? datosCatalogo
                : valor(datosCatalogo, "medicamentos", "items") || [];

            const medicaciones = Array.isArray(datosMedicaciones)
                ? datosMedicaciones
                : valor(datosMedicaciones, "medicaciones", "items") || [];

            lista.replaceChildren();

            if (medicaciones.length) {
                medicaciones.forEach(crearFila);
            } else {
                estadoCarga.textContent =
                    "Este paciente todavía no tiene medicación habitual registrada.";
            }

            estadoCarga.textContent = medicaciones.length
                ? `Se cargaron ${medicaciones.length} medicación(es).`
                : "No hay medicaciones habituales registradas.";
        } catch (error) {
            estadoCarga.textContent = error.message ||
                "No se pudieron cargar los medicamentos.";
            mostrarMensaje(
                "No se pudo cargar el listado. No guardes cambios de medicación hasta resolver el problema.",
                true
            );
        }
    }

    botonAgregar.addEventListener("click", () => {
        crearFila();
    });

    formulario.addEventListener("submit", async evento => {
        evento.preventDefault();
        mostrarMensaje("");

        if (!formulario.reportValidity()) {
            return;
        }

        const filas = [...lista.querySelectorAll(".fila-medicacion-habitual")];
        const medicaciones = filas.map(leerFila);
        const nuevas = [];
        const existentes = [];

        for (const item of medicaciones) {
            const estaVacia = !item.idMedicamento &&
                !item.nombreMedicamentoInformado &&
                !item.dosis &&
                !item.frecuencia &&
                !item.indicaciones &&
                !item.fechaInicio &&
                !item.fechaFin;

            if (!item.idExistente && estaVacia) {
                continue;
            }

            if (!item.idMedicamento && !item.nombreMedicamentoInformado) {
                mostrarMensaje(
                    "En cada medicación registrada, selecciona un medicamento del catálogo o escribe su nombre.",
                    true
                );
                return;
            }

            if (item.idMedicamento && item.nombreMedicamentoInformado) {
                mostrarMensaje(
                    "Usa el catálogo o el nombre informado, pero no ambos en la misma medicación.",
                    true
                );
                return;
            }

            if (item.fechaInicio && item.fechaFin &&
                item.fechaFin < item.fechaInicio) {
                mostrarMensaje(
                    "La fecha de finalización no puede ser anterior a la de inicio.",
                    true
                );
                return;
            }

            const { idExistente, ...datos } = item;

            if (idExistente) {
                existentes.push({ idExistente, datos });
            } else {
                nuevas.push(datos);
            }
        }

        botonGuardar.disabled = true;
        botonAgregar.disabled = true;
        botonGuardar.textContent = "Guardando...";

        try {
            const obtener = id => document.getElementById(id);

            const paciente = {
                idPaciente: pacienteId,
                nombres: obtener("Paciente_Nombres").value.trim(),
                apellidos: obtener("Paciente_Apellidos").value.trim(),
                fechaNacimiento: obtener("Paciente_FechaNacimiento").value,
                dpi: obtener("Paciente_DPI").value.trim(),
                sexo: obtener("Paciente_Sexo").value,
                direccion: obtener("Paciente_Direccion").value.trim(),
                telefono: obtener("Paciente_Telefono").value.trim(),
                motivoIngreso: obtener("Paciente_MotivoIngreso").value.trim(),
                psicopatologia: obtener("Paciente_Psicopatologia").value.trim(),
                fechaIngreso: obtener("Paciente_FechaIngreso").value,
                estado: ["true", "1"].includes(
                    obtener("Paciente_Estado").value.toLowerCase()
                )
            };

            const respuestaPaciente = await fetch(
                `http://localhost:32768/api/Ancianos/${pacienteId}`,
                {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(paciente)
                }
            );

            const resultadoPaciente = await leerRespuesta(respuestaPaciente);

            if (!respuestaPaciente.ok) {
                throw new Error(
                    resultadoPaciente.mensaje ||
                    resultadoPaciente.title ||
                    `No se pudo actualizar el paciente (HTTP ${respuestaPaciente.status}).`
                );
            }

            // Actualiza las medicaciones existentes sin borrar su historial.
            for (const item of existentes) {
                const respuesta = await fetch(
                    `/api/medicaciones-habituales/${pacienteId}/${item.idExistente}`,
                    {
                        method: "PUT",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify(item.datos)
                    }
                );

                const resultado = await leerRespuesta(respuesta);

                if (!respuesta.ok) {
                    throw new Error(
                        resultado.mensaje ||
                        `No se pudo actualizar la medicación #${item.idExistente}.`
                    );
                }
            }

            // Envía todas las medicaciones nuevas en una sola petición.
            if (nuevas.length) {
                const respuesta = await fetch(
                    `/api/medicaciones-habituales/${pacienteId}`,
                    {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify(nuevas)
                    }
                );

                const resultado = await leerRespuesta(respuesta);

                if (!respuesta.ok) {
                    throw new Error(
                        resultado.mensaje ||
                        "El paciente se actualizó, pero no se pudieron guardar las nuevas medicaciones."
                    );
                }
            }

            window.location.href =
                "/PacientesRegistrados?actualizado=1";
        } catch (error) {
            mostrarMensaje(
                error.message ||
                "Ocurrió un error al guardar. Revisa los datos e inténtalo de nuevo.",
                true
            );
        } finally {
            botonGuardar.disabled = false;
            botonAgregar.disabled = false;
            botonGuardar.textContent = "Guardar cambios";
        }
    });

    if (!Number.isInteger(pacienteId) || pacienteId <= 0) {
        estadoCarga.textContent = "El identificador del paciente no es válido.";
        botonAgregar.disabled = true;
        botonGuardar.disabled = true;
        return;
    }

    await cargarMedicaciones();
});
