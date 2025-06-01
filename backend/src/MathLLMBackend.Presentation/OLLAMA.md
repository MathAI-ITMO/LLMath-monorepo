# Инструкция по использовании Ollama

Чтобы переключиться на использование Ollama вместо OpenAI, откройте файл `appsettings.Secrets.json` в каталоге проекта и замените секцию `"OpenAi"` следующим образом:

```json
"OpenAi": {
  "ChatModel": {
    "Token": "ollama",
    "Url": "http://localhost:11434/v1",
    "Model": "gemma3:12b"
  },
  "SolverModel": {
    "Token": "ollama",
    "Url": "http://localhost:11434/v1",
    "Model": "gemma3:12b"
  }
}
```

После изменения сохраните файл и перезапустите приложение. 