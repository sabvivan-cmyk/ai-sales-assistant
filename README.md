# AiSalesAssistant

Минимальный ASP.NET Core Web API для подготовки ответа клиенту веб-студии и внутренней подсказки менеджеру по допродаже.

## Архитектура и flow

1. `POST /api/assistant` принимает сообщение клиента и валидирует его.
2. `AssistantService` загружает услуги через `IKnowledgeBaseService` и выполняет простой поиск по словам.
3. Если совпадений нет, API сразу возвращает нейтральный ответ и не вызывает LLM.
4. Если услуга найдена, в `ILlmService` передаются только найденная услуга и связанные с ней upsell-услуги.
5. `OpenRouterLlmService` вызывает `POST /chat/completions` и требует ответ по JSON Schema.
6. Результат десериализуется в `AssistantResponse`. При сетевой ошибке, timeout, пустом или некорректном ответе используется безопасный детерминированный fallback из базы знаний.

Основные слои:

- `Controllers` — HTTP endpoints и валидация входных данных;
- `Services/AssistantService` — бизнес-flow и подготовка релевантного контекста;
- `Services/OpenRouterLlmService` — только HTTP-взаимодействие с OpenRouter;
- `Services/KnowledgeBaseService` — чтение локальной базы знаний;
- `Data/knowledge-base.json` — услуги, цены и допустимые связи для upsell.

## OpenRouter

Демонстрационная версия использует бесплатный маршрутизатор `openrouter/free` и endpoint:

```text
https://openrouter.ai/api/v1/chat/completions
```

Несекретные настройки находятся в `appsettings.json`:

```json
{
  "Llm": {
    "Model": "openrouter/free",
    "BaseUrl": "https://openrouter.ai/api/v1",
    "TimeoutSeconds": 30
  }
}
```

API key хранится только в .NET User Secrets:

```powershell
dotnet user-secrets set "Llm:ApiKey" "<YOUR_OPENROUTER_API_KEY>" --project AiSalesAssistant.csproj
```

Проверить наличие настройки без добавления ключа в проект можно командой:

```powershell
dotnet user-secrets list --project AiSalesAssistant.csproj
```

## Запуск

```powershell
dotnet run --project AiSalesAssistant.csproj --launch-profile http
```

После запуска пример реального запроса находится в `AiSalesAssistant.http`. Если OpenRouter недоступен, endpoint всё равно вернёт безопасный ответ из локальной базы знаний.

## Почему JSON и простого поиска достаточно

База знаний содержит всего несколько структурированных услуг. Для такого объёма словарный поиск прозрачен, быстро работает локально и позволяет отправлять LLM только небольшой релевантный контекст. Embeddings и vector database на этапе MVP добавили бы инфраструктуру без заметной практической пользы.

## Меры против галлюцинаций

- LLM получает только найденную услугу и разрешённые связанные upsell-услуги, а не всю базу.
- System prompt запрещает придумывать услуги, цены, сроки и условия.
- Сообщение клиента передаётся как данные и не может отменять системные правила.
- Ответ запрашивается через строгую JSON Schema и проверяется типизированной десериализацией.
- При отсутствии контекста LLM не вызывается.
- При ошибке или некорректном ответе используется текст, составленный непосредственно из базы знаний.

## Ограничения MVP

- Поиск основан на простом совпадении слов и не понимает сложные синонимы или намерения.
- `openrouter/free` выбирает доступную бесплатную модель динамически, поэтому стиль и задержка ответа могут меняться.
- Нет истории диалога, авторизации, базы данных, frontend, embeddings и vector search.
- Нет retry и circuit breaker; настроен только timeout внешнего запроса.

## Тесты

```powershell
dotnet test AiSalesAssistant.slnx
```

Unit-тесты используют fake-сервисы и подменённый `HttpMessageHandler`; реальные запросы к OpenRouter и настоящий API key в тестах не используются.
