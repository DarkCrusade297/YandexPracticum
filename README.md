---
# 🏗 Структура проекта

Решение (`.sln`) состоит из четырёх проектов, каждый из которых отвечает за свой уровень абстракции.

```
EventManagerSystem.sln
│
├── Domain/                        ← ядро, не зависит ни от чего
│   ├── Enums/
│   ├── Exceptions/
│   └── Models/
│       ├── BookingModel.cs
│       └── EventModel.cs
│
├── Application/                   ← зависит только от Domain
│   ├── Common/
│   ├── DTO/
│   ├── Services/
│   └── DependencyInjection.cs
│
├── Infrastructure/                ← зависит от Domain (и Application)
│   ├── DataAccess/
│   │   ├── Configurations/
│   │   │   ├── BookingConfiguration/
│   │   │   └── EventConfiguration/
│   │   ├── Entities/
│   │   │   ├── BookingEntity.cs
│   │   │   └── EventEntity.cs
│   │   ├── Mapper/
│   │   │   ├── BookingMapper.cs
│   │   │   └── EventMapper.cs
│   │   └── AppDbContext.cs
│   ├── Migrations/
│   ├── Repositories/
│   │   ├── Booking/
│   │   └── Event/
│   ├── DependencyInjection.cs
│   └── MigrationExtensions.cs
│
└── EventManagerSystem/            ← точка входа, ASP.NET Core Web API
    ├── Connected Services/
    ├── Properties/
    ├── Controllers/
    ├── Exceptions/
    ├── Middleware/
    ├── appsettings.json
    ├── EventManagerSystem.http
    └── Program.cs
```

## 🎯 Правило зависимостей

Ключевой принцип Clean Architecture — зависимости всегда направлены **внутрь**, к домену, а не наружу:

| Проект | Зависит от | Назначение |
|---|---|---|
| **Domain** | *(ничего)* | Чистая бизнес-логика и модели, без ссылок на EF Core, ASP.NET или любую инфраструктуру |
| **Application** | Domain | Сценарии использования (use cases), DTO для входных/выходных данных |
| **Infrastructure** | Domain, Application | Реализация доступа к данным, внешние интеграции |
| **EventManagerSystem** | Domain, Application, Infrastructure | Composition root — точка сборки всех слоёв в единое приложение |

Такая структура позволяет менять детали реализации (например, PostgreSQL на другую СУБД, или EF Core на другой ORM) в `Infrastructure`, не затрагивая бизнес-логику в `Domain` и `Application`.

---

## 📦 `Domain`

Ядро приложения — не имеет зависимостей ни от одного другого проекта в решении.

- **`Models/`** — доменные модели (`BookingModel`, `EventModel`), инкапсулирующие бизнес-правила через приватные сеттеры и явные доменные методы (например, `UpdateStatus`).
- **`Enums/`** — перечисления состояний домена (`BookingStatus` и др.).
- **`Exceptions/`** — доменные исключения, сигнализирующие о нарушении бизнес-правил.

## 🧠 `Application`

Слой бизнес-логики и сценариев использования, зависит только от `Domain`.

- **`Services/`** — прикладные сервисы, реализующие сценарии работы с бронированиями и событиями.
- **`DTO/`** — объекты передачи данных на границе слоя (входные команды и выходные представления).
- **`Common/`** — общие вспомогательные абстракции и интерфейсы, используемые сервисами.
- **`DependencyInjection.cs`** — регистрирует сервисы `Application` в контейнере DI (extension-метод, вызываемый из `Program.cs`).

## 🗄 `Infrastructure`

Реализация доступа к данным и внешних интеграций, зависит от `Domain` и `Application`.

- **`DataAccess/Entities/`** — отдельные persistence-модели EF Core (`BookingEntity`, `EventEntity`), намеренно отделённые от доменных моделей `Domain/Models`. Это классический паттерн разделения Domain Model / Persistence Model — он защищает домен от деталей ORM.
- **`DataAccess/Configurations/`** — реализации `IEntityTypeConfiguration<TEntity>` для каждой сущности: имена таблиц, ограничения, конвертация enum-значений, индексы.
- **`DataAccess/Mapper/`** — двусторонний маппинг между доменными моделями (`Domain/Models`) и persistence-сущностями (`Entities`), используемый репозиториями на входе и выходе.
- **`DataAccess/AppDbContext.cs`** — контекст EF Core, объединяющий `DbSet<TEntity>` всех сущностей.
- **`Migrations/`** — история миграций EF Core.
- **`Repositories/`** — реализация репозиториев (`Booking`, `Event`) по паттерну **Unit of Work**: методы изменения состояния лишь помещают изменения в `ChangeTracker`, а фактическое сохранение в БД происходит только при явном вызове `SaveChangesAsync()`.
- **`DependencyInjection.cs`** — регистрирует `AppDbContext` и репозитории в DI-контейнере.
- **`MigrationExtensions.cs`** — extension-метод для применения миграций при старте приложения (обёртка над `Database.Migrate()` / `MigrateAsync()`).

## 🌐 `EventManagerSystem` (host / Web API)

Точка входа приложения — **composition root**, где собираются вместе все три внутренних слоя.

- **`Controllers/`** — HTTP-эндпоинты API.
- **`Middleware/`** — сквозная логика конвейера запросов (например, глобальная обработка исключений).
- **`Exceptions/`** — обработка исключений на уровне HTTP (маппинг доменных исключений в коды ответа).
- **`Program.cs`** — конфигурирует DI (вызывает `AddApplication()`, `AddInfrastructure()` из соответствующих проектов), настраивает middleware pipeline, применяет миграции через `MigrationExtensions`.
- **`appsettings.json`** — конфигурация приложения, включая строку подключения к PostgreSQL.
- **`Properties/`** — `launchSettings.json` для локального запуска.
- **`EventManagerSystem.http`** — набор HTTP-запросов для ручного тестирования API прямо из IDE.
---

# Требования для запуска проекта

Для запуска приложения требуется:

- установленный .NET SDK;
- установленный и запущенный PostgreSQL.

Приложение использует PostgreSQL в качестве основной базы данных, поэтому перед запуском необходимо убедиться, что сервер PostgreSQL доступен.

Для запуска интеграционных тестов дополнительно требуется:

- установленный Docker;
- запущенный Docker Engine.

Интеграционные тесты используют Testcontainers и поднимают временный контейнер PostgreSQL.

# Настройка строки подключения к PostgreSQL

Строка подключения к PostgreSQL указывается в конфигурационном файле приложения.

Пример настройки строки подключения:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=eventapi;Username=postgres;Password=postgres"
  }
}
```

# Управление схемой базы данных

Схема базы данных управляется с помощью **миграций Entity Framework Core**.

Миграции позволяют фиксировать изменения модели данных в коде и последовательно применять их к базе данных. Это предпочтительный способ управления схемой БД в проекте.

При старте приложения схема базы данных создаётся и обновляется через механизм миграций EF Core с использованием метода:

```csharp
Database.Migrate();
```

Это означает, что при запуске приложения EF Core применяет все ещё не применённые миграции к PostgreSQL.

## Создание миграции

Для создания новой миграции используйте команду:

```bash
dotnet ef migrations add InitialCreate --project EventManagerSystem
```

Где:

- `InitialCreate` — имя миграции;
- `--project EventManagerSystem` — проект, в котором находится `DbContext`.

Пример создания следующей миграции:

```bash
dotnet ef migrations add AddBookings --project EventManagerSystem
```

## Применение миграций к базе данных

Чтобы применить миграции к PostgreSQL вручную, используйте команду:

```bash
dotnet ef database update --project EventManagerSystem
```

После выполнения команды EF Core создаст или обновит схему базы данных в соответствии с миграциями.

# Запуск и тестирование проекта

## Сборка проекта

Для того, чтобы выполнить сборку проекта, используйте команду:

```bash
dotnet build
```

## Запуск проекта

Для запуска проекта используйте команду:

```bash
dotnet run
```

Перед запуском убедитесь, что:

- PostgreSQL запущен;
- строка подключения настроена корректно.

При запуске приложения схема базы данных будет создана или обновлена через миграции EF Core с помощью `Migrate`.

Также миграции можно применить вручную командой:

```bash
dotnet ef database update --project EventManagerSystem
```

## Запуск тестов

Для запуска тестов используйте команду:

```bash
dotnet test
```

## Интеграционные тесты

Интеграционные тесты выполняются на реальной базе данных PostgreSQL.

Для этого используется **Testcontainers**: во время запуска тестов автоматически создаётся временный Docker-контейнер с PostgreSQL. Тесты подключаются к этой базе данных, применяют миграции EF Core через `MigrateAsync` и проверяют работу приложения с реальной PostgreSQL.

Для запуска интеграционных тестов требуется установленный и запущенный **Docker**.

Перед запуском тестов можно проверить доступность Docker:

```bash
docker --version
```

```bash
docker ps
```

Если Docker не установлен или Docker Engine не запущен, интеграционные тесты завершатся ошибкой.

Во время выполнения интеграционных тестов Testcontainers:

- запускает временный контейнер PostgreSQL;
- создаёт тестовую базу данных;
- применяет миграции EF Core;
- выполняет тесты;
- удаляет контейнер после завершения тестов.
