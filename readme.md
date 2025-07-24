# Reglamentator

## Оглавление
1. [Архитектура](#архитектура)
2. [Технологии](#технологии)
3. [Запуск проекта](#запуск-проекта)
4. [API и gRPC](#api-и-grpc)
5. [Доменные сущности](#доменные-сущности)
6. [Тестирование](#тестирование)
7. [CI/CD](#cicd)
8. [Мониторинг и логи](#мониторинг-и-логи)

---

## 1. Архитектура

Проект построен по принципам многослойной архитектуры и состоит из следующих основных компонентов:

- **Backend**  
  - `Reglamentator.WebAPI` — gRPC сервер, реализующий бизнес-логику и взаимодействие с клиентами.
  - `Reglamentator.Application` — слой бизнес-логики, сервисы, менеджеры, хелперы.
  - `Reglamentator.Data` — слой доступа к данным, репозитории, контекст EF Core.
  - `Reglamentator.Domain` — доменные сущности и интерфейсы.

- **Bot**  
  - `Reglamentator.Bot` — Telegram-бот, взаимодействующий с backend через gRPC.

- **Shared**  
  - Общие proto-файлы для gRPC.

Взаимодействие между слоями реализовано через DI (Dependency Injection). Для асинхронных задач и напоминаний используется Hangfire.

---

## 2. Технологии

- .NET 8 (C#)
- gRPC
- Entity Framework Core (PostgreSQL)
- Hangfire (планировщик задач)
- AutoMapper
- FluentValidation
- Docker, Docker Compose
- GitHub Actions (CI/CD)

---

## 3. Запуск проекта

### Локально

1. Установите .NET 8 SDK и Docker.
2. Настройте переменные окружения (например, строку подключения к БД).
3. Запустите backend и bot через Docker Compose:

```sh
docker-compose -f docker-compose.backend.yml up --build
docker-compose -f docker-compose.bot.yml up --build
```

### Через Visual Studio

- Откройте `Reglamentator.sln` и выберите нужный проект для запуска.

---

## 4. API и gRPC

Взаимодействие между компонентами реализовано через gRPC.  
Основные сервисы описаны в файле [`reglamentator.proto`](Backend/Reglamentator.WebAPI/Protos/reglamentator.proto):

- **Operation** — операции пользователя (создание, обновление, удаление, история, планирование)
- **Reminder** — напоминания к операциям
- **User** — регистрация пользователей
- **Notification** — стриминг уведомлений

Примеры сообщений и методов см. в proto-файле.

---

## 5. Доменные сущности

- **TelegramUser** — пользователь Telegram
- **Operation** — задача/операция пользователя
- **Reminder** — напоминание к операции
- **OperationInstance** — экземпляр выполнения операции

Каждая сущность описана в проекте [`Reglamentator.Domain`](Backend/Reglamentator.Domain/Entities/).

---

## 6. Тестирование

- Юнит-тесты для бота находятся в [`Reglamentator.Test.Bot`](Tests/Reglamentator.Test.Bot/).
- Для запуска тестов:

```sh
dotnet test Tests/Reglamentator.Test.Bot/Reglamentator.Test.Bot.csproj
```

---

## 7. CI/CD

- Используется GitHub Actions:  
  - Сборка, тестирование и публикация артефактов при каждом push/pull request.
  - Файл workflow: [ci-cd-pipeline](.github/workflows/ci-cd.yml)

---

## 8. Мониторинг и логи

- Логирование реализовано средствами .NET (ILogger).
- Для production рекомендуется интеграция с внешними системами мониторинга (например, Grafana, Prometheus, Sentry).
