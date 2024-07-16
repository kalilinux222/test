REM
REM	FL_install_v11_Scripts.bat
REM
REM 05/23/2017 y. chan   	initial release. 

REM ************************************************ 
REM 	set up environment variables              *
REM ************************************************ 

REM ********************************************************************************
REM	-d:	database name
REM	-P:	password
REM -S:	server name. not specified here 
REM	-U:	username 
REM
REM ********************************************************************************* 

REM assuming directory structure as follows:
REM	..\db_releases\v1.1
REM	..\Db_Scripts\Functions
REM	..\Db_Scripts\StoredProcs
REM	..\Db_Scripts\Triggers
REM	..\Db_Scripts\Views
REM
REM
REM assuming FL_Install_v11.bat and FL_Install_v11_Scripts.bat are in ..\db_releases\v1.1

REM ************************************************************************************************************
REM 	CREATE/ALTER TABLES, other one time scripts
REM
REM sqlcmd -S "%sqlcmdserver%" -d "%sqlcmddbname%" -U "%sqlcmduser%"  -P "%sqlcmdpassword%" -o logs\alter_tbl_ShoppingCartItem.log -r1 -i alter_tbl_ShoppingCartItem.sql


REM ************************************************************************************************************
REM Views
REM
REM sqlcmd -S "%sqlcmdserver%" -d "%sqlcmddbname%" -U "%sqlcmduser%"  -P "%sqlcmdpassword%" -o logs\cre_vw_aspnet_Applications.log -r1  -i ..\..\db_scripts\Views\cre_vw_aspnet_Applications.sql


REM ************************************************************************************************************
REM Functions
REM
REM sqlcmd -S "%sqlcmdserver%" -d "%sqlcmddbname%" -U "%sqlcmduser%"  -P "%sqlcmdpassword%" -o logs\fn_GetAccountEndingBalanceByAccountIDAndSubAccountType.log -r1  -i ..\..\db_scripts\Functions\fn_GetAccountEndingBalanceByAccountIDAndSubAccountType.sql


REM ************************************************************************************************************
REM Proceduress
REM
REM sqlcmd -S "%sqlcmdserver%" -d "%sqlcmddbname%" -U "%sqlcmduser%"  -P "%sqlcmdpassword%" -o logs\proc_Account_select.log -r1  -i ..\..\db_scripts\StoredProcs\proc_Account_select.sql

sqlcmd -S "%sqlcmdserver%" -d "%sqlcmddbname%" -U "%sqlcmduser%"  -P "%sqlcmdpassword%" -o logs\cre_ProductLoadAllPaged_sp.log -r1  -i ..\..\db_scripts\StoredProcs\cre_ProductLoadAllPaged_sp.sql


REM ************************************************************************************************************
REM TRIGGERS
REM
REM sqlcmd -S "%sqlcmdserver%" -d "%sqlcmddbname%" -U "%sqlcmduser%"  -P "%sqlcmdpassword%" -o logs\trg_SpotPrice.log -r1  -i ..\..\db_scripts\Triggers\trg_SpotPrice.sql


