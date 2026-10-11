async function cargarDonaciones() {

    try {

        const respuesta =
            await fetch("http://localhost:8080/api/Donaciones");

        const datos = await respuesta.json();

        let tabla =
            document.getElementById("tablaDonaciones");

        tabla.innerHTML = "";

        datos.forEach(donacion => {

            tabla.innerHTML += `
                <tr>
                    <td>${donacion.id}</td>
                    <td>${donacion.donante}</td>
                    <td>Q${donacion.monto}</td>
                    <td>${donacion.fecha}</td>
                </tr>
            `;
        });

    }
    catch (error) {

        console.error(error);

        alert("Error al consultar donaciones");
    }
}