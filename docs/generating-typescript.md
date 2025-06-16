# Generating TypeScript files from a Cyrus Application

* Add the `NForza.Cyrus.MSBuild` package to the Cyrus application
  
* In the `*.csproj` file, add the following:

```xml
<PropertyGroup>
    <Cyrus-TypeScriptFolder>./ts</Cyrus-TypeScriptFolder>
    <Cyrus-CleanTypeScriptFolder>true</Cyrus-CleanTypeScriptFolder>
</PropertyGroup>
```

* When building, the `./ts` folder will be cleared first (all files and directories are deleted) and then Cyrus will generate TypeScript interfaces, enums and even SignalR hubs in that folder for Queries, Commands and events in your application.
