@echo off
echo Starting Drug Prevention System Docker Build...

echo.
echo === Stopping existing containers ===
docker-compose down

echo.
echo === Removing old images ===
docker rmi drugprevention_api drugprevention_ui 2>nul

echo.
echo === Building and starting services ===
docker-compose up --build -d

echo.
echo === Checking container status ===
docker-compose ps

echo.
echo === Services should be available at: ===
echo API: http://localhost:5000
echo UI:  http://localhost:3000
echo Database: localhost:1433 (SA password: DrugPrevention@2024)

echo.
echo === To view logs, run: ===
echo docker-compose logs -f

echo.
echo Build completed! Press any key to exit...
pause