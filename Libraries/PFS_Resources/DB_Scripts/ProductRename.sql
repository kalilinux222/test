/* Set product name to be the short description (skip products containing 'delete' in their names) */

DECLARE @ProductId int

DECLARE ProductRename CURSOR 
LOCAL STATIC READ_ONLY FORWARD_ONLY
FOR 
SELECT Id FROM [Product]

OPEN ProductRename
FETCH NEXT FROM ProductRename INTO @ProductId
WHILE @@FETCH_STATUS = 0
BEGIN                                             
    UPDATE [Product] SET [Name] = [ShortDescription] WHERE Name != 'delete' AND Id = @ProductId

    FETCH NEXT FROM ProductRename INTO @ProductId
END
CLOSE ProductRename
DEALLOCATE ProductRename