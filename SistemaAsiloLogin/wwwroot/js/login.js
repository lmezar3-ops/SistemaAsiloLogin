async function iniciarSesion() {

    let usuario = document.getElementById("usuario").value;
    let password = document.getElementById("password").value;

    let mensaje = document.getElementById("mensaje");

    if (usuario === "" || password === "") {
        mensaje.innerText = "Debe ingresar su usuario y su contraseña";
        return;
    }

    try {

        let respuesta = await fetch("/api/Login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                nombreUsuario: usuario,
                contraseña: password
            })
        });

        let datos = await respuesta.json();

        if (respuesta.ok) {

            mensaje.innerText = datos.mensaje + " - Rol: " + datos.rol;

            setTimeout(() => {
                window.location.href = "/Dashboard";
            }, 1000);
        }
        else {
            mensaje.innerText = datos.mensaje;
        }

    }
    catch (error) {
        mensaje.innerText = "Error al conectar con el servidor";
        console.error(error);
    }
}

//cambio
function mostrarOcultarPassword() {

    const password = document.getElementById("password");
    const boton = document.getElementById("togglePassword");

    if (password.type === "password") {

        password.type = "text";
        boton.innerText = "🙈";

    } else {

        password.type = "password";
        boton.innerText = "👁️";
    }
}