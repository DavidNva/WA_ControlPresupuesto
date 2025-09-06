
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