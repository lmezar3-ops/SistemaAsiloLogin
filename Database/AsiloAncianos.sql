USE [master]
GO
/****** Objeto: Database [AsiloAncianos] Fecha de script: 11/10/2026 11:42:32 ******/
CREATE DATABASE [AsiloAncianos]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'AsiloAncianos', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\AsiloAncianos.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'AsiloAncianos_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\AsiloAncianos_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [AsiloAncianos] SET COMPATIBILITY_LEVEL = 170
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [AsiloAncianos].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [AsiloAncianos] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [AsiloAncianos] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [AsiloAncianos] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [AsiloAncianos] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [AsiloAncianos] SET ARITHABORT OFF 
GO
ALTER DATABASE [AsiloAncianos] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [AsiloAncianos] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [AsiloAncianos] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [AsiloAncianos] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [AsiloAncianos] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [AsiloAncianos] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [AsiloAncianos] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [AsiloAncianos] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [AsiloAncianos] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [AsiloAncianos] SET  ENABLE_BROKER 
GO
ALTER DATABASE [AsiloAncianos] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [AsiloAncianos] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [AsiloAncianos] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [AsiloAncianos] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [AsiloAncianos] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [AsiloAncianos] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [AsiloAncianos] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [AsiloAncianos] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [AsiloAncianos] SET  MULTI_USER 
GO
ALTER DATABASE [AsiloAncianos] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [AsiloAncianos] SET DB_CHAINING OFF 
GO
ALTER DATABASE [AsiloAncianos] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [AsiloAncianos] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [AsiloAncianos] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [AsiloAncianos] SET OPTIMIZED_LOCKING = OFF 
GO
ALTER DATABASE [AsiloAncianos] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [AsiloAncianos] SET QUERY_STORE = ON
GO
ALTER DATABASE [AsiloAncianos] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [AsiloAncianos]
GO
/****** Objeto: User [microservicio] Fecha de script: 11/10/2026 11:42:33 ******/
CREATE USER [microservicio] FOR LOGIN [microservicio] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_datareader] ADD MEMBER [microservicio]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [microservicio]
GO
/****** Objeto: Table [dbo].[CargoPaciente] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CargoPaciente](
	[idCargo] [int] IDENTITY(1,1) NOT NULL,
	[idPaciente] [int] NOT NULL,
	[idFundacion] [int] NOT NULL,
	[fechaCargo] [datetime] NOT NULL,
	[tipoCargo] [varchar](50) NOT NULL,
	[concepto] [varchar](300) NOT NULL,
	[montoOriginal] [decimal](10, 2) NOT NULL,
	[descuento] [decimal](10, 2) NOT NULL,
	[montoFinal] [decimal](10, 2) NOT NULL,
	[estado] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idCargo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[CitaMedica] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CitaMedica](
	[idCita] [int] IDENTITY(1,1) NOT NULL,
	[idSolicitud] [int] NOT NULL,
	[idMedico] [int] NOT NULL,
	[fechaCita] [datetime] NOT NULL,
	[estado] [varchar](50) NOT NULL,
	[observaciones] [varchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[idCita] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[CuotaFamiliar] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CuotaFamiliar](
	[idCuota] [int] IDENTITY(1,1) NOT NULL,
	[idPaciente] [int] NOT NULL,
	[fechaCuota] [date] NOT NULL,
	[monto] [decimal](10, 2) NOT NULL,
	[estado] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idCuota] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Donacion] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Donacion](
	[idDonacion] [int] IDENTITY(1,1) NOT NULL,
	[idDonante] [int] NOT NULL,
	[fechaDonacion] [datetime] NOT NULL,
	[tipoDonacion] [varchar](50) NOT NULL,
	[descripcion] [varchar](500) NULL,
	[monto] [decimal](10, 2) NOT NULL,
 CONSTRAINT [PK_Donacion] PRIMARY KEY CLUSTERED 
(
	[idDonacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Donante] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Donante](
	[idDonante] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](150) NOT NULL,
	[tipoDonante] [varchar](50) NOT NULL,
	[telefono] [varchar](30) NULL,
	[CorreoElectronico] [varchar](150) NULL,
	[direccion] [varchar](250) NULL,
 CONSTRAINT [PK_Donante] PRIMARY KEY CLUSTERED 
(
	[idDonante] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Enfermero] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Enfermero](
	[idEnfermero] [int] IDENTITY(1,1) NOT NULL,
	[nombres] [varchar](100) NOT NULL,
	[apellidos] [varchar](100) NOT NULL,
	[telefono] [varchar](20) NULL,
	[estado] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idEnfermero] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Especialidad] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Especialidad](
	[idEspecialidad] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](100) NOT NULL,
	[descripcion] [varchar](300) NULL,
PRIMARY KEY CLUSTERED 
(
	[idEspecialidad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[ExamenLaboratorio] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExamenLaboratorio](
	[idExamen] [int] IDENTITY(1,1) NOT NULL,
	[idVisita] [int] NOT NULL,
	[nombreExamen] [varchar](150) NOT NULL,
	[fechaSolicitud] [datetime] NOT NULL,
	[estado] [varchar](50) NOT NULL,
	[costo] [decimal](10, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[idExamen] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Familiar] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Familiar](
	[idFamiliar] [int] IDENTITY(1,1) NOT NULL,
	[nombres] [varchar](100) NOT NULL,
	[apellidos] [varchar](100) NOT NULL,
	[parentesco] [varchar](50) NOT NULL,
	[telefono] [varchar](20) NULL,
	[correoElectronico] [varchar](150) NULL,
	[direccion] [varchar](300) NULL,
	[estado] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idFamiliar] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Fundacion] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Fundacion](
	[idFundacion] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](150) NOT NULL,
	[telefono] [varchar](20) NULL,
	[correoElectronico] [varchar](150) NULL,
	[direccion] [varchar](300) NULL,
	[estado] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idFundacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Gasto] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Gasto](
	[idGasto] [int] IDENTITY(1,1) NOT NULL,
	[fechaGasto] [date] NOT NULL,
	[tipoGasto] [varchar](100) NOT NULL,
	[concepto] [varchar](300) NOT NULL,
	[monto] [decimal](10, 2) NOT NULL,
	[observaciones] [varchar](500) NULL,
	[estado] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idGasto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Medicamento] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Medicamento](
	[idMedicamento] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](150) NOT NULL,
	[descripcion] [varchar](300) NULL,
	[existencia] [int] NOT NULL,
	[fechaVencimiento] [date] NULL,
	[costo] [decimal](10, 2) NOT NULL,
	[estado] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idMedicamento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Medico] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Medico](
	[idMedico] [int] IDENTITY(1,1) NOT NULL,
	[nombres] [varchar](100) NOT NULL,
	[apellidos] [varchar](100) NOT NULL,
	[telefono] [varchar](20) NULL,
	[correoElectronico] [varchar](150) NULL,
	[idEspecialidad] [int] NOT NULL,
	[estado] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idMedico] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[MovimientoCaja] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MovimientoCaja](
	[idMovimiento] [int] IDENTITY(1,1) NOT NULL,
	[fechaMovimiento] [datetime] NOT NULL,
	[tipoMovimiento] [varchar](20) NOT NULL,
	[concepto] [varchar](300) NOT NULL,
	[monto] [decimal](10, 2) NOT NULL,
	[observaciones] [varchar](500) NULL,
	[estado] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idMovimiento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Notificacion] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notificacion](
	[idNotificacion] [int] IDENTITY(1,1) NOT NULL,
	[idPaciente] [int] NOT NULL,
	[idFamiliar] [int] NULL,
	[fechaNotificacion] [datetime] NOT NULL,
	[tipo] [varchar](50) NOT NULL,
	[asunto] [varchar](200) NOT NULL,
	[mensaje] [varchar](1000) NOT NULL,
	[estado] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idNotificacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Paciente] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Paciente](
	[idPaciente] [int] IDENTITY(1,1) NOT NULL,
	[nombres] [varchar](100) NOT NULL,
	[apellidos] [varchar](100) NOT NULL,
	[fechaNacimiento] [date] NOT NULL,
	[sexo] [varchar](20) NOT NULL,
	[direccion] [varchar](200) NOT NULL,
	[telefono] [varchar](20) NOT NULL,
	[fechaIngreso] [date] NOT NULL,
	[estado] [bit] NOT NULL,
	[motivoIngreso] [varchar](500) NULL,
	[psicopatologia] [varchar](500) NULL,
	[dpi] [varchar](13) NULL,
PRIMARY KEY CLUSTERED 
(
	[idPaciente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[PacienteFamiliar] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PacienteFamiliar](
	[idPaciente] [int] NOT NULL,
	[idFamiliar] [int] NOT NULL,
	[parentesco] [varchar](50) NOT NULL,
	[esResponsable] [bit] NOT NULL,
 CONSTRAINT [PK_PacienteFamiliar] PRIMARY KEY CLUSTERED 
(
	[idPaciente] ASC,
	[idFamiliar] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[PagoFundacion] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PagoFundacion](
	[idPagoFundacion] [int] IDENTITY(1,1) NOT NULL,
	[idFundacion] [int] NOT NULL,
	[fechaPago] [date] NOT NULL,
	[monto] [decimal](10, 2) NOT NULL,
	[descripcion] [varchar](300) NULL,
	[estado] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idPagoFundacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[PagoPaciente] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PagoPaciente](
	[idPago] [int] IDENTITY(1,1) NOT NULL,
	[idCargo] [int] NULL,
	[fechaPago] [datetime] NOT NULL,
	[montoPagado] [decimal](10, 2) NOT NULL,
	[metodoPago] [varchar](50) NOT NULL,
	[observaciones] [varchar](500) NULL,
	[idCuota] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[idPago] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[ResultadoExamen] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResultadoExamen](
	[idResultado] [int] IDENTITY(1,1) NOT NULL,
	[idExamen] [int] NOT NULL,
	[resultado] [varchar](1000) NOT NULL,
	[fechaResultado] [datetime] NOT NULL,
	[observaciones] [varchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[idResultado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[SolicitudMedica] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SolicitudMedica](
	[idSolicitud] [int] IDENTITY(1,1) NOT NULL,
	[idPaciente] [int] NOT NULL,
	[idMedicoGeneral] [int] NOT NULL,
	[idEspecialidad] [int] NOT NULL,
	[idEnfermero] [int] NULL,
	[fechaSolicitud] [datetime] NOT NULL,
	[motivo] [varchar](500) NOT NULL,
	[estado] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idSolicitud] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Tratamiento] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tratamiento](
	[idTratamiento] [int] IDENTITY(1,1) NOT NULL,
	[idVisita] [int] NOT NULL,
	[idMedicamento] [int] NOT NULL,
	[dosis] [varchar](100) NOT NULL,
	[frecuencia] [varchar](100) NOT NULL,
	[duracion] [varchar](100) NOT NULL,
	[cantidad] [int] NOT NULL,
	[indicaciones] [varchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[idTratamiento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[Usuario] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuario](
	[idUsuario] [int] IDENTITY(1,1) NOT NULL,
	[nombreUsuario] [varchar](50) NOT NULL,
	[contraseña] [varchar](100) NOT NULL,
	[rol] [varchar](50) NOT NULL,
	[estado] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Objeto: Table [dbo].[VisitaMedica] Fecha de script: 11/10/2026 11:42:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VisitaMedica](
	[idVisita] [int] IDENTITY(1,1) NOT NULL,
	[idPaciente] [int] NOT NULL,
	[idMedico] [int] NOT NULL,
	[idCita] [int] NOT NULL,
	[fechaVisita] [datetime] NOT NULL,
	[motivoConsulta] [varchar](500) NOT NULL,
	[diagnostico] [varchar](500) NULL,
	[observaciones] [varchar](1000) NULL,
PRIMARY KEY CLUSTERED 
(
	[idVisita] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Objeto: Index [UX_Paciente_DPI] Fecha de script: 11/10/2026 11:42:33 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Paciente_DPI] ON [dbo].[Paciente]
(
	[dpi] ASC
)
WHERE ([dpi] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CargoPaciente] ADD  DEFAULT (getdate()) FOR [fechaCargo]
GO
ALTER TABLE [dbo].[CargoPaciente] ADD  DEFAULT ((0)) FOR [descuento]
GO
ALTER TABLE [dbo].[CargoPaciente] ADD  DEFAULT ('Pendiente') FOR [estado]
GO
ALTER TABLE [dbo].[CitaMedica] ADD  DEFAULT ('Programada') FOR [estado]
GO
ALTER TABLE [dbo].[CuotaFamiliar] ADD  DEFAULT ('Pendiente') FOR [estado]
GO
ALTER TABLE [dbo].[Enfermero] ADD  DEFAULT ((1)) FOR [estado]
GO
ALTER TABLE [dbo].[ExamenLaboratorio] ADD  DEFAULT (getdate()) FOR [fechaSolicitud]
GO
ALTER TABLE [dbo].[ExamenLaboratorio] ADD  DEFAULT ('Pendiente') FOR [estado]
GO
ALTER TABLE [dbo].[Familiar] ADD  DEFAULT ((1)) FOR [estado]
GO
ALTER TABLE [dbo].[Fundacion] ADD  DEFAULT ((1)) FOR [estado]
GO
ALTER TABLE [dbo].[Gasto] ADD  DEFAULT ('Registrado') FOR [estado]
GO
ALTER TABLE [dbo].[Medicamento] ADD  DEFAULT ((0)) FOR [existencia]
GO
ALTER TABLE [dbo].[Medicamento] ADD  DEFAULT ((0)) FOR [costo]
GO
ALTER TABLE [dbo].[Medicamento] ADD  DEFAULT ((1)) FOR [estado]
GO
ALTER TABLE [dbo].[Medico] ADD  DEFAULT ((1)) FOR [estado]
GO
ALTER TABLE [dbo].[MovimientoCaja] ADD  DEFAULT (getdate()) FOR [fechaMovimiento]
GO
ALTER TABLE [dbo].[MovimientoCaja] ADD  DEFAULT ('Registrado') FOR [estado]
GO
ALTER TABLE [dbo].[Notificacion] ADD  DEFAULT (getdate()) FOR [fechaNotificacion]
GO
ALTER TABLE [dbo].[Notificacion] ADD  DEFAULT ('Pendiente') FOR [estado]
GO
ALTER TABLE [dbo].[PacienteFamiliar] ADD  DEFAULT ((0)) FOR [esResponsable]
GO
ALTER TABLE [dbo].[PagoFundacion] ADD  DEFAULT ('Pagado') FOR [estado]
GO
ALTER TABLE [dbo].[PagoPaciente] ADD  DEFAULT (getdate()) FOR [fechaPago]
GO
ALTER TABLE [dbo].[ResultadoExamen] ADD  DEFAULT (getdate()) FOR [fechaResultado]
GO
ALTER TABLE [dbo].[SolicitudMedica] ADD  DEFAULT (getdate()) FOR [fechaSolicitud]
GO
ALTER TABLE [dbo].[SolicitudMedica] ADD  DEFAULT ('Pendiente') FOR [estado]
GO
ALTER TABLE [dbo].[VisitaMedica] ADD  DEFAULT (getdate()) FOR [fechaVisita]
GO
ALTER TABLE [dbo].[CargoPaciente]  WITH CHECK ADD  CONSTRAINT [FK_CargoPaciente_Fundacion] FOREIGN KEY([idFundacion])
REFERENCES [dbo].[Fundacion] ([idFundacion])
GO
ALTER TABLE [dbo].[CargoPaciente] CHECK CONSTRAINT [FK_CargoPaciente_Fundacion]
GO
ALTER TABLE [dbo].[CargoPaciente]  WITH CHECK ADD  CONSTRAINT [FK_CargoPaciente_Paciente] FOREIGN KEY([idPaciente])
REFERENCES [dbo].[Paciente] ([idPaciente])
GO
ALTER TABLE [dbo].[CargoPaciente] CHECK CONSTRAINT [FK_CargoPaciente_Paciente]
GO
ALTER TABLE [dbo].[CitaMedica]  WITH CHECK ADD  CONSTRAINT [FK_Cita_Medico] FOREIGN KEY([idMedico])
REFERENCES [dbo].[Medico] ([idMedico])
GO
ALTER TABLE [dbo].[CitaMedica] CHECK CONSTRAINT [FK_Cita_Medico]
GO
ALTER TABLE [dbo].[CitaMedica]  WITH CHECK ADD  CONSTRAINT [FK_Cita_Solicitud] FOREIGN KEY([idSolicitud])
REFERENCES [dbo].[SolicitudMedica] ([idSolicitud])
GO
ALTER TABLE [dbo].[CitaMedica] CHECK CONSTRAINT [FK_Cita_Solicitud]
GO
ALTER TABLE [dbo].[CuotaFamiliar]  WITH CHECK ADD  CONSTRAINT [FK_CuotaFamiliar_Paciente] FOREIGN KEY([idPaciente])
REFERENCES [dbo].[Paciente] ([idPaciente])
GO
ALTER TABLE [dbo].[CuotaFamiliar] CHECK CONSTRAINT [FK_CuotaFamiliar_Paciente]
GO
ALTER TABLE [dbo].[Donacion]  WITH CHECK ADD  CONSTRAINT [FK_Donacion_Donante] FOREIGN KEY([idDonante])
REFERENCES [dbo].[Donante] ([idDonante])
GO
ALTER TABLE [dbo].[Donacion] CHECK CONSTRAINT [FK_Donacion_Donante]
GO
ALTER TABLE [dbo].[ExamenLaboratorio]  WITH CHECK ADD  CONSTRAINT [FK_Examen_Visita] FOREIGN KEY([idVisita])
REFERENCES [dbo].[VisitaMedica] ([idVisita])
GO
ALTER TABLE [dbo].[ExamenLaboratorio] CHECK CONSTRAINT [FK_Examen_Visita]
GO
ALTER TABLE [dbo].[Medico]  WITH CHECK ADD  CONSTRAINT [FK_Medico_Especialidad] FOREIGN KEY([idEspecialidad])
REFERENCES [dbo].[Especialidad] ([idEspecialidad])
GO
ALTER TABLE [dbo].[Medico] CHECK CONSTRAINT [FK_Medico_Especialidad]
GO
ALTER TABLE [dbo].[Notificacion]  WITH CHECK ADD  CONSTRAINT [FK_Notificacion_Familiar] FOREIGN KEY([idFamiliar])
REFERENCES [dbo].[Familiar] ([idFamiliar])
GO
ALTER TABLE [dbo].[Notificacion] CHECK CONSTRAINT [FK_Notificacion_Familiar]
GO
ALTER TABLE [dbo].[Notificacion]  WITH CHECK ADD  CONSTRAINT [FK_Notificacion_Paciente] FOREIGN KEY([idPaciente])
REFERENCES [dbo].[Paciente] ([idPaciente])
GO
ALTER TABLE [dbo].[Notificacion] CHECK CONSTRAINT [FK_Notificacion_Paciente]
GO
ALTER TABLE [dbo].[PacienteFamiliar]  WITH CHECK ADD  CONSTRAINT [FK_PacienteFamiliar_Familiar] FOREIGN KEY([idFamiliar])
REFERENCES [dbo].[Familiar] ([idFamiliar])
GO
ALTER TABLE [dbo].[PacienteFamiliar] CHECK CONSTRAINT [FK_PacienteFamiliar_Familiar]
GO
ALTER TABLE [dbo].[PacienteFamiliar]  WITH CHECK ADD  CONSTRAINT [FK_PacienteFamiliar_Paciente] FOREIGN KEY([idPaciente])
REFERENCES [dbo].[Paciente] ([idPaciente])
GO
ALTER TABLE [dbo].[PacienteFamiliar] CHECK CONSTRAINT [FK_PacienteFamiliar_Paciente]
GO
ALTER TABLE [dbo].[PagoFundacion]  WITH CHECK ADD  CONSTRAINT [FK_PagoFundacion_Fundacion] FOREIGN KEY([idFundacion])
REFERENCES [dbo].[Fundacion] ([idFundacion])
GO
ALTER TABLE [dbo].[PagoFundacion] CHECK CONSTRAINT [FK_PagoFundacion_Fundacion]
GO
ALTER TABLE [dbo].[PagoPaciente]  WITH CHECK ADD  CONSTRAINT [FK_PagoPaciente_Cargo] FOREIGN KEY([idCargo])
REFERENCES [dbo].[CargoPaciente] ([idCargo])
GO
ALTER TABLE [dbo].[PagoPaciente] CHECK CONSTRAINT [FK_PagoPaciente_Cargo]
GO
ALTER TABLE [dbo].[PagoPaciente]  WITH CHECK ADD  CONSTRAINT [FK_PagoPaciente_Cuota] FOREIGN KEY([idCuota])
REFERENCES [dbo].[CuotaFamiliar] ([idCuota])
GO
ALTER TABLE [dbo].[PagoPaciente] CHECK CONSTRAINT [FK_PagoPaciente_Cuota]
GO
ALTER TABLE [dbo].[ResultadoExamen]  WITH CHECK ADD  CONSTRAINT [FK_ResultadoExamen_Examen] FOREIGN KEY([idExamen])
REFERENCES [dbo].[ExamenLaboratorio] ([idExamen])
GO
ALTER TABLE [dbo].[ResultadoExamen] CHECK CONSTRAINT [FK_ResultadoExamen_Examen]
GO
ALTER TABLE [dbo].[SolicitudMedica]  WITH CHECK ADD  CONSTRAINT [FK_Solicitud_Enfermero] FOREIGN KEY([idEnfermero])
REFERENCES [dbo].[Enfermero] ([idEnfermero])
GO
ALTER TABLE [dbo].[SolicitudMedica] CHECK CONSTRAINT [FK_Solicitud_Enfermero]
GO
ALTER TABLE [dbo].[SolicitudMedica]  WITH CHECK ADD  CONSTRAINT [FK_Solicitud_Especialidad] FOREIGN KEY([idEspecialidad])
REFERENCES [dbo].[Especialidad] ([idEspecialidad])
GO
ALTER TABLE [dbo].[SolicitudMedica] CHECK CONSTRAINT [FK_Solicitud_Especialidad]
GO
ALTER TABLE [dbo].[SolicitudMedica]  WITH CHECK ADD  CONSTRAINT [FK_Solicitud_MedicoGeneral] FOREIGN KEY([idMedicoGeneral])
REFERENCES [dbo].[Medico] ([idMedico])
GO
ALTER TABLE [dbo].[SolicitudMedica] CHECK CONSTRAINT [FK_Solicitud_MedicoGeneral]
GO
ALTER TABLE [dbo].[SolicitudMedica]  WITH CHECK ADD  CONSTRAINT [FK_Solicitud_Paciente] FOREIGN KEY([idPaciente])
REFERENCES [dbo].[Paciente] ([idPaciente])
GO
ALTER TABLE [dbo].[SolicitudMedica] CHECK CONSTRAINT [FK_Solicitud_Paciente]
GO
ALTER TABLE [dbo].[Tratamiento]  WITH CHECK ADD  CONSTRAINT [FK_Tratamiento_Visita] FOREIGN KEY([idVisita])
REFERENCES [dbo].[VisitaMedica] ([idVisita])
GO
ALTER TABLE [dbo].[Tratamiento] CHECK CONSTRAINT [FK_Tratamiento_Visita]
GO
ALTER TABLE [dbo].[VisitaMedica]  WITH CHECK ADD  CONSTRAINT [FK_Visita_Cita] FOREIGN KEY([idCita])
REFERENCES [dbo].[CitaMedica] ([idCita])
GO
ALTER TABLE [dbo].[VisitaMedica] CHECK CONSTRAINT [FK_Visita_Cita]
GO
ALTER TABLE [dbo].[VisitaMedica]  WITH CHECK ADD  CONSTRAINT [FK_Visita_Medico] FOREIGN KEY([idMedico])
REFERENCES [dbo].[Medico] ([idMedico])
GO
ALTER TABLE [dbo].[VisitaMedica] CHECK CONSTRAINT [FK_Visita_Medico]
GO
ALTER TABLE [dbo].[VisitaMedica]  WITH CHECK ADD  CONSTRAINT [FK_Visita_Paciente] FOREIGN KEY([idPaciente])
REFERENCES [dbo].[Paciente] ([idPaciente])
GO
ALTER TABLE [dbo].[VisitaMedica] CHECK CONSTRAINT [FK_Visita_Paciente]
GO
ALTER TABLE [dbo].[PagoPaciente]  WITH CHECK ADD  CONSTRAINT [CK_PagoPaciente_Obligacion] CHECK  (([idCargo] IS NOT NULL AND [idCuota] IS NULL OR [idCargo] IS NULL AND [idCuota] IS NOT NULL))
GO
ALTER TABLE [dbo].[PagoPaciente] CHECK CONSTRAINT [CK_PagoPaciente_Obligacion]
GO
USE [master]
GO
ALTER DATABASE [AsiloAncianos] SET  READ_WRITE 
GO
