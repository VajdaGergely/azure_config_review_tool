# Azure config review tool
## What is this tool?
This is a Windows GUI tool for administer azure testing with AZSK, AZTS tools and manual aproach
## Features
* import azsk and azts scan data
* import Azure resource list from Get-AzResource command output
* view and edit resource related test cases
* open image and docx test case evidences
* edit and save config review status into database
* export results to csv file
## Technical details
* Written in C#
* Uses .NET Framework 4.7.2
* Uses YML test case files (missing from the repository because it is company property)
* Uses SqLite database to store config review results
* Tested on Windows 10 (64bit)
