use master 
GO
CREATE DATABASE ControlPresupuestos
GO
use ControlPresupuestos
GO
CREATE TABLE Usuarios (
Id INT NOT NULL PRIMARY KEY IDENTITY (1,1),
Email NVARCHAR (256) NOT NULL,
EmailNormalizado NVARCHAR(256) NOT NULL,
PasswordHash NVARCHAR (MAX) NOT NULL
)

GO
CREATE TABLE TiposCuentas(
Id INT NOT NULL PRIMARY KEY IDENTITY (1,1),
Nombre NVARCHAR(50) NOT NULL,
UsuarioId INT NOT NULL,
Orden INT NOT NULL,
CONSTRAINT FK_TiposCuentas_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios (Id)
)
GO
CREATE TABLE Cuentas(
Id INT NOT NULL PRIMARY KEY IDENTITY (1,1),
Nombre NVARCHAR(50) NOT NULL,
TipoCuentaId INT NOT NULL,
Balance decimal (8,2) NOT NULL,
Descripcion nvarchar(1000) null,

CONSTRAINT FK_Cuentas_TiposCuentas FOREIGN KEY (TipoCuentaId) REFERENCES TiposCuentas (Id)
)
GO
CREATE TABLE TiposOperaciones(
Id INT NOT NULL PRIMARY KEY IDENTITY (1,1),
Descripcion nvarchar(50)
)
GO
CREATE TABLE Categorias (
Id INT NOT NULL PRIMARY KEY IDENTITY (1,1),
Nombre NVARCHAR(100) NOT NULL,
TipoOperacionId INT NOT NULL, 
UsuarioId INT NOT NULL,
CONSTRAINT FK_Categorias_TiposOperaciones FOREIGN KEY (TipoOperacionId) REFERENCES TiposOperaciones (Id),
CONSTRAINT FK_Categorias_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios (Id)
)
GO
CREATE TABLE Transacciones(
Id INT NOT NULL PRIMARY KEY IDENTITY (1,1),
UsuarioId INT not null, 
FechaTransaccion DATETIME not null DEFAULT GETDATE(), 
Monto DECIMAL(18,2) NOT NULL,-- puede ser un numero de 18 digitos de los cuales 2 decimales. 
Nota NVARCHAR(1000) null,
TipoOperacionId INT NOT NULL,
CuentaId INT NOT NULL,
CategoriaId INT NOT NULL,
CONSTRAINT FK_Transacciones_TiposOperaciones FOREIGN KEY (TipoOperacionId) REFERENCES TiposOperaciones (id),
CONSTRAINT FK_Transacciones_Cuentas FOREIGN KEY (CuentaId) REFERENCES Cuentas (Id),
CONSTRAINT FK_Transacciones_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios (Id),
CONSTRAINT FK_Transacciones_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias (Id),
)
GO
SELECT * FROM USUARIOS