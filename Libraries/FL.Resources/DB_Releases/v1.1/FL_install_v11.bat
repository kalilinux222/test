REM
REM	FL_install_v11.bat
REM
REM
REM 05/23/2017	y. chan		initial release. 
REM
REM

SET /P sqlcmdserver= Enter server name and press enter:
SET /P sqlcmddbname=Enter database name and press enter:
SET /P sqlcmduser= Enter user name and press enter:
SET /P sqlcmdpassword= Enter password and press enter:  


FL_install_v11_scripts.bat > logs\FL_install_v11.log

