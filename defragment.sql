USE [D:\USERS\MURAD\SPACER\DB\IMAGES.MDF]
GO
ALTER INDEX [PK__tmp_ms_x__607056C0EDA04299] ON [dbo].[Images] REBUILD PARTITION = ALL WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO
