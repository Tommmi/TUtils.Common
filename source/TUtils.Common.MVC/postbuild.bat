::

:: %1: project dir
:: %2: debug release
:: %3: TUtils.Common.MVC.dll

::**********************************************************
:: copy output to ..\lib
::**********************************************************
copy "%1bin\%2\%3" "%1..\..\lib\%3"
