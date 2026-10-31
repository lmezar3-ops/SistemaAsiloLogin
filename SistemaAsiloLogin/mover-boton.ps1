$archivo = ".\Pages\RegistroAncianos.cshtml"
$contenido = Get-Content $archivo -Raw

$patron = '(?s)\s*<button onclick="registrarAnciano\(\)">\s*Guardar Paciente\s*</button>\s*'

if (-not $contenido.Contains('id="listaMedicacionesHabituales"')) {
    throw "No se encontró la sección de medicación. No se modificó el formulario."
}

if (-not $contenido.Contains('<p id="mensaje"></p>')) {
    throw "No se encontró el mensaje. No se modificó el formulario."
}

$contenido = [regex]::Replace($contenido, $patron, "`r`n", 1)

$marcador = '<p id="mensaje"></p>'

$botonFinal = @"
        <button type="button" onclick="registrarAnciano()">
            Guardar Paciente
        </button>

        <p id="mensaje"></p>
"@

$contenido = $contenido.Replace($marcador, $botonFinal)

Set-Content -Path $archivo -Value $contenido -Encoding UTF8
Write-Output "Formulario actualizado."
