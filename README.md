# Event Manager System

Система управления пользователями, событиями и бронированиями, построенная на .NET как набор из трёх микросервисов. Каждый сервис владеет собственной PostgreSQL-базой, а подтверждённые брони передаются из Bookings в Events асинхронно через Kafka.

## Состав системы

| Сервис | Что делает | API на хосте | Собственная база | Порт БД на хосте |
|---|---|---:|---|---:|
| **Users** | Регистрирует пользователей, проверяет учётные данные и выдаёт JWT | `5001` | PostgreSQL `users` | `5432` |
| **Events** | Создаёт и изменяет события, хранит общее и доступное количество мест | `5002` | PostgreSQL `events` | `5433` |
| **Bookings** | Создаёт, показывает, отменяет и фоново подтверждает брони | `5003` | PostgreSQL `bookings` | `5434` |

Дополнительная инфраструктура:

| Компонент | Назначение | Порт на хосте | Порт внутри Docker |
|---|---|---:|---:|
| Kafka | Передача интеграционных событий | `9092` | `29092` |
| Zookeeper | Координация Kafka | — | `2181` |

Сервисы не обращаются к чужим базам данных. Bookings также не вызывает Events по HTTP: уменьшение мест в штатном сценарии выполняется Events после получения Kafka-сообщения.

### Swagger

- Users: <http://localhost:5001/swagger>
- Events: <http://localhost:5002/swagger>
- Bookings: <http://localhost:5003/swagger>

## Структура решения

Каждый сервис разделён на слои Clean Architecture:

```text
EventManagerSystem.sln
├── UserService
│   ├── User.Domain
│   ├── User.Application
│   ├── User.Infrastructure
│   └── User.Presentation
├── EventService
│   ├── Event.Domain
│   ├── Event.Application
│   ├── Event.Infrastructure
│   └── Event.Presentation
├── BookingService
│   ├── Booking.Domain
│   ├── Booking.Application
│   ├── Booking.Infrastructure
│   └── Booking.Presentation
└── Messaging.Contracts
```

### Правило зависимостей

Зависимости направлены внутрь, к доменной модели:

| Слой | Зависит от | Ответственность |
|---|---|---|
| **Domain** | Ни от чего | Доменные модели, перечисления, исключения и бизнес-правила |
| **Application** | Domain | Сценарии использования, DTO и интерфейсы внешних зависимостей |
| **Infrastructure** | Application, Domain | EF Core, PostgreSQL, Kafka, репозитории и реализации интерфейсов |
| **Presentation** | Application, Infrastructure | HTTP API, DI, JWT, Swagger, middleware и запуск приложения |

`Messaging.Contracts` — отдельная библиотека с публичными контрактами сообщений. Она подключена к издателю и подписчику и не содержит внутренних моделей сервисов.

### Domain

Слой содержит чистую бизнес-логику без зависимостей от ASP.NET Core, EF Core и Kafka. Доменные модели защищают инварианты через закрытые сеттеры и методы изменения состояния: например, `BookingModel.UpdateStatus`, `EventModel.BookSeat` и `EventModel.ReleaseSeat`.

### Application

Слой реализует сценарии работы сервиса:

- DTO для входных и выходных данных;
- прикладные сервисы;
- интерфейсы репозиториев и издателей сообщений;
- регистрацию прикладных зависимостей.

Именно здесь Bookings определяет абстракцию издателя `BookingConfirmed`, не привязывая бизнес-логику к конкретному Kafka-клиенту.

### Infrastructure

Слой содержит технические реализации:

- `DbContext`, persistence-сущности и конфигурации EF Core;
- репозитории и маппинг между persistence- и domain-моделями;
- миграции PostgreSQL;
- Kafka producer в Bookings;
- Kafka consumer, создание топика и Inbox в Events.

Репозитории работают как Unit of Work: изменения фиксируются вызовом `SaveChangesAsync`. Миграции автоматически применяются при старте соответствующего API.

### Presentation

Точка входа каждого сервиса:

- настраивает DI и middleware;
- публикует HTTP-контроллеры;
- проверяет JWT в Events и Bookings;
- включает Swagger в окружении `Development`;
- применяет миграции перед началом обработки запросов и фоновых задач.

## HTTP API

Все примеры ниже используют адреса Docker Compose. Поля enum передаются и возвращаются строками: например, `User`, `Admin`, `Pending` и `Confirmed`.

### Общие коды ошибок

| Код | Значение |
|---:|---|
| `400 Bad Request` | Некорректное тело запроса, параметры или учётные данные |
| `401 Unauthorized` | JWT отсутствует, просрочен или не прошёл проверку |
| `403 Forbidden` | Пользователь аутентифицирован, но не имеет нужной роли или доступа к ресурсу |
| `404 Not Found` | Событие или бронь не найдены |
| `409 Conflict` | Конфликт бизнес-правил: повторный логин, лимит активных броней или недопустимая отмена |
| `500 Internal Server Error` | Неожиданная необработанная ошибка |

Доменные ошибки возвращаются в формате `ProblemDetails`, например:

```json
{
  "status": 404,
  "detail": "Event with id '...' not found"
}
```

## Users API

Users отвечает за регистрацию и выдачу JWT. Его эндпоинты не требуют авторизации.

| Метод и путь | Назначение | Успешный ответ | Основные ошибки |
|---|---|---|---|
| `POST /auth/register` | Регистрация пользователя | `201 Created` | `400`, `409`, `500` |
| `POST /auth/login` | Проверка логина и пароля, выдача JWT | `200 OK` | `400`, `500` |

### Регистрация

```http
POST http://localhost:5001/auth/register
Content-Type: application/json
```

```json
{
  "login": "demo-admin",
  "password": "Demo-Password-123!",
  "role": "Admin"
}
```

Успешный ответ — `201 Created` без тела. Если логин занят, сервис вернёт `409 Conflict`. Текущий контракт позволяет явно передать роль `User` или `Admin`.

### Вход

```http
POST http://localhost:5001/auth/login
Content-Type: application/json
```

```json
{
  "login": "demo-admin",
  "password": "Demo-Password-123!"
}
```

Ответ `200 OK`:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

Неверные учётные данные приводят к `400 Bad Request`.

## Events API

Чтение событий доступно без авторизации. Создавать, изменять и удалять события может только пользователь с ролью `Admin`.

| Метод и путь | Авторизация | Назначение | Успешный ответ | Основные ошибки |
|---|---|---|---|---|
| `GET /events` | Не требуется | Список с фильтрами и пагинацией | `200 OK` | `500` |
| `GET /events/{id}` | Не требуется | Получение события | `200 OK` | `404`, `500` |
| `POST /events` | Admin | Создание события | `201 Created` | `400`, `401`, `403`, `500` |
| `PUT /events/{id}` | Admin | Изменение события | `204 No Content` | `400`, `401`, `403`, `404`, `500` |
| `DELETE /events/{id}` | Admin | Удаление события | `204 No Content` | `401`, `403`, `404`, `500` |

`GET /events` поддерживает параметры `title`, `from`, `to`, `page` и `pageSize`. Значения пагинации по умолчанию: `page=1`, `pageSize=10`.

### Кеширование события по идентификатору

Для `GET /events/{id}` используется паттерн Cache-Aside. Событие хранится в Redis по ключу `event:{id}`, а время жизни записи задаётся параметром `Redis:EventTtlMinutes`.

При чтении Events сначала запрашивает значение из Redis. Если ключ отсутствует или содержит некорректный JSON, сервис получает событие из PostgreSQL и сохраняет актуальный DTO в кеш с настроенным TTL.

При изменениях источником истины остаётся PostgreSQL. Операция сначала фиксируется вызовом `SaveChangesAsync`, и только после этого изменяется кеш:

- `PUT /events/{id}`, резервирование и освобождение мест перезаписывают `event:{id}` актуальными данными;
- `DELETE /events/{id}` удаляет ключ;
- обработчик Kafka-сообщения `BookingConfirmed` инвалидирует ключ после сохранения изменений и фиксации транзакции. Следующий GET заново заполнит кеш из базы данных.

Redis используется только как необязательный кеш. Ошибки чтения, записи и удаления логируются как предупреждения и не возвращаются клиенту: при недоступном Redis запрос выполняется через PostgreSQL. Соединение Redis автоматически восстанавливается после возвращения сервера.

### Создание события

```http
POST http://localhost:5002/events
Authorization: Bearer <admin-jwt>
Content-Type: application/json
```

```json
{
  "title": "Backend Meetup",
  "description": "Kafka and microservices",
  "startAt": "2026-09-01T18:00:00Z",
  "endAt": "2026-09-01T21:00:00Z",
  "totalSeats": 100
}
```

Ответ `201 Created`:

```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "title": "Backend Meetup",
  "description": "Kafka and microservices",
  "startAt": "2026-09-01T18:00:00Z",
  "endAt": "2026-09-01T21:00:00Z",
  "totalSeats": 100,
  "availableSeats": 100
}
```

### Получение списка

```http
GET http://localhost:5002/events?title=Backend&page=1&pageSize=10
```

Ответ `200 OK`:

```json
{
  "total": 1,
  "events": [
    {
      "id": "11111111-1111-1111-1111-111111111111",
      "title": "Backend Meetup",
      "description": "Kafka and microservices",
      "startAt": "2026-09-01T18:00:00Z",
      "endAt": "2026-09-01T21:00:00Z",
      "totalSeats": 100,
      "availableSeats": 100
    }
  ],
  "currentPage": 1,
  "pageSize": 10
}
```

## Bookings API

Все эндпоинты Bookings требуют JWT. Обычный пользователь может работать только со своими бронями; Admin может получать и отменять чужие.

| Метод и путь | Назначение | Успешный ответ | Основные ошибки |
|---|---|---|---|
| `POST /events/{eventId}/book` | Создание брони | `202 Accepted` | `401`, `409`, `500` |
| `GET /bookings/{id}` | Получение брони | `200 OK` | `401`, `403`, `404`, `500` |
| `DELETE /bookings/{id}` | Отмена брони | `204 No Content` | `401`, `403`, `404`, `409`, `500` |

### Создание брони

```http
POST http://localhost:5003/events/11111111-1111-1111-1111-111111111111/book
Authorization: Bearer <user-jwt>
```

Ответ `202 Accepted` содержит заголовок `Location: /bookings/{id}` и бронь в начальном статусе:

```json
{
  "id": "22222222-2222-2222-2222-222222222222",
  "eventId": "11111111-1111-1111-1111-111111111111",
  "userId": "33333333-3333-3333-3333-333333333333",
  "status": "Pending"
}
```

Bookings обрабатывает ожидающие брони фоново. Проверка выполняется раз в 40 секунд, затем перед подтверждением применяется дополнительная задержка 5 секунд.

### Получение подтверждённой брони

```http
GET http://localhost:5003/bookings/22222222-2222-2222-2222-222222222222
Authorization: Bearer <user-jwt>
```

После фоновой обработки ответ `200 OK` выглядит так:

```json
{
  "id": "22222222-2222-2222-2222-222222222222",
  "eventId": "11111111-1111-1111-1111-111111111111",
  "userId": "33333333-3333-3333-3333-333333333333",
  "status": "Confirmed",
  "createdAt": "2026-08-26T10:00:00Z",
  "processedAt": "2026-08-26T10:00:45Z"
}
```

## Аутентификация и авторизация

Users подписывает JWT, а Events и Bookings проверяют подпись, issuer, audience и срок действия. Поэтому все API должны использовать одинаковые значения `JwtSettings:Secret`, `Issuer` и `Audience`.

Роли:

| Роль | Права |
|---|---|
| **User** | Создание брони, получение и отмена собственной брони |
| **Admin** | Права User, управление событиями, получение и отмена любой брони |

Чтобы вызвать защищённый эндпоинт в Swagger:

1. Зарегистрируйте пользователя через Users `POST /auth/register`.
2. Получите токен через Users `POST /auth/login`.
3. Откройте Swagger Events или Bookings.
4. Нажмите **Authorize** и вставьте только значение токена, без префикса `Bearer`.
5. Swagger самостоятельно добавит заголовок `Authorization: Bearer <token>`.

После истечения срока действия токена выполните вход повторно и обновите значение в Swagger.

## Поток BookingConfirmed

Контракт расположен в `Messaging.Contracts` и содержит:

```text
BookingId, EventId, UserId, SeatCount, ConfirmedAt
```

Имя топика также вынесено в общий контракт: `booking-confirmed`.

Поток данных:

1. Bookings создаёт бронь со статусом `Pending`.
2. Фоновый обработчик переводит бронь в `Confirmed` и сначала сохраняет это изменение в базе Bookings.
3. Kafka producer сериализует `BookingConfirmed` в JSON и публикует его в `booking-confirmed`.
4. Ключ сообщения — `EventId`: сообщения одного события попадают в один partition и обрабатываются по порядку.
5. Events использует consumer group `event-service-booking-confirmed-v1` и получает каждое сообщение только одним экземпляром внутри группы.
6. Для сообщения создаётся DI scope, в котором доступны scoped `EventDbContext` и репозиторий.
7. Events проверяет Inbox по `BookingId`, находит событие и вызывает доменное списание `SeatCount` мест.
8. Новое значение `AvailableSeats` и Inbox-запись сохраняются одной PostgreSQL-транзакцией.
9. Kafka offset фиксируется после успешной обработки. При повторной доставке Inbox предотвращает повторное списание.

Если событие отсутствует, мест недостаточно или `SeatCount` некорректен, сообщение логируется и явно пропускается. Повреждённый JSON также пропускается без остановки consumer. При неожиданной технической ошибке offset не подтверждается, consumer возвращается к сообщению и повторяет его после задержки.

Events при запуске пытается создать топик с помощью Kafka AdminClient. Уже существующий топик считается нормальным состоянием, а ошибка создания логируется и не блокирует запуск API.

## Запуск через Docker Compose

### Требования

- Docker Desktop или Docker Engine;
- Docker Compose;
- свободные порты `3000`, `4317`, `5001–5003`, `5432–5434`, `9090`, `9092` и `16686`.

.NET SDK и локальный PostgreSQL для запуска через Compose не требуются: API собираются в Docker, базы поднимаются отдельными контейнерами.

### Настройка JWT-секрета

Создайте `.env` на основе `.env.example`.

PowerShell:

```powershell
Copy-Item .env.example .env
```

Bash:

```bash
cp .env.example .env
```

Заполните переменную секретом длиной не менее 32 байт:

```dotenv
JWT_SECRET=replace-with-a-random-secret-at-least-32-bytes-long
```

Не добавляйте `.env` с реальным секретом в Git.

### Запуск

```bash
docker compose up --build -d
```

При запуске Compose:

- поднимает Zookeeper, Kafka и три PostgreSQL-базы;
- ждёт успешных healthcheck зависимостей;
- запускает три API;
- применяет миграции EF Core;
- создаёт топик `booking-confirmed`, если он отсутствует;
- запускает фоновые обработчики Bookings и Events.

Проверить состояние:

```bash
docker compose ps
```

Посмотреть логи:

```bash
docker compose logs -f
```

Логи Kafka-потока:

```bash
docker compose logs -f booking-api event-api
```

Посмотреть сообщения топика:

```bash
docker compose exec kafka kafka-console-consumer --bootstrap-server localhost:29092 --topic booking-confirmed --from-beginning --property print.key=true --property print.partition=true --property print.offset=true --property key.separator=" | "
```

Остановить просмотр можно сочетанием `Ctrl+C`.

### Остановка

Остановить контейнеры и сохранить данные в Docker volumes:

```bash
docker compose down
```

Удалить контейнеры вместе с данными трёх PostgreSQL-баз и Grafana:

```bash
docker compose down -v
```

> `docker compose down -v` безвозвратно удаляет локальные данные PostgreSQL и состояние Grafana этого Compose-проекта.

## Наблюдаемость

Стек наблюдаемости запускается вместе с основными сервисами по инструкции из раздела [«Запуск через Docker Compose»](#запуск-через-docker-compose).

В систему входят:

- **OpenTelemetry** — собирает трейсы входящих и исходящих HTTP-запросов, запросов EF Core, метрики ASP.NET Core и рантайма .NET;
- **Prometheus** — скрейпит эндпоинт `/metrics` каждого API по настройкам из `prometheus.yml`;
- **Jaeger** — принимает трейсы по OTLP gRPC и отображает их отдельно для `users-service`, `events-service` и `bookings-service`;
- **Grafana** — визуализирует метрики и сохраняет своё состояние в Docker volume `grafana-data`;
- **Serilog** — выводит логи приложений в stdout как отдельные JSON-объекты в формате Compact JSON.

| Инструмент | Адрес | Назначение |
|---|---|---|
| Prometheus | <http://localhost:9090> | Запросы и состояние сбора метрик |
| Jaeger | <http://localhost:16686> | Поиск и просмотр распределённых трейсов |
| Grafana | <http://localhost:3000> | Дашборды и визуализация метрик |
| OTLP gRPC | `localhost:4317` | Приём трейсов Jaeger с хоста |

Для первого входа в Grafana используйте имя пользователя `admin` и пароль `admin`. Источник данных Prometheus автоматически не создаётся: добавьте его в Grafana вручную, указав внутренний адрес `http://prometheus:9090`.

## Миграции EF Core

Каждый сервис владеет собственным `DbContext` и набором миграций. При старте API вызывает `Database.Migrate()`, поэтому ожидающие миграции применяются автоматически.

Для ручного создания миграции нужен .NET SDK и `dotnet-ef`. Примеры:

```bash
dotnet ef migrations add MigrationName --project UserService/User.Infrastructure --startup-project UserService/User.Presentation
dotnet ef migrations add MigrationName --project EventService/Event.Infrastructure --startup-project EventService/Event.Presentation
dotnet ef migrations add MigrationName --project BookingService/Booking.Infrastructure --startup-project BookingService/Booking.Presentation
```

Применение миграции вручную выполняется аналогично командой `dotnet ef database update` с нужными `--project` и `--startup-project`.

## Сборка и тесты

Собрать решение при установленном .NET SDK:

```bash
dotnet build EventManagerSystem.sln
```

Запустить тесты:

```bash
dotnet test EventManagerSystem.sln
```

Интеграционные тесты используют реальный PostgreSQL через Testcontainers. Для них Docker Engine должен быть запущен; временные контейнеры и тестовые базы создаются и удаляются автоматически.
