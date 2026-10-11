
async function cargarInventario() {
    await consultarMedicamentos(
        "http://localhost:8081/api/Medicamentos",
        "No hay medicamentos registrados.",
        false
    );
}

async function cargarMedicamentos() {
    await consultarMedicamentos(
        "http://localhost:8081/api/Medicamentos/vencer",
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
        tabla.innerHTML = "";

        if (datos.length === 0) {
            tabla.innerHTML = `
                <tr>
                    <td colspan="7">${mensajeVacio}</td>
                </tr>
            `;
            return;
        }

        datos.forEach(medicamento => {
            const fecha = medicamento.fechaVencimiento
                ? new Date(medicamento.fechaVencimiento)
                    .toLocaleDateString("es-GT", {
                        year: "numeric",
                        month: "2-digit",
                        day: "2-digit",
                        timeZone: "UTC"
                    })
                : "Sin fecha registrada";

            if (soloVencimientos) {
                tabla.innerHTML += `
                    <tr>
                        <td>${medicamento.id}</td>
                        <td>${medicamento.nombre}</td>
                        <td>${fecha}</td>
                    </tr>
                `;
            } else {
                tabla.innerHTML += `
                    <tr>
                        <td>${medicamento.idMedicamento}</td>
                        <td>${medicamento.nombre}</td>
                        <td>${medicamento.descripcion ?? ""}</td>
                        <td>${medicamento.existencia}</td>
                        <td>${fecha}</td>
                        <td>${Number(medicamento.costo).toFixed(2)}</td>
                        <td>${medicamento.estado ? "Activo" : "Inactivo"}</td>
                    </tr>
                `;
            }
        });
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