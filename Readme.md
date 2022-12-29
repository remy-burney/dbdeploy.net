# dbdeploy.NET

Manages the deployment of numbered change scripts in versioned folders to a SQL, Oracle, or MySQL database, using a simple table in the database to track applied changes.

## Features

* Auto-creates change tracking table with customizable name.
* Tracks user, output, status, and dates of all scripts executed in change log table.
* Supports SQLCMD mode.
* Supports preventing new script execution when previous scripts failed.
* Supports XML configuration for multiple change sets.
* Supports separate schema name for change log table (example: dbo2.ChangeLog).

# Usage

dbdeploy.NET can be used in your .net core project.

It has integrations with the following platforms:

* Command Line
* Powershell
* MSBuild
* NAnt

It also runs in the following modes:

* Direct to database
* SQLCMD
* Combined ouput script to be run

## Code

To use dbdeploy.NET in your .net core project you need to include the nuget package [ `Dbdeploy.Core`](https://www.nuget.org/packages/Dbdeploy.Core)
Then it's as easy as a few lines of codes as per the below example :
```
var dbDeployConfig = new DbDeployConfig()
{
    ConnectionString = "Server=.\SQL2012;Database=DBDEPLOY;Trusted_Connection=True;",
    ScriptDirectory = new DirectoryInfo("C:\MyProject\Database\Scripts")
};
var deployer = new DbDeployer();
deployer.Execute(dbDeployConfig, Console.Out);
```

## Command Line

The standard usage for dbdeploy.NET in a SQL Server environment is shown below:

```
dbdeploy.exe --connectionstring="Server=.\SQL2012;Database=DBDEPLOY;Trusted_Connection=True;" --scriptdirectory="C:\MyProject\Database\Scripts" -usesqlcmd=true
```

The `scriptdirectory` is the location on the file system where a set of numbered scripts to run are located.

### Script Directory Structure

The recommended structure for scripts is a folder that contains multiple sub-folders by application versions number.  Then inside of these folder are scripts prefixed with a number that represents the order they should be applied. An example is shown below:

<pre>
Scripts/
    v1.0/
        001.Initial Schema Creation.sql	
        002.Add Customer Table.sql
        003.Add Geographic Data.sql
    v1.2/
        001.Add Product Table.sql	
        002.Add Stock Column to Product Table.sql
</pre>

The versioned folder system supports up to the standard four numbers in .NET (Example: 1.0.0.0).  All scripts will be executed by version order then script number order.

### Example Output

```
==========================================================
dbdeploy.net 2.0.0.0
Reading change scripts from directory 'C:\MyProject\Database\Scripts'...

Changes currently applied to database:
  v2.0.0.0       1, 2
Scripts available:
  v2.0.0.0       1..3
  v2.0.10.0     1..3
To be applied:
  v2.0.0.0       3
  v2.0.10.0     1..3

Applying change scripts...

v2.0.0.0/003.Add Sold Column.sql (3)
----------------------------------------------------------
Adding nullable Sold Column


v2.0.10.0/001.Add Customer Table.sql (1)
----------------------------------------------------------
Creating Customer table.


v2.0.10.0/002.Add Email Column Table.sql (2)
----------------------------------------------------------
Adding nullable Email Column
Setting Email Column default

(0 rows affected)
Setting Email Column to not nullable


v2.0.10.0/003.Add Customer Data.sql (3)
----------------------------------------------------------
Adding 2 customers

(2 rows affected)
Done adding customers


All scripts applied successfully.
```

### All Command Options
```
  -d, --dbms=VALUE           DBMS type ('mssql', 'mysql' or 'ora')
  -c, --connectionstring=VALUE
                             connection string for database
  -s, --scriptdirectory=VALUE
                             directory containing change scripts (default: .)
  -o, --outputfile=VALUE     output file
  -t, --changelogtablename=VALUE
                             name of change log table to use (default:
                               ChangeLog)
  -a, --autocreatechangelogtable=VALUE
                             automatically creates the change log table if it
                               does not exist (true or false).  Defaults to
                               true.
  -f, --forceupdate=VALUE    forces previously failed scripts to be run again
                               (true or false).  Defaults to false.
  -u, --usesqlcmd=VALUE      runs scripts in SQLCMD mode (true or false).
                               Defaults to false.
  -l, --lastchangetoapply=VALUE
                             sets the last change to apply in the form of
                               folder/scriptnumber (v1.0.0/4).
  -e, --encoding=VALUE       encoding for input and output files (default:
                               UTF-8)
      --templatedirectory=VALUE
                             template directory
      --delimiter=VALUE      delimiter to separate sql statements
      --delimitertype=VALUE  delimiter type to separate sql statements (row
                               or normal)
      --lineending=VALUE     line ending to use when applying scripts direct
                               to db (platform, cr, crlf, lf)
      --config=VALUE         configuration file to use for all settings.
```

### Re-running with Failed Scripts

When a script has failed to execute successfully, it will be tracked in the change log table with a status of 1 (Failure).  On subsequent runs, dbdeploy.NET will show an error, and the output from the previous run until one of the following is done:

* `--forceupdate=true` is specified on the command parameters.
* The failed change log entry is set to a status of 3 (Resolved) in the database.

Example output when a run has failed is shown below:

```
==========================================================
dbdeploy.net 2.0.0.0
Reading change scripts from directory 'C:\MyProject\Database\Scripts'...

The script 'v2.0.10.0/001.Add Customer Table.sql (1)' failed to complete
on a previous run.
You must update the status to Resolved (2), or force updates.

Ouput from the previous run
----------------------------------------------------------
Unable to create object 'Customer'.

The table already exists.
```

# Change Log Table

The table that is automatically created for change tracking has the following schema:

   Column    |  Data Type   | Description
-------------|--------------|--------------------------------------
ChangeId     | INT          | Auto-incrementing unique ID for each change entry
Folder       | VARCHAR(256) | Versioned folder name (Example: v1.0).
ScriptNumber | SMALLINT     | Squential script number within folder.
ScriptName   | VARCHAR(512) | File name including extension.
StartDate    | DATETIME     | Date and time script started.
CompleteDate | DATETIME     | Date and time script ended even if it failed.
AppliedBy    | VARCHAR(128) | User account that ran the script.
ScriptStatus | TINYINT      | 0 = Failure, 1 = Success, 2 = Problem Resolved, 3 = Started
ScriptOutput | VARCHAR(MAX) | Full output of the script execution.

# XML Configured Runs

The command line dbdeploy.NET supports running scripts from an XML configuration.  An example is shown below:

```
dbdeploy.exe --config="C:\MyProject\Database\Scripts\Deployments.config.xml"
```

The XML file can contain multiple runs and change sets as shown below:

```
<?xml version="1.0" encoding="utf-8" ?>
<!-- Multiple deployments can be executed. -->
<config>
  <dbdeployments>
     <!-- Typical SQL Server run. -->
    <dbdeploy
        connectionString="Server=.\SQL2012;Database=DBDEPLOY;Trusted_Connection=True;"
        scriptDirectory="..\Scripts"
        useSqlCmd="true"
        />

    <!-- Run with all options. -->
    <dbdeploy 
        dbms="mysql" 
        connectionString="Server=.\;Initial Catalog=MyDatabase;User Id=MyUser;Password=SomePass01"
        scriptDirectory="Versioned"
        outputFile="Output\dbdeploy.sql"
        changeLogTableName="InstallLog"
        autoCreateChangeLogTable="false"
        forceUpdate="true"
        useSqlCmd="true"
        lastChangeToApply="v1.1/4"
        encoding="UTF-32"
        templateDirectory="Templates"
        delimiter=";"
        delimiterType="row"
        lineEnding="LF"
        />
  </dbdeployments>
</config>
```

Any path specified can be absolute, or relative to the location of the configuration file.

# What About SSDT?

Using SQL Server Developer Tools is still highly valuable with dbdeploy.NET.  It can be used to generate the change scripts in the following way:

1. Setup a SQL Server Database project.
2. Pull latest from source control.
3. Run dbdeploy.NET to make sure your database instance is up to the latest version.
4. Make your changes in SQL Server Management Studio.
5. Run a SQL Compare from your database project to your database instance.
6. Save the script from the compare as your change script.
7. Apply the changes from the SQL Compare to your database project.
8. Check in the database project and the change scripts together.
9. Now the next developer can get latest and make their changes.

# Additional Info

### History of DbDeploy

- Before 2007 : DBDeploy was created using Java language by Graham Tackley, Nick Ashley with some help by Sam Newman. It was designed to be a database refactoring tool in its original implementation.
- 2007, September 25th : DBDeploy.NET v.1.0 officially released. It represents a port of the existing java version into the .NET framework and is more geared towards inclusion in .Net projects. It currently integrates with the NAnt build tool supporting Oracle, MS SQL Server, and MySQL databases. Thanks goes out to Owen Rogers, Troy Gould, Robert Norton and Tim Goodwin, who contributed to the release
- 2009, May 4th : DbDeploy for java 3.0 M1 has been released. This version includes significant refactoring and other updates as
  a result of the move to [google code](http://code.google.com/p/dbdeploy/)
- 2009, Oct 19th : DbDeploy for java 3.0 M2 has been released.
- 2011, Oct 19th : DbDeploy for java 3.0 M3 has been released. It improves behaviour in failure conditions if the script fails. Source code is also available on the [Graham Tackley Github](https://github.com/tackley/dbdeploy)
- 2013, June 5th : Dbdeploy.NET v.2.0 release. The project was continued and maintained by [Bruno Lopes on Github](https://github.com/brunomlopes/dbdeploy.net)
- 2015, Jan 21st : Dbdeploy.NET v.2.2 release. Migration to use .Net Framework 4.5, includes fixes and other upgrades.
- 2022, Dec 29th : Dbdeploy.NET v3.0 release. Migration to use .Net core 7.0 by [Remy Burney on Github](https://github.com/remy-burney/dbdeploy.net)
