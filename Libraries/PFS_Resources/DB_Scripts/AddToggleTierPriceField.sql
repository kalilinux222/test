/* Add HideTierPrice field into the Product table */

ALTER TABLE dbo.Product Add HideTierPrice BIT NOT NULL DEFAULT 0;