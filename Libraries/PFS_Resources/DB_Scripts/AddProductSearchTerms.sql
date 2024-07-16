USE [FurnitureLeisureNop37]
GO

/****** Object:  Table [dbo].[SS_FL_ProductSearchTerms]    Script Date: 18.1.2021 г. 18:37:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SS_FL_ProductSearchTerms](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SearchTerm] [nvarchar](4000) NOT NULL,
	[ProductId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

DECLARE @ProductId int

DECLARE PRODUCTS_CURSOR CURSOR 
  LOCAL STATIC READ_ONLY FORWARD_ONLY
FOR 
SELECT DISTINCT [Id] 
FROM [dbo].[Product]

OPEN PRODUCTS_CURSOR
FETCH NEXT FROM PRODUCTS_CURSOR INTO @ProductId
WHILE @@FETCH_STATUS = 0
BEGIN  
	DECLARE @searchTerm Varchar(MAX); 

	SELECT @searchTerm = p.[Name]
	FROM [dbo].[Product] p
	WHERE p.[Id] = @ProductId

	SELECT @searchTerm = COALESCE(@searchTerm + ' ' + pt.[Name], pt.[Name]) 
	FROM [dbo].[Product_ProductTag_Mapping] pptm JOIN [dbo].[ProductTag] pt ON pt.[Id] = pptm.[ProductTag_Id]
	WHERE pptm.[Product_Id] = @ProductId

	INSERT INTO [dbo].[SS_FL_ProductSearchTerms] (ProductId, SearchTerm)
	VALUES (@ProductId, @searchTerm)
	
    FETCH NEXT FROM PRODUCTS_CURSOR INTO @ProductId	
END
CLOSE PRODUCTS_CURSOR
DEALLOCATE PRODUCTS_CURSOR

CREATE UNIQUE INDEX ui_productSearchTerm ON [dbo].[SS_FL_ProductSearchTerms](Id);  
CREATE FULLTEXT INDEX ON [dbo].[SS_FL_ProductSearchTerms](SearchTerm)   
   KEY INDEX ui_productSearchTerm   
   WITH STOPLIST = SYSTEM;  
GO 