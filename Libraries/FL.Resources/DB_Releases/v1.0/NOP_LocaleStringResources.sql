/*****************************************************************************
* FileName:	 NOP_LocaleStringResources.sql
******************************************************************************
* Author:	Irvine Software Company
*
* Purpose:	insert new resources
*
******************************************************************************
* Changes:
* Date      Developer		PTrack  Description
* ---------- -------------	------  ----------------------------------------- 
* 11/21/2016 y. Chan		TBD 	Initial Release
******************************************************************************/

UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'Quote Request'
WHERE [ResourceName] = 'pagetitle.wishlist' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');

UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'Email your Quote Request to a friend'
WHERE [ResourceName] = 'pagetitle.wishlistemailafriend' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');

UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'The product has been added to your Quote Request'
WHERE [ResourceName] = 'products.producthasbeenaddedtothewishlist' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');

UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'The product has been added to your <a href="{0}">Quote Request</a>'
WHERE [ResourceName] = 'products.producthasbeenaddedtothewishlist.link' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');

UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'Added To Your Quote Request'
WHERE [ResourceName] = 'sevenspikes.nopajaxcart.wishlistnotificationboxtitle' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');

UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'Add to Quote Request'
WHERE [ResourceName] = 'shoppingcart.addtowishlist' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');

/*********/
UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'The maximum number of distinct products allowed in the Quote Request is {0}.'
WHERE [ResourceName] = 'shoppingcart.maximumwishlistitems' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');


UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'Quote Request is disabled for this product'
WHERE [ResourceName] = 'shoppingcart.wishlistdisabled' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');


UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'Quote Request'
WHERE [ResourceName] = 'wishlist' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');


UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'Some product(s) from Quote Request could not be moved to the cart for some reasons.'
WHERE [ResourceName] = 'wishlist.addtocart.error' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');


UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'The Quote Request is empty!'
WHERE [ResourceName] = 'wishlist.cartisempty' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');


UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'Email your Quote Request to a friend'
WHERE [ResourceName] = 'wishlist.emailafriend.title' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');


UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'Update Quote Request'
WHERE [ResourceName] = 'wishlist.updatecart' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');


UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'Quote Request of {0}'
WHERE [ResourceName] = 'wishlist.wishlistof' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');


UPDATE [dbo].[LocaleStringResource]
SET [ResourceValue] = 'Your Quote Request URL for sharing'
WHERE [ResourceName] = 'wishlist.yourwishlisturl' and [LanguageID] = (SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English');

INSERT INTO [dbo].[LocaleStringResource] ([LanguageID],[ResourceName],[ResourceValue])
VALUES ((SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English'),'Admin.Catalog.Categories.Fields.Header1','Category H1 Tag');

INSERT INTO [dbo].[LocaleStringResource] ([LanguageID],[ResourceName],[ResourceValue])
VALUES ((SELECT ID FROM [dbo].[Language] WHERE [Name] = 'English'),'Admin.Catalog.Categories.Fields.Header1.Hint','The text shown in H1 Tag for the Category');