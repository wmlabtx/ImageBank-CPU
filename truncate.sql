--TRUNCATE TABLE Images;
--TRUNCATE TABLE Vars;
--INSERT INTO Vars (Id) VALUES (0);
--DBCC SHRINKDATABASE(N'D:\USERS\MURAD\DOCUMENTS\SDB\DB\IMAGES.MDF')
--GO
--UPDATE Images SET Generation = 1;
--UPDATE Images SET Stars = 0;
--UPDATE Images SET Ratio = 0;
--UPDATE Images SET Id = 0;
UPDATE Images SET LastCheck = '20200101';
UPDATE Images SET NextId = 0;
UPDATE Images SET Counter = 0;
UPDATE Images SET Distance = 0;

--UPDATE Images SET Distance = 1;
--UPDATE Images SET LastCheck = '20200208';
--UPDATE Images SET NextHash = '12345678';
--UPDATE Images SET LastFind = '20200223';
--UPDATE Images SET Format = 103;
--UPDATE Images SET Person = 'Pt';
--UPDATE Images SET MagickHash = '';
--UPDATE Images SET History = 0x0;

