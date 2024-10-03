# IP API Assignment #
This repository contains the .NET source code for the IP Assignment, implementing the following functionalities:
1. Expose a REST API that looks up an IP Address and returns its country information from the DB. If the information does not exist in local DB, it is retrieved from https://ip2c.org and stored. Caching of successful request is applied.
2. Schedule a job that periodically looks up country information of existing IPs in batches from https://ip2c.org and updates the DB.
3. Expose a REST API that receives a country name and returns a report of aggregated information for IPs of this country in the local DB. Without any input, the API returns the information of IPs for all countries.

## Prerequisites ##
1. .NET framework 8.0
2. a running MS SQL Server instance with the databases described by the connection strings below:
   1. IPDB (schema and data provided in the assignement, contains IP Address and country info):
     ```
      "Server=localhost,1433;Database=IPDB;User Id=sa;Password=Adm1n12#;TrustServerCertificate=True;"
     ```
   2. IntegrationDB (Hangfire scheduler execution information will be stored here; schema automatically created on first run):
     ```
     "Server=localhost,1433;Database=IntegrationDB;User Id=sa;Password=Adm1n12#;TrustServerCertificate=True;"
     ```

## How to run ##
You can run the application by cloning the Git repository and executing the following commands in the clone directory:
```
cd Assignment
dotnet run
```
This is a Web server application, therefore it will remain active until you manually terminate it. Logs will be displayed in the console window in which you ran it.

You can also run the unit tests of the application by executing the following commands in the clone directory:
```
cd Assignment.Tests
dotnet test
```
Running the unit tests does not require having a running instance of the application or the DB in the background.

## How to use ##
The easiest way to use the application is by accessing the Swagger UI at: https://localhost:5259/swagger
You can also use the exposed API endpoints at:
* https://localhost:5259/api/ip/{ip}
* https://localhost:5259/api/report/
* https://localhost:5259/api/report/{countryName}

The Hangfire job scheduler dashboard can be accessed at: https://localhost:5259/hangfire

## Notes ##
* The application is using some different settings than the ones specified in the assignment requirements for testing purposes. Those settings are:
  * IP Address lookup cached messages expire after 2 minutes (no original requirement specified)
  * IP Address updates happen in batches of 5 (instead of 100)
  * IP Address update scheduled job has a period of 5 minutes (instead of 1 hour)
  
  To change these values to the ones required in the assignment, you can change the running profile to something other than "_Development_" in [launchSettings.json](https://github.com/parfotis/DotNetTemplate/blob/master/Assignment/Properties/launchSettings.json), or delete [appsettings.Development.json](https://github.com/parfotis/DotNetTemplate/blob/master/Assignment/appsettings.Development.json).
* The application is served over HTTPS with a self-signed certificate. To access it via your browser, you need to manually accept the certificate the first time.
