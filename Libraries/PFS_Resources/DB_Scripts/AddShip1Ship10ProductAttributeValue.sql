/* Add Ship1, Ship10 fields into the ProductAttributeValue table */

ALTER TABLE dbo.ProductAttributeValue Add Ship1 decimal(18, 4) NOT NULL DEFAULT 0.0000;

ALTER TABLE dbo.ProductAttributeValue Add Ship10 decimal(18, 4) NOT NULL DEFAULT 0.0000;