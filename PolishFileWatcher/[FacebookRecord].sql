USE [PolishData]
GO

/****** Object:  Table [dbo].[FacebookRecord]    Script Date: 4/15/2017 10:12:22 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FacebookRecord](
	[Date] [nvarchar](100) NULL,
	[Firstnames] [nvarchar](100) NULL,
	[Surname] [nvarchar](100) NULL,
	[PoliceDistrict] [nvarchar](1000) NULL,
	[Reason] [nvarchar](max) NULL,
	[Age] [nvarchar](100) NULL,
	[Description] [nvarchar](max) NULL,
	[NameofImagelink] [nvarchar](max) NULL,
	[Facebooklink] [nvarchar](max) NULL,
	[Comment] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


