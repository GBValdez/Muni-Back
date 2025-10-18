# Setup rápido — Backend (.NET 8)

1) **SDK & herramientas**
```bash
# Windows
winget install Microsoft.DotNet.SDK.8
# Linux
sudo apt-get install dotnet-sdk-8.0
```

2) **Secretos en desarrollo**
```bash
cd Muni-Back
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=..."
dotnet user-secrets set "keyJwt" "<secreto>"
```

3) **Base de datos**
```bash
dotnet tool restore
dotnet ef database update
```

4) **Ejecutar API**
```bash
dotnet run
# => http://localhost:5003
```
