
USE ControlPresupuestos
GO
CREATE PROCEDURE sp_TiposCuentas_Insertar
	@Nombre NVARCHAR(50),
	@UsuarioId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Orden int;
	SELECT @Orden = COALESCE(MAX(Orden), 0)+1 --COALESCE PERMITE DECIR, SI ES UN VALOR DISTINTO  de null usalo, sino usa el valor despues de la , que en este caso dijimos 0
	FROM TiposCuentas
	WHERE UsuarioId = @UsuarioId


	INSERT INTO TiposCuentas(Nombre, UsuarioId, Orden)
	VALUES(@Nombre, @UsuarioId, @Orden);

	SELECT SCOPE_IDENTITY();
END



go
USE [ControlPresupuestos]

CREATE PROCEDURE [dbo].[sp_Transacciones_Insertar]
	@UsuarioId int,
	@FechaTransaccion date,
	@Monto decimal (18,2),
	@CategoriaId int,
	@CuentaId int, 
	@Nota nvarchar(1000) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO Transacciones(UsuarioId, FechaTransaccion, Monto,CategoriaId, CuentaId, Nota)
	VALUES (@UsuarioId, @FechaTransaccion,ABS(@Monto), @CategoriaId, @CuentaId, @Nota)
	--Tomamos el valor absoluto del monto para tenerlo siempre positivo para operaciones
	UPDATE Cuentas 
	SET Balance += @Monto 
	WHERE Id = @CuentaId

	SELECT SCOPE_IDENTITY();
END

go

CREATE PROCEDURE [dbo].[sp_Transacciones_Actualizar]
	@Id int,
	@FechaTransaccion date,
	@Monto decimal (18,2),
	@MontoAnterior decimal (18,2),
	@CuentaId int,
	@CuentaAnteriorId int, 
	@CategoriaId int,
	@Nota nvarchar(1000) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	--Revertir transacción anterior
	UPDATE Cuentas 
	SET Balance -= @MontoAnterior
	WHERE Id = @CuentaAnteriorId

	--Realizar nueva transacción
	UPDATE Cuentas 
	SET Balance += @Monto 
	WHERE Id = @CuentaId


	UPDATE Transacciones 
	SET Monto = ABS(@Monto),
	FechaTransaccion = @FechaTransaccion,
	@CategoriaId = @CategoriaId, CuentaId = @CuentaId, Nota = @Nota
	WHERE Id = @Id

	SELECT SCOPE_IDENTITY();
END