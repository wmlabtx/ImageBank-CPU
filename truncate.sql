﻿TRUNCATE TABLE Images;
GO

--TRUNCATE TABLE Vars;
--INSERT INTO Vars (Id) VALUES (0);
--GO

DBCC SHRINKDATABASE(N'D:\Users\Murad\Documents\Sdb\Db\images.mdf')
GO
