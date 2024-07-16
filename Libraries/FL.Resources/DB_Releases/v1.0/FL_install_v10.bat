REM
REM	FL_install_v10.bat
REM
REM
REM 10/12/2016	y. chan		TBD	initial release. 
REM
REM

SET /P sqlcmdserver= Enter server name and press enter:
SET /P sqlcmddbname=Enter database name and press enter:
SET /P sqlcmduser= Enter user name and press enter:
SET /P sqlcmdpassword= Enter password and press enter:  


FL_install_v10_scripts.bat > logs\FL_install_v10.log

