DROP TABLE [dbo].[WordTranslation]
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WordTranslation](
	[Word] [varchar](256) NOT NULL,
	[Language] [varchar](5) NOT NULL,
	[Translation] [nvarchar](300) COLLATE SQL_Latin1_General_CP850_BIN NOT NULL ,
 CONSTRAINT [PK_WordTranslation] PRIMARY KEY CLUSTERED 
(
	[Word] ASC,
	[Language] ASC,
	[Translation] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


