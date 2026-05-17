@echo off
start "RestMock" cmd /c "dotnet run --project RestMock"
timeout /t 5 /nobreak > nul
start http://localhost:5087
