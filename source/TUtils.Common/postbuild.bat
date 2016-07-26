::

:: %1: project dir
:: %2: debug release
:: %3: TUtils.Common.dll

::**********************************************************
:: copy output to ..\lib
::**********************************************************
del "%1..\..\lib\*.*" /f /q /s
copy "%1bin\%2\%3" "%1..\..\lib\%3"
copy "%1..\external\Log4Net\log4net.dll" "%1..\..\lib\log4net.dll"
