# Бэкенд чата
Приложение поднимается на 5000 порту, к swagger можно достучить по `https://localhost:5000/swagger`
## Инструкция по запуску для разраюотчика
1. Поднять в докере базу 
``` sh
docker compose up -d
```
2. Установить EF
``` sh
dotnet tool install --global dotnet-ef
```
3. Накатить миграции
``` sh
dotnet ef database update --project src/MathLLMBackend.Presentation
```