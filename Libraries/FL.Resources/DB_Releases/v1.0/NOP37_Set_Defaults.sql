/*****************************************************************************
* FileName:	 NOP37_Set_Defaults.sql
******************************************************************************
* Author:	Irvine Software Company
*
* Purpose:	set some ISC preferred NOP settings and localizations
*
******************************************************************************
* Changes:
* Date      Developer		PTrack  Description
* ---------- -------------	------  ----------------------------------------- 
* 02/22/2016 y. Chan		TBD 	Initial Release
* 03/01/2016 Y. Chan		TBD		Added PDF Invoice printing custom settings 
******************************************************************************/

-- disable Euro currency
UPDATE [dbo].[Currency]
   SET [Published] = 0
      ,[UpdatedOnUtc] = getdate()
 WHERE [CurrencyCode] = 'EUR'
GO


-- change some default settings
UPDATE [dbo].[Setting]
  SET [Value] = '100'
 WHERE [Name] = 'adminareasettings.defaultgridpagesize' AND [StoreId] = 0
GO

UPDATE [dbo].[Setting]
  SET [Value] = 'SansSerif.ttf'
 WHERE [Name] = 'pdfsettings.fontfilename' AND [StoreId] = 0
GO

UPDATE [dbo].[Setting]
  SET [Value] = 'True'
 WHERE [Name] = 'pdfsettings.letterpagesizeenabled' AND [StoreId] = 0
GO

UPDATE [dbo].[Setting]
  SET [Value] = 'False'
 WHERE [Name] = 'blogsettings.allownotregistereduserstoleavecomments' AND [StoreId] = 0
GO

UPDATE [dbo].[Setting]
  SET [Value] = 'False'
 WHERE [Name] = 'catalogsettings.allowproductviewmodechanging' AND [StoreId] = 0
GO
	
UPDATE [dbo].[Setting]
  SET [Value] = 'True'
 WHERE [Name] = 'catalogsettings.displaytaxshippinginfofooter' AND [StoreId] = 0
GO

UPDATE [dbo].[Setting]
  SET [Value] = 'True'
 WHERE [Name] = 'catalogsettings.productreviewsmustbeapproved' AND [StoreId] = 0
GO

UPDATE [dbo].[Setting]
  SET [Value] = 'False'
 WHERE [Name] = 'catalogsettings.productsbytagallowcustomerstoselectpagesize' AND [StoreId] = 0
GO

UPDATE [dbo].[Setting]
  SET [Value] = 'True'
 WHERE [Name] = 'commonsettings.hideadvertisementsonadminarea' AND [StoreId] = 0
GO

UPDATE [dbo].[Setting]
  SET [Value] = 'True'
 WHERE [Name] = 'commonsettings.sitemapincludeproducts' AND [StoreId] = 0
GO

UPDATE [dbo].[Setting]
  SET [Value] = 'False'
 WHERE [Name] = 'customersettings.dateofbirthenabled' AND [StoreId] = 0
GO	

UPDATE [dbo].[Setting]
  SET [Value] = 'False'
 WHERE [Name] = 'customersettings.genderenabled' AND [StoreId] = 0
GO	

UPDATE [dbo].[Setting]
  SET [Value] = 'False'
 WHERE [Name] = 'customersettings.newslettertickedbydefault' AND [StoreId] = 0
GO	
	
UPDATE [dbo].[Setting]
  SET [Value] = 'True'
 WHERE [Name] = 'customersettings.stateprovinceenabled' AND [StoreId] = 0
GO	
	
UPDATE [dbo].[Setting]
  SET [Value] = 'True'
 WHERE [Name] = 'customersettings.zippostalcodeenabled' AND [StoreId] = 0
GO

UPDATE [dbo].[Setting]
  SET [Value] = 'False'
 WHERE [Name] = 'newssettings.allownotregistereduserstoleavecomments' AND [StoreId] = 0
GO

UPDATE [dbo].[Setting]
  SET [Value] = 'False'
 WHERE [Name] = 'newssettings.enabled' AND [StoreId] = 0
GO
	
UPDATE [dbo].[Setting]
  SET [Value] = 'True'
 WHERE [Name] = 'ordersettings.onepagecheckoutdisplayordertotalsonpaymentinfotab' AND [StoreId] = 0
GO	
	
UPDATE [dbo].[Setting]
  SET [Value] = 'False'
 WHERE [Name] = 'rewardpointssettings.enabled' AND [StoreId] = 0
GO

UPDATE [dbo].[Setting]
  SET [Value] = 'False'
 WHERE [Name] = 'shippingsettings.allowpickupinstore' AND [StoreId] = 0
GO	
	
UPDATE [dbo].[Setting]
  SET [Value] = 'True'
 WHERE [Name] = 'shippingsettings.bypassshippingmethodselectionifonlyone' AND [StoreId] = 0
GO

UPDATE [dbo].[Setting]
  SET [Value] = 'True'
 WHERE [Name] = 'shoppingcartsettings.allowoutofstockitemstobeaddedtowishlist' AND [StoreId] = 0
GO

UPDATE [dbo].[Setting]
  SET [Value] = 'False'
 WHERE [Name] = 'storeinformationsettings.allowcustomertoselecttheme' AND [StoreId] = 0
GO

--Not present in NOP 3.7
--UPDATE [dbo].[Setting]
--  SET [Value] = 'True'
-- WHERE [Name] = 'storeinformationsettings.storeclosedallowforadmins' AND [StoreId] = 0
--GO

-- PDF invoice custom printing
UPDATE [dbo].[Setting]
  SET [Value] = 'FreeSans.ttf'
 WHERE [Name] = 'pdfsettings.fontfilename' AND [StoreId] = 0
GO
	
-- customize some of the Localization strings
UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Inquiry'
 WHERE [ResourceName] = 'contactus.enquiry' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Enter your inquiry'
 WHERE [ResourceName] = 'contactus.enquiry.hint' AND [LanguageId] = (SELECT [Id] FROM .[dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Enter inquiry'
 WHERE [ResourceName] = 'contactus.enquiry.required' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Your inquiry has been sent to the store owner.'
 WHERE [ResourceName] = 'contactus.yourenquiryhasbeensent' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Inquiry'
 WHERE [ResourceName] = 'contactvendor.enquiry' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Enter your inquiry'
 WHERE [ResourceName] = 'contactvendor.enquiry.hint' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Enter inquiry'
 WHERE [ResourceName] = 'contactvendor.enquiry.required' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Your inquiry has been sent to the vendor.'
 WHERE [ResourceName] = 'contactvendor.yourenquiryhasbeensent' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Featured Products'
 WHERE [ResourceName] = 'products.featuredproducts' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Recently Viewed Products'
 WHERE [ResourceName] = 'products.recentlyviewedproducts' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Customer Info'
 WHERE [ResourceName] = 'account.customerinfo' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Order Status'
 WHERE [ResourceName] = 'account.customerorders.orderstatus' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO
	
UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'State / Province'
 WHERE [ResourceName] = 'account.fields.stateprovince' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO	
	
UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'State / Province is required'
 WHERE [ResourceName] = 'account.fields.stateprovince.required' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Log In'
 WHERE [ResourceName] = 'account.login' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO	

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Log In'
 WHERE [ResourceName] = 'account.login.loginbutton' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'My Account'
 WHERE [ResourceName] = 'account.myaccount' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO	

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'My Account'
 WHERE [ResourceName] = 'account.navigation' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO	

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Fax Number'
 WHERE [ResourceName] = 'address.fields.faxnumber' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'First Name'
 WHERE [ResourceName] = 'address.fields.firstname' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Last Name'
 WHERE [ResourceName] = 'address.fields.lastname' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'State / Province'
 WHERE [ResourceName] = 'address.fields.stateprovince' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'State / Province required'
 WHERE [ResourceName] = 'address.fields.stateprovince.required' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'State / Province'
 WHERE [ResourceName] = 'admin.address.fields.stateprovince' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Select State / Province'
 WHERE [ResourceName] = 'admin.address.fields.stateprovince.hint' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'State / Province is required.'
 WHERE [ResourceName] = 'admin.address.fields.stateprovince.required' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Free Shipping'
 WHERE [ResourceName] = 'products.freeshipping' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Manufacturer Part Number'
 WHERE [ResourceName] = 'products.manufacturerpartnumber' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'New Products'
 WHERE [ResourceName] = 'products.newproducts' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Old Price'
 WHERE [ResourceName] = 'products.price.oldprice' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Rental Price'
 WHERE [ResourceName] = 'products.price.rentalprice' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Your Price'
 WHERE [ResourceName] = 'products.price.withdiscount' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'The product has been added to your Shopping Cart'
 WHERE [ResourceName] = 'products.producthasbeenaddedtothecart' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'The product has been added to your <a href="{0}">Shopping Cart</a>'
 WHERE [ResourceName] = 'products.producthasbeenaddedtothecart.link' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Recently Viewed Products'
 WHERE [ResourceName] = 'products.recentlyviewedproducts' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Related Products'
 WHERE [ResourceName] = 'products.relatedproducts' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Shopping Cart'
 WHERE [ResourceName] = 'shoppingcart' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'State / Province'
 WHERE [ResourceName] = 'shoppingcart.estimateshipping.stateprovince' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

UPDATE [dbo].[LocaleStringResource]
  SET [ResourceValue] = 'Unit Price'
 WHERE [ResourceName] = 'shoppingcart.mini.unitprice' AND [LanguageId] = (SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
GO

-- PDF invoice custom printing
INSERT INTO [dbo].[LocaleStringResource]
           ([LanguageId]
           ,[ResourceName]
           ,[ResourceValue])
     VALUES
           ((SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
           ,'pdfinvoice.storeinfo.line1'
           ,'PHONE: (000) 000-0000                        FAX: (000) 000-0000')
GO

INSERT INTO [dbo].[LocaleStringResource]
           ([LanguageId]
           ,[ResourceName]
           ,[ResourceValue])
     VALUES
           ((SELECT [Id] FROM [dbo].[Language] WHERE [Name] = 'English')
           ,'pdfinvoice.storeinfo.line2'
           ,'www.somesite.com         info@somesite.com')
GO