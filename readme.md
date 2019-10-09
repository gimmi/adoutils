![Screenshot](https://github.com/gimmi/adoutils/raw/master/screenshot.jpg)

### Development environment setup

```
docker run -d --rm `
    --name sql `
    -e "ACCEPT_EULA=Y" `
    -e "SA_PASSWORD=Passw0rd" `
   -p 1433:1433 `
   mcr.microsoft.com/mssql/server:2017-latest
```

### Build

```
dotnet pack `
    --configuration Release `
    .\src\ADOUtils\ADOUtils.csproj
```