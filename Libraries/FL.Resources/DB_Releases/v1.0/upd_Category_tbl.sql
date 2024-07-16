/*****************************************************************************
* FileName:	 upd_Category_tbl.sql
******************************************************************************
* Author:	Irvine Software Company
*
* Purpose:	add new column to the Category table
*
******************************************************************************
* Changes:
* Date      Developer			PTrack  Description
* ---------- ------------------	------ ----------------------------------------
* 11/23/2016 y. Chan			TBD 	Initial Release
* 01/06/2017 y. Chan			TBD		Made Header1 a null column
* 01/09/2019 Nop-Templates.com	TBD		Added Description2
******************************************************************************/

alter table dbo.Category
add Header1 nvarchar(MAX) null;

update dbo.Category
set Header1 = Name;

--alter table dbo.Category
--alter column Header1 nvarchar(400) not null;

ALTER TABLE dbo.Category
ADD Description2 nvarchar(MAX) null;