IF(EXISTS (SELECT NULL FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductReview]') AND type in (N'U')))
BEGIN
	IF( NOT EXISTS (SELECT NULL FROM sys.columns WHERE object_id = object_id(N'[ProductReview]') and NAME='Name'))
	BEGIN
		ALTER TABLE [ProductReview] ADD [Name] nvarchar(MAX) NULL;
	END
END

IF(EXISTS (SELECT NULL FROM sys.objects WHERE object_id = OBJECT_ID(N'[ProductReview]') AND type in (N'U')))
BEGIN
	IF( NOT EXISTS (SELECT NULL FROM sys.columns WHERE object_id = object_id(N'[ProductReview]') and NAME='BusinessName'))
	BEGIN
		ALTER TABLE [ProductReview] ADD [BusinessName] nvarchar(MAX) NULL;
	END
END