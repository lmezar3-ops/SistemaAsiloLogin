-- Tabla Usuario
CREATE TABLE Usuario (
    idUsuario INT IDENTITY(1,1) PRIMARY KEY,
    nombreUsuario VARCHAR(50) NOT NULL,
    contraseña VARCHAR(100) NOT NULL,
    rol VARCHAR(50) NOT NULL,
    estado BIT NOT NULL
);

INSERT INTO Usuario (nombreUsuario, contraseña, rol, estado)
VALUES
('admin', '1234', 'Administrador', 1),
('medico', '1234', 'Medico', 1),
('enfermero', '1234', 'Enfermero', 1);

USE AsiloAncianos;
GO
-- Tabla Paciente
CREATE TABLE Paciente (
    idPaciente INT IDENTITY(1,1) PRIMARY KEY,
    nombres VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    fechaNacimiento DATE NOT NULL,
    sexo VARCHAR(20) NOT NULL,
    direccion VARCHAR(200) NOT NULL,
    telefono VARCHAR(20) NOT NULL,
    fechaIngreso DATE NOT NULL,
    estado BIT NOT NULL
);

IF COL_LENGTH('dbo.Paciente', 'dpi') IS NULL
BEGIN
    ALTER TABLE dbo.Paciente
    ADD dpi VARCHAR(13) NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_Paciente_DPI'
      AND object_id = OBJECT_ID('dbo.Paciente')
)
BEGIN
    CREATE UNIQUE INDEX UX_Paciente_DPI
    ON dbo.Paciente(dpi)
    WHERE dpi IS NOT NULL;
END;
GO


USE AsiloAncianos;
GO
CREATE TABLE Especialidad (
    idEspecialidad INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(300)
);

USE AsiloAncianos;
GO
CREATE TABLE Medico (
    idMedico INT IDENTITY(1,1) PRIMARY KEY,
    nombres VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    telefono VARCHAR(20),
    correoElectronico VARCHAR(150),
    idEspecialidad INT NOT NULL,
    estado BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_Medico_Especialidad
        FOREIGN KEY (idEspecialidad)
        REFERENCES Especialidad(idEspecialidad)
);

CREATE TABLE Enfermero (
    idEnfermero INT IDENTITY(1,1) PRIMARY KEY,
    nombres VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    telefono VARCHAR(20),
    estado BIT NOT NULL DEFAULT 1
);

CREATE TABLE SolicitudMedica (
    idSolicitud INT IDENTITY(1,1) PRIMARY KEY,

    idPaciente INT NOT NULL,
    idMedicoGeneral INT NOT NULL,
    idEspecialidad INT NOT NULL,
    idEnfermero INT NULL,

    fechaSolicitud DATETIME NOT NULL DEFAULT GETDATE(),
    motivo VARCHAR(500) NOT NULL,
    estado VARCHAR(50) NOT NULL DEFAULT 'Pendiente',

    CONSTRAINT FK_Solicitud_Paciente
        FOREIGN KEY (idPaciente)
        REFERENCES Paciente(idPaciente),

    CONSTRAINT FK_Solicitud_MedicoGeneral
        FOREIGN KEY (idMedicoGeneral)
        REFERENCES Medico(idMedico),

    CONSTRAINT FK_Solicitud_Especialidad
        FOREIGN KEY (idEspecialidad)
        REFERENCES Especialidad(idEspecialidad),

    CONSTRAINT FK_Solicitud_Enfermero
        FOREIGN KEY (idEnfermero)
        REFERENCES Enfermero(idEnfermero)
);

CREATE TABLE CitaMedica (
    idCita INT IDENTITY(1,1) PRIMARY KEY,

    idSolicitud INT NOT NULL,
    idMedico INT NOT NULL,

    fechaCita DATETIME NOT NULL,
    estado VARCHAR(50) NOT NULL DEFAULT 'Programada',
    observaciones VARCHAR(500),

    CONSTRAINT FK_Cita_Solicitud
        FOREIGN KEY (idSolicitud)
        REFERENCES SolicitudMedica(idSolicitud),

    CONSTRAINT FK_Cita_Medico
        FOREIGN KEY (idMedico)
        REFERENCES Medico(idMedico)
);

CREATE TABLE VisitaMedica (
    idVisita INT IDENTITY(1,1) PRIMARY KEY,

    idPaciente INT NOT NULL,
    idMedico INT NOT NULL,
    idCita INT NOT NULL,

    fechaVisita DATETIME NOT NULL DEFAULT GETDATE(),
    motivoConsulta VARCHAR(500) NOT NULL,
    diagnostico VARCHAR(500),
    observaciones VARCHAR(1000),

    CONSTRAINT FK_Visita_Paciente
        FOREIGN KEY (idPaciente)
        REFERENCES Paciente(idPaciente),

    CONSTRAINT FK_Visita_Medico
        FOREIGN KEY (idMedico)
        REFERENCES Medico(idMedico),

    CONSTRAINT FK_Visita_Cita
        FOREIGN KEY (idCita)
        REFERENCES CitaMedica(idCita)
);

CREATE TABLE ExamenLaboratorio (
    idExamen INT IDENTITY(1,1) PRIMARY KEY,

    idVisita INT NOT NULL,

    nombreExamen VARCHAR(150) NOT NULL,
    fechaSolicitud DATETIME NOT NULL DEFAULT GETDATE(),
    estado VARCHAR(50) NOT NULL DEFAULT 'Pendiente',

    CONSTRAINT FK_Examen_Visita
        FOREIGN KEY (idVisita)
        REFERENCES VisitaMedica(idVisita)
);

ALTER TABLE ExamenLaboratorio
ADD costo DECIMAL(10,2) NULL;

CREATE TABLE ResultadoExamen (
    idResultado INT IDENTITY(1,1) PRIMARY KEY,

    idExamen INT NOT NULL,

    resultado VARCHAR(1000) NOT NULL,
    fechaResultado DATETIME NOT NULL DEFAULT GETDATE(),
    observaciones VARCHAR(500),

    CONSTRAINT FK_ResultadoExamen_Examen
        FOREIGN KEY (idExamen)
        REFERENCES ExamenLaboratorio(idExamen)
);

CREATE TABLE Medicamento (
    idMedicamento INT IDENTITY(1,1) PRIMARY KEY,

    nombre VARCHAR(150) NOT NULL,
    descripcion VARCHAR(300),
    existencia INT NOT NULL DEFAULT 0,
    fechaVencimiento DATE,
    costo DECIMAL(10,2) NOT NULL DEFAULT 0,
    estado BIT NOT NULL DEFAULT 1
);

CREATE TABLE Tratamiento (
    idTratamiento INT IDENTITY(1,1) PRIMARY KEY,

    idVisita INT NOT NULL,
    idMedicamento INT NOT NULL,

    dosis VARCHAR(100) NOT NULL,
    frecuencia VARCHAR(100) NOT NULL,
    duracion VARCHAR(100) NOT NULL,
    cantidad INT NOT NULL,
    indicaciones VARCHAR(500),

    CONSTRAINT FK_Tratamiento_Visita
        FOREIGN KEY (idVisita)
        REFERENCES VisitaMedica(idVisita)
);

CREATE TABLE Fundacion (
    idFundacion INT IDENTITY(1,1) PRIMARY KEY,

    nombre VARCHAR(150) NOT NULL,
    telefono VARCHAR(20),
    correoElectronico VARCHAR(150),
    direccion VARCHAR(300),
    estado BIT NOT NULL DEFAULT 1
);

CREATE TABLE CargoPaciente (
    idCargo INT IDENTITY(1,1) PRIMARY KEY,

    idPaciente INT NOT NULL,
    idFundacion INT NOT NULL,

    fechaCargo DATETIME NOT NULL DEFAULT GETDATE(),

    tipoCargo VARCHAR(50) NOT NULL,
    concepto VARCHAR(300) NOT NULL,

    montoOriginal DECIMAL(10,2) NOT NULL,
    descuento DECIMAL(10,2) NOT NULL DEFAULT 0,
    montoFinal DECIMAL(10,2) NOT NULL,

    estado VARCHAR(50) NOT NULL DEFAULT 'Pendiente',

    CONSTRAINT FK_CargoPaciente_Paciente
        FOREIGN KEY (idPaciente)
        REFERENCES Paciente(idPaciente),

    CONSTRAINT FK_CargoPaciente_Fundacion
        FOREIGN KEY (idFundacion)
        REFERENCES Fundacion(idFundacion)
);


CREATE TABLE PagoPaciente (
    idPago INT IDENTITY(1,1) PRIMARY KEY,

    idCargo INT NOT NULL,

    fechaPago DATETIME NOT NULL DEFAULT GETDATE(),

    montoPagado DECIMAL(10,2) NOT NULL,

    metodoPago VARCHAR(50) NOT NULL,

    observaciones VARCHAR(500),

    CONSTRAINT FK_PagoPaciente_Cargo
        FOREIGN KEY (idCargo)
        REFERENCES CargoPaciente(idCargo)
);

CREATE TABLE Familiar (
    idFamiliar INT IDENTITY(1,1) PRIMARY KEY,
    nombres VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    parentesco VARCHAR(50) NOT NULL,
    telefono VARCHAR(20),
    correoElectronico VARCHAR(150),
    direccion VARCHAR(300),
    estado BIT NOT NULL DEFAULT 1
);

CREATE TABLE PacienteFamiliar (
    idPaciente INT NOT NULL,
    idFamiliar INT NOT NULL,
    parentesco VARCHAR(50) NOT NULL,
    esResponsable BIT NOT NULL DEFAULT 0,

    CONSTRAINT PK_PacienteFamiliar
        PRIMARY KEY (idPaciente, idFamiliar),

    CONSTRAINT FK_PacienteFamiliar_Paciente
        FOREIGN KEY (idPaciente)
        REFERENCES Paciente(idPaciente),

    CONSTRAINT FK_PacienteFamiliar_Familiar
        FOREIGN KEY (idFamiliar)
        REFERENCES Familiar(idFamiliar)
);

CREATE TABLE CuotaFamiliar (
    idCuota INT IDENTITY(1,1) PRIMARY KEY,
    idPaciente INT NOT NULL,
    fechaCuota DATE NOT NULL,
    monto DECIMAL(10,2) NOT NULL,
    estado VARCHAR(50) NOT NULL DEFAULT 'Pendiente',

    CONSTRAINT FK_CuotaFamiliar_Paciente
        FOREIGN KEY (idPaciente)
        REFERENCES Paciente(idPaciente)
);

CREATE TABLE PagoFundacion (
    idPagoFundacion INT IDENTITY(1,1) PRIMARY KEY,
    idFundacion INT NOT NULL,
    fechaPago DATE NOT NULL,
    monto DECIMAL(10,2) NOT NULL,
    descripcion VARCHAR(300),
    estado VARCHAR(50) NOT NULL DEFAULT 'Pagado',

    CONSTRAINT FK_PagoFundacion_Fundacion
        FOREIGN KEY (idFundacion)
        REFERENCES Fundacion(idFundacion)
);

CREATE TABLE Donante
(
    idDonante INT IDENTITY(1,1) NOT NULL,
    Nombre VARCHAR(150) NOT NULL,
    tipoDonante VARCHAR(50) NOT NULL,
    telefono VARCHAR(30) NULL,
    CorreoElectronico VARCHAR(150) NULL,
    direccion VARCHAR(250) NULL,

    CONSTRAINT PK_Donante
        PRIMARY KEY (idDonante)
);

CREATE TABLE Donacion
(
    idDonacion INT IDENTITY(1,1) NOT NULL,
    idDonante INT NOT NULL,
    fechaDonacion DATE NOT NULL,
    tipoDonacion VARCHAR(50) NOT NULL,
    descripcion VARCHAR(300) NULL,
    monto DECIMAL(10,2) NOT NULL,

    CONSTRAINT PK_Donacion
        PRIMARY KEY (idDonacion),

    CONSTRAINT FK_Donacion_Donante
        FOREIGN KEY (idDonante)
        REFERENCES Donante(idDonante)
);

CREATE TABLE Gasto (
    idGasto INT IDENTITY(1,1) PRIMARY KEY,
    fechaGasto DATE NOT NULL,
    tipoGasto VARCHAR(100) NOT NULL,
    concepto VARCHAR(300) NOT NULL,
    monto DECIMAL(10,2) NOT NULL,
    observaciones VARCHAR(500),
    estado VARCHAR(50) NOT NULL DEFAULT 'Registrado'
);

CREATE TABLE MovimientoCaja (
    idMovimiento INT IDENTITY(1,1) PRIMARY KEY,
    fechaMovimiento DATETIME NOT NULL DEFAULT GETDATE(),
    tipoMovimiento VARCHAR(20) NOT NULL,
    concepto VARCHAR(300) NOT NULL,
    monto DECIMAL(10,2) NOT NULL,
    observaciones VARCHAR(500),
    estado VARCHAR(50) NOT NULL DEFAULT 'Registrado'
);

CREATE TABLE Notificacion (
    idNotificacion INT IDENTITY(1,1) PRIMARY KEY,
    idPaciente INT NOT NULL,
    idFamiliar INT NULL,
    fechaNotificacion DATETIME NOT NULL DEFAULT GETDATE(),
    tipo VARCHAR(50) NOT NULL,
    asunto VARCHAR(200) NOT NULL,
    mensaje VARCHAR(1000) NOT NULL,
    estado VARCHAR(50) NOT NULL DEFAULT 'Pendiente',

    CONSTRAINT FK_Notificacion_Paciente
        FOREIGN KEY (idPaciente)
        REFERENCES Paciente(idPaciente),

    CONSTRAINT FK_Notificacion_Familiar
        FOREIGN KEY (idFamiliar)
        REFERENCES Familiar(idFamiliar)
);

INSERT INTO Fundacion
(
    nombre,
    telefono,
    correoElectronico,
    direccion,
    estado
)
VALUES
(
    'Fundacion Medica Asilo',
    '55550000',
    'contacto@fundacion.com',
    'Mazatenango',
    1
);

GO

USE AsiloAncianos;
GO

INSERT INTO Paciente
(
    nombres,
    apellidos,
    fechaNacimiento,
    sexo,
    direccion,
    telefono,
    fechaIngreso,
    estado
)
VALUES
(
    'Carlos',
    'Perez',
    '1945-06-15',
    'Masculino',
    'Mazatenango',
    '55555555',
    '2026-09-03',
    1
);
GO

INSERT INTO Especialidad (nombre, descripcion)
VALUES
('Medicina General', 'Evaluación médica inicial de los pacientes.'),
('Cardiología', 'Atención de enfermedades relacionadas con el corazón.'),
('Geriatría', 'Atención médica especializada para adultos mayores.'),
('Neurología', 'Atención de enfermedades del sistema nervioso.'),
('Traumatología', 'Atención de lesiones y problemas musculoesqueléticos.');

INSERT INTO Medico
(nombres, apellidos, telefono, correoElectronico, idEspecialidad, estado)
VALUES
('Carlos', 'Ramírez', '55551111', 'carlos.ramirez@asilo.com', 1, 1),
('Ana', 'López', '55552222', 'ana.lopez@asilo.com', 3, 1),
('Miguel', 'Pérez', '55553333', 'miguel.perez@asilo.com', 2, 1);

INSERT INTO Enfermero
(nombres, apellidos, telefono, estado)
VALUES
('María', 'González', '55554444', 1),
('José', 'Hernández', '55555555', 1),
('Luisa', 'Morales', '55556666', 1);

INSERT INTO Donante
(
    Nombre,
    tipoDonante,
    telefono,
    CorreoElectronico,
    direccion
)
VALUES
(
    'Ernesto Gonzales',
    'Persona',
    '5555-1001',
    'ernest.gonza@email.com',
    'Guatemala'
),
(
    'Empresa ABC, S.A.',
    'Empresa',
    '5555-2002',
    'contacto@empresaabc.com',
    'Ciudad de Guatemala'
),
(
    'Fundación Esperanza',
    'Fundación',
    '5555-3003',
    'info@fundacionesperanza.org',
    'Guatemala'
);

ALTER TABLE Paciente
ADD motivoIngreso VARCHAR(500) NULL,
    psicopatologia VARCHAR(500) NULL;

ALTER TABLE Donacion
ALTER COLUMN fechaDonacion DATETIME NOT NULL;

ALTER TABLE Donacion
ALTER COLUMN descripcion VARCHAR(500) NULL;
