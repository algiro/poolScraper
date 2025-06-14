@echo off
echo "Usage with no parameter, it creates 'latest' version, with 'newRelease' parameter it creates a new release tag"
:parse
set version=latest
IF not "%~1"=="newRelease" GOTO endparse

for /f %%a in ('wmic os get localdatetime ^| find "."') do set datetime=%%a
set YYYY=%datetime:~0,4%
set MM=%datetime:~4,2%
set DD=%datetime:~6,2%
set formatted_date=%YYYY%%MM%%DD%
echo Date: %formatted_date%
set version=R.%formatted_date%
:endparse
echo Version: %version%
docker build . -t algiro/pool-scraper:%version%
