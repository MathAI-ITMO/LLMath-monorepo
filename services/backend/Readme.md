# Бэкенд чата
Приложение поднимается на 5000 порту, к swagger можно достучить по `https://localhost:5000/swagger`

## API Endpoints

| Метод | Endpoint | Описание | Авторизация |
|-------|----------|----------|-------------|
| **Auth** |
| `POST` | `/api/auth/register` | Регистрация нового пользователя | Нет |
| **User** |
| `GET` | `/api/user/me` | Получить информацию о текущем пользователе | Да |
| **Chat** |
| `POST` | `/api/chat/create` | Создать новый чат (обычный или для решения задачи) | Да |
| `GET` | `/api/chat/get` | Получить список всех чатов пользователя | Да |
| `GET` | `/api/chat/get/{chatId}` | Получить детали чата (включая тип задачи и ссылку на теорию) | Да |
| `POST` | `/api/chat/delete/{id}` | Удалить чат | Да |
| **Messages** |
| `POST` | `/api/message/complete` | Отправить сообщение в чат и получить ответ от LLM | Да |
| `GET` | `/api/message/get-messages-from-chat` | Получить все видимые сообщения из чата | Да |
| **UserTasks** |
| `GET` | `/api/usertasks?taskType={taskType}` | Получить или создать задачи пользователя для указанного типа | Да |
| `POST` | `/api/usertasks/{userTaskId}/start` | Начать задачу (создать/связать с чатом) | Да |
| `POST` | `/api/usertasks/{userTaskId}/complete` | Пометить задачу как решенную | Да |
| **Stats** |
| `GET` | `/api/stats/task-mode-titles` | Получить названия режимов задач | Нет |
| `GET` | `/api/stats/user-stats` | Получить статистику всех пользователей | Нет |
| `GET` | `/api/stats/user-details/{userId}` | Получить детальную информацию о пользователе | Нет |
| **LLM** |
| `POST` | `/api/v1/llm/solve-problem` | Решить математическую задачу с помощью LLM | Нет |
| `POST` | `/api/v1/llm/extract-answer` | Извлечь финальный ответ из готового решения задачи | Нет |
| **Geolin Proxy** |
| `GET` | `/api/v1/geolin-proxy/problem-data?prefix={prefix}&seed={seed}` | Получить данные задачи по префиксу из GeoLin API | Нет |
| `POST` | `/api/v1/geolin-proxy/check-answer-direct` | Проверить правильность ответа на задачу (прямой вызов GeoLin) | Нет |
| **Tasks** |
| `GET` | `/api/tasks/problems?page={page}&size={size}&prefixName={prefixName}` | Получить список задач с пагинацией | Да |

**Легенда:**
- Да - Требуется авторизация
- Нет - Авторизация не требуется

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
``` shReadme.md
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project src/MathLLMBackend.DataAccess --startup-project src/MathLLMBackend.Presentation
```
4. (опционально) Создать пользователя
``` sh
curl -X 'POST' \
  'http://localhost:5000/register' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "admin@gmail.com",
  "password": "Pwd123!"
}'
```