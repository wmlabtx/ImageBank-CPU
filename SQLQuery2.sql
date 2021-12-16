UPDATE Images SET Vector = 0x
TRUNCATE TABLE Clusters
--DECLARE @id AS int
--DECLARE @nextid AS int
--DECLARE @j AS int
--DECLARE @descriptor binary(128)
--SET @id = 0
--SET @descriptor = 0x
--WHILE @id < 10000
--BEGIN
--	SET @nextid = @id + 1
--	IF @nextid = 10000 SET @nextid = 0
--	INSERT INTO Clusters (Id, Descriptor, Distance, NextId) VALUES (@id, @descriptor, 0, @nextid)
--	SET @id = @id + 1
--END
