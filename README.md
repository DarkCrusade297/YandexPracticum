# Event Manager System

Микросервисная система для управления пользователями, событиями и бронированиями. Сервисы хранят данные в отдельных PostgreSQL-базах, а подтверждение брони передаётся из Bookings в Events через Kafka.

## Состав системы

| Компонент | Назначение | Порт на хосте | Порт в Docker |
|---|---|---:|---:|
| Users API | Регистрация, вход и выдача JWT | `5001` | `8080` |
| Events API | Создание событий и управление доступными местами | `5002` | `8080` |
| Bookings API | Создание и фоновое подтверждение броней | `5003` | `8080` |
| Users PostgreSQL | База пользователей `users` | `5432` | `5432` |
| Events PostgreSQL | База событий `events` | `5433` | `5432` |
| Bookings PostgreSQL | База бронирований `bookings` | `5434` | `5432` |
| Kafka | Брокер сообщений | `9092` | `29092` |
| Zookeeper | Координация Kafka | — | `2181` |

У каждого сервиса собственная база данных. Сервисы не обращаются к базам друг друга.

После запуска Swagger доступен отдельно для каждого API:

- Users: <http://localhost:5001/swagger>
- Events: <http://localhost:5002/swagger>
- Bookings: <http://localhost:5003/swagger>

## Поток BookingConfirmed

Bookings и Events используют общий контракт `BookingConfirmed` из проекта `Messaging.Contracts`. Сообщение содержит только необходимые подписчику данные:

- `BookingId` — идентификатор брони;
- `EventId` — идентификатор события;
- `UserId` — идентификатор пользователя;
- `SeatCount` — количество забронированных мест;
- `ConfirmedAt` — момент подтверждения брони.

Обмен сообщением происходит следующим образом:

1. Пользователь создаёт бронь через Bookings API. Новая бронь получает статус `Pending`.
2. Фоновый обработчик Bookings выбирает ожидающие брони и подтверждает их. Проверка выполняется раз в 40 секунд, после чего перед подтверждением есть дополнительная задержка 5 секунд.
3. Bookings сначала сохраняет статус `Confirmed` в собственной базе, затем публикует JSON-сообщение в топик `booking-confirmed`.
4. Ключом Kafka-сообщения служит `EventId`, поэтому брони одного события попадают в один partition и сохраняют порядок обработки.
5. Events подписан на топик в группе `event-service-booking-confirmed-v1`.
6. При получении сообщения Events создаёт отдельный DI scope, проверяет Inbox и уменьшает `AvailableSeats` у соответствующего события.
7. Изменение события и запись обработанного `BookingId` в Inbox сохраняются одной транзакцией. Повторная доставка того же сообщения не уменьшает места второй раз.
8. Kafka offset фиксируется только после успешной обработки либо явного пропуска бизнес-ошибки. Неожиданные технические ошибки приводят к повторной обработке.

Если событие не найдено, мест недостаточно или количество мест некорректно, сообщение пропускается с записью в лог. Повреждённый JSON также логируется и пропускается, не останавливая consumer.

Bookings не вызывает Events по HTTP и не уменьшает места самостоятельно. В штатном сценарии интеграции количество мест изменяет только Kafka consumer сервиса Events.

## Запуск через Docker Compose

### Требования

- установлен и запущен Docker Desktop или Docker Engine;
- доступна команда `docker compose`;
- порты `5001–5003`, `5432–5434` и `9092` свободны.

### 1. Настройте JWT-секрет

Из корня репозитория создайте `.env` на основе примера.

PowerShell:

```powershell
Copy-Item .env.example .env
```

Bash:

```bash
cp .env.example .env
```

Заполните `JWT_SECRET` в созданном файле:

```dotenv
JWT_SECRET=replace-with-a-random-secret-at-least-32-bytes-long
```

Для всех трёх API используется один секрет, чтобы токен, выданный Users, принимался Events и Bookings. Не добавляйте `.env` с реальным секретом в Git.

### 2. Запустите систему

```bash
docker compose up --build -d
```

Compose дождётся готовности PostgreSQL и Kafka перед запуском зависимых API. Миграции EF Core применяются при старте сервисов, а Events создаёт топик `booking-confirmed`, если он ещё не существует.

### 3. Проверьте состояние

```bash
docker compose ps
```

Все API и базы должны иметь состояние `Up`, а Kafka и PostgreSQL — дополнительно `healthy`.

Посмотреть общие логи:

```bash
docker compose logs -f
```

Посмотреть только публикацию и обработку бронирований:

```bash
docker compose logs -f booking-api event-api
```

Посмотреть сообщения топика с ключом, partition и offset:

```bash
docker compose exec kafka kafka-console-consumer --bootstrap-server localhost:29092 --topic booking-confirmed --from-beginning --property print.key=true --property print.partition=true --property print.offset=true --property key.separator=" | "
```

Остановить просмотр сообщений или логов можно сочетанием `Ctrl+C`.

## Остановка

Остановить и удалить контейнеры, сохранив данные PostgreSQL в Docker volumes:

```bash
docker compose down
```

Полностью удалить контейнеры вместе с данными всех трёх баз:

```bash
docker compose down -v
```

> `docker compose down -v` безвозвратно удаляет локальные данные PostgreSQL этого Compose-проекта.
