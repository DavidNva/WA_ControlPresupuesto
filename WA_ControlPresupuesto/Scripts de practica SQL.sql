USE ControlFinanzas
GO
---UPDATE Transacciones SET Nota = 'Nota Actualizada', Monto = 4780 where Id = 1

--SELECT * FROM Transacciones ORDER BY UsuarioId desc, MONTO desc

--SELECT * FROM Transacciones WHERE UsuarioId IN ('DAVID', 'juan')
--SELECT * FROM Transacciones WHERE UsuarioId LIKE '%da%'--todos los registos con ese da, no importa si esta al principio, al final, en medio
--SELECT * FROM Transacciones WHERE UsuarioId LIKE 'da%' --solo los que empiezan con da
--SELECT * FROM Transacciones WHERE UsuarioId LIKE '%dro' --solo los que terminan con dro
--SELECT * FROM Transacciones WHERE UsuarioId not LIKE '%dro' --todos excepto los que terminan con 'dro' y asi aplica con los demas

--SELECT * FROM Transacciones WHERE YEAR(FechaTransaccion) = 2020
--SELECT * FROM Transacciones WHERE YEAR(FechaTransaccion) = 2024
--SELECT * FROM Transacciones WHERE YEAR(FechaTransaccion) = 2024 AND MONTH(FechaTransaccion) = 8  
--SELECT * FROM Transacciones WHERE YEAR(FechaTransaccion) = 2025 AND MONTH(FechaTransaccion) = 8  

--SELECT * FROM Transacciones WHERE DAY(FechaTransaccion) = 30

--SELECT * FROM Transacciones WHERE MONTO NOT BETWEEN 100 AND 500
SELECT * FROM Transacciones

SELECT SUM(Monto) as Suma, UsuarioId
FROM Transacciones 
GROUP BY UsuarioId


--SELECT SUM(Monto) as Suma, MONTH(FechaTransaccion) as Mes
--FROM Transacciones 
--GROUP BY MONTH(FechaTransaccion) 

SELECT SUM(Monto) as Suma, UsuarioId, TipoTransaccionId
FROM Transacciones 
GROUP BY UsuarioId, TipoTransaccionId
GO

SELECT COUNT(*) AS CONTEO, UsuarioId, AVG(Monto) as Promedio
FROM TRANSACCIONES
--where UsuarioId ='david'
group by UsuarioId


SELECT COUNT(*) AS CONTEO
FROM TRANSACCIONES
group by UsuarioId


GO


select * from TiposOperaciones

select * from Transacciones


select * from TiposOperaciones tp INNER JOIN Transacciones t ON tp.Id = t.TipoOperacionId



-----------------------------------Procedimientos almacenados --------------------
select * from TiposOperaciones tp INNER JOIN Transacciones t ON tp.Id = t.TipoOperacionId



-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE sp_Transacciones_SelectConTipoOperacion
	-- Add the parameters for the stored procedure here
	@fecha DATE
	--<@Param1, sysname, @p1> <Datatype_For_Param1, , int> = <Default_Value_For_Param1, , 0>, 
	--<@Param2, sysname, @p2> <Datatype_For_Param2, , int> = <Default_Value_For_Param2, , 0>
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	select t.Id, t.Monto, t.Nota, tp.Descripcion from TiposOperaciones tp INNER JOIN Transacciones t ON tp.Id = t.TipoOperacionId
	where FechaTransaccion = @fecha
	ORDER BY UsuarioId DESC
    -- Insert statements for procedure here
	--SELECT <@Param1, sysname, @p1>, <@Param2, sysname, @p2>
END
GO



exec sp_Transacciones_SelectConTipoOperacion '2025-08-30';

exec sp_Transacciones_SelectConTipoOperacion '2024-08-30';



SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE sp_Transacciones_Insertar
	-- Add the parameters for the stored procedure here
	@UsuarioId NVARCHAR(450),
	@FechaTransaccion DATETIME,
	@Monto DECIMAL (18,2),
	@TipoOperacionId INT, 
	@Nota NVARCHAR(1000) = NULL
	--<@Param1, sysname, @p1> <Datatype_For_Param1, , int> = <Default_Value_For_Param1, , 0>, 
	--<@Param2, sysname, @p2> <Datatype_For_Param2, , int> = <Default_Value_For_Param2, , 0>
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	INSERT INTO Transacciones (UsuarioId, FechaTransaccion, Monto, TipoOperacionId, Nota)
	VALUES(@UsuarioId, @FechaTransaccion, @Monto, @TipoOperacionId, @Nota)
END
GO
exec sp_Transacciones_Insertar 'felipe', '2025-08-31',799.99,2, 'Nota ejemplo'
exec sp_Transacciones_Insertar 'felipe', '2025-08-31',1300,1

exec sp_Transacciones_SelectConTipoOperacion '2025-08-31'



GO




INSERT INTO [TiposOperaciones] VALUES
('Ingresos'),('Egresos')
go

INSERT INTO [Transacciones] VALUES('david','2025-8-30',1500.99,1,'Esta es una nota de transaccion')
INSERT INTO [Transacciones] VALUES('david','2025-8-30',350.00,2,'Esta es la nota de transaccion 2')
INSERT INTO [Transacciones] VALUES('pedro','2025-8-30',501.00,2,'Esta es la nota de transaccion 3')
INSERT INTO [Transacciones] VALUES('juan','2025-8-30',2000.00,2,'Esta es la nota de transaccion 4')
INSERT INTO [Transacciones] VALUES('david','2025-8-30',200.00,1,'Esta es la nota de transaccion 5')
INSERT INTO [Transacciones] VALUES('pedro','2025-8-30',100.00,1,null);
INSERT INTO [Transacciones] VALUES('pedro','2024-8-30',100.00,1,null);
INSERT INTO [Transacciones] VALUES('pedro','2024-8-30',100.00,1,null);
INSERT INTO [Transacciones] VALUES('juan','2024-8-30',100.00,2,null);
