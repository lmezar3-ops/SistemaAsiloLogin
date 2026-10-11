async function registrarAnciano() {

    const nombres = document.getElementById("nombres").value.trim();
    const apellidos = document.getElementById("apellidos").value.trim();
    const fechaNacimiento = document.getElementById("fechaNacimiento").value;
    const sexo = document.getElementById("sexo").value;
    const direccion = document.getElementById("direccion").value.trim();
    const telefono = document.getElementById("telefono").value.trim();
    const fechaIngreso = document.getElementById("fechaIngreso").value;
    const estado = document.getElementById("estado").value;

    const mensaje = document.getElementById("mensaje");

    // Validar campos
    if (
        nombres === "" ||
        apellidos === "" ||
        fechaNacimiento === "" ||
        sexo === "" ||
        direccion === "" ||
        telefono === "" ||
        fechaIngreso === ""
    ) {
        mensaje.textContent = "Por favor, complete todos los campos.";
        return;
    }

    const paciente = {
        nombres: nombres,
        apellidos: apellidos,
        fechaNacimiento: fechaNacimiento,
        sexo: sexo,
        direccion: direccion,
        telefono: telefono,
        fechaIngreso: fechaIngreso,
        estado: estado === "1"
    };

    try {

        console.log("Paciente enviado:", paciente);

        const respuesta = await fetch("http://localhost:32768/api/Ancianos", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(paciente)
        });

        const resultado = await respuesta.json();

        if (!respuesta.ok) {
            throw new Error(
                resultado.mensaje || resultado.title || "Error al registrar el paciente."
            );
        }


        mensaje.textContent = resultado.mensaje;

        console.log("Respuesta del servidor:", resultado);

    } catch (error) {

        console.error("Error completo:", error);

        mensaje.textContent =
            "Error: " + error.message;
    }
}