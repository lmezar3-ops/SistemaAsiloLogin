
const API_MEDICAMENTOS = "http://localhost:8081/api/Medicamentos";



function filtrarMedicamentos() {
    const buscador = document.getElementById("buscarMedicamento");
    const tabla = document.getElementById("tablaMedicamentos");
    const resultado = document.getElementById("resultadoBusquedaMedicamento");

    if (!buscador || !tabla || !resultado) return;

    const filas = Array.from(
        tabla.querySelectorAll("tr[data-medicamento-row]")
    );

    const consultaOriginal = buscador.value.trim();
    const consulta = consultaOriginal.toLocaleLowerCase("es-GT");
    let coincidencias = 0;

    // Si se escribió un número, comprobar si corresponde a un ID exacto.
    const esNumero = /^\d+$/.test(consultaOriginal);

    const filaIdExacto = esNumero
        ? filas.find(fila =>
            fila.cells[0]?.textContent.trim() === consultaOriginal
        )
        : null;

    filas.forEach(fila => {
        let coincide;

        if (filaIdExacto) {
            // Si existe ese ID, mostrar solamente ese medicamento.
            coincide = fila === filaIdExacto;
        } else {
            // Si no hay ID exacto, buscar en el contenido de la fila.
            coincide = fila.textContent
                .toLocaleLowerCase("es-GT")
                .includes(consulta);
        }

        fila.hidden = !coincide;

        if (coincide) coincidencias++;
    });

    let aviso = tabla.querySelector("[data-no-results]");

    if (consulta && coincidencias === 0) {
        if (!aviso) {
            aviso = document.createElement("tr");
            aviso.dataset.noResults = "true";

            const celda = document.createElement("td");
            celda.colSpan = 7;
            celda.textContent =
                "No se encontraron medicamentos coincidentes.";

            aviso.appendChild(celda);
            tabla.appendChild(aviso);
        }
    } else if (aviso) {
        aviso.remove();
    }

    if (filas.length === 0) {
        resultado.textContent =
            "Consulta el inventario para comenzar la búsqueda.";
    } else {
        resultado.textContent = consulta
            ? `${coincidencias} medicamento(s) encontrado(s).`
            : `${filas.length} medicamento(s) en la consulta actual.`;
    }
}

document.getElementById("buscarMedicamento")
    ?.addEventListener("input", filtrarMedicamentos);

document.getElementById("limpiarBusquedaMedicamento")
    ?.addEventListener("click", () => {
        const buscador = document.getElementById("buscarMedicamento");

        buscador.value = "";
        filtrarMedicamentos();
        buscador.focus();
    });



async function cargarInventario() {
    await consultarMedicamentos(
        API_MEDICAMENTOS,
        "No hay medicamentos registrados.",
        false
    );
}

async function cargarMedicamentos() {
    await consultarMedicamentos(
        `${API_MEDICAMENTOS}/vencer`,
        "No hay medicamentos próximos a vencer.",
        true
    );
}



async function consultarMedicamentos(url, mensajeVacio, soloVencimientos) {
    const tabla = document.getElementById("tablaMedicamentos");

    tabla.innerHTML = `
        <tr>
            <td colspan="7">Consultando medicamentos...</td>
        </tr>
    `;

    try {
        const respuesta = await fetch(url);

        if (!respuesta.ok) {
            throw new Error(`Error HTTP: ${respuesta.status}`);
        }

        const datos = await respuesta.json();

datos.sort((a, b) =>
    Number(a.idMedicamento) - Number(b.idMedicamento)
);

tabla.innerHTML = "";

        if (!datos.length) {
            tabla.innerHTML = `
                <tr>
                    <td colspan="7">${mensajeVacio}</td>
                </tr>
            `;
            return;
        }

        datos.forEach(medicamento => {
            const fecha = medicamento.fechaVencimiento
                ? new Date(medicamento.fechaVencimiento).toLocaleDateString(
                    "es-GT",
                    {
                        year: "numeric",
                        month: "2-digit",
                        day: "2-digit",
                        timeZone: "UTC"
                    }
                )
                : "Sin fecha registrada";

            const fila = document.createElement("tr");

            fila.dataset.medicamentoRow = "true";

            [
                medicamento.idMedicamento,
                medicamento.nombre,
                medicamento.descripcion ?? "",
                medicamento.existencia,
                fecha,
                Number(medicamento.costo).toFixed(2),
                medicamento.estado ? "Activo" : "Inactivo"
            ].forEach(valor => {
                const celda = document.createElement("td");
                celda.textContent = valor ?? "";
                fila.appendChild(celda);
            });

            tabla.appendChild(fila);
           
        });
         filtrarMedicamentos();

    } catch (error) {
        console.error("Error al consultar medicamentos:", error);

        tabla.innerHTML = `
            <tr>
                <td colspan="7">
                    No fue posible consultar los medicamentos.
                    Comprueba que el microservicio esté funcionando.
                </td>
            </tr>
        `;
    }
}



document.getElementById("formMedicamento").addEventListener("submit", async function (evento) {
    evento.preventDefault();

    const mensaje = document.getElementById("mensajeMedicamento");
    const boton = document.getElementById("btnGuardarMedicamento");

    const nombre = document.getElementById("nombreMedicamento").value.trim();
    const descripcion = document.getElementById("descripcionMedicamento").value.trim();
    const existenciaTexto = document.getElementById("existenciaMedicamento").value;
    const fechaTexto = document.getElementById("fechaVencimientoMedicamento").value;
    const costoTexto = document.getElementById("costoMedicamento").value;

    const existencia = Number(existenciaTexto);
    const costo = Number(costoTexto);

    mensaje.textContent = "";
    mensaje.className = "mt-3";

    if (!nombre) {
        mensaje.textContent = "El nombre del medicamento es obligatorio.";
        mensaje.classList.add("text-danger");
        return;
    }

    if (!Number.isInteger(existencia) || existencia < 0) {
        mensaje.textContent = "La existencia debe ser un número entero no negativo.";
        mensaje.classList.add("text-danger");
        return;
    }

    if (!Number.isFinite(costo) || costo < 0 || costo > 99999999.99) {
        mensaje.textContent = "Ingresa un costo válido y no negativo.";
        mensaje.classList.add("text-danger");
        return;
    }

    const medicamento = {
        nombre: nombre,
        descripcion: descripcion || null,
        existencia: existencia,
        fechaVencimiento: fechaTexto || null,
        costo: costo,
        estado: document.getElementById("estadoMedicamento").checked
    };

    boton.disabled = true;
    boton.textContent = "Registrando...";

    try {
        const respuesta = await fetch(API_MEDICAMENTOS, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(medicamento)
        });

        const resultado = await respuesta.json().catch(() => ({}));

        if (!respuesta.ok) {
            const detalle = resultado.mensaje
                || resultado.title
                || `Error HTTP ${respuesta.status}`;

            throw new Error(detalle);
        }

        mensaje.textContent =
            resultado.mensaje || "Medicamento registrado correctamente.";
        mensaje.classList.add("text-success");

        this.reset();
        document.getElementById("existenciaMedicamento").value = "0";
        document.getElementById("costoMedicamento").value = "0.00";
        document.getElementById("estadoMedicamento").checked = true;

        await cargarInventario();
    } catch (error) {
        console.error("Error al registrar medicamento:", error);
        mensaje.textContent = `No fue posible registrar el medicamento: ${error.message}`;
        mensaje.classList.add("text-danger");
    } finally {
        boton.disabled = false;
        boton.textContent = "Registrar medicamento";
    }
});
