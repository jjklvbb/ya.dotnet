# Event Booking — микросервисная система бронирования

Учебный проект на **C# / .NET 10 и ASP.NET Core Web API**. Система управляет пользователями, мероприятиями и бронированиями. Микросервисы используют отдельные базы PostgreSQL и обмениваются событиями через Apache Kafka без прямых HTTP-вызовов друг к другу.

## Состав репозитория

| Каталог | Назначение |
|---|---|
| `src/Services/Users` | Регистрация, вход, выдача JWT |
| `src/Services/Events` | Управление мероприятиями и доступными местами |
| `src/Services/Bookings` | Создание, просмотр и отмена бронирований |
| `src/Shared/Messaging.Contracts` | Контракт `BookingConfirmed` и имя топика |
| `tests` | Тесты актуальных сервисов (xUnit) |
| `legacy` | Код и тесты исходного монолита, не входящие в активное решение |

Каждый сервис состоит из проектов **Domain, Application, Infrastructure и Api** (слой представления), имеет собственные `DbContext`, миграции EF Core и PostgreSQL. Активное решение: `ya.dotnet.slnx`.

## Архитектура и обмен сообщениями

```text
                       ┌─────────────┐
                       │ Users.Api   │ → usersdb
                       │ выдаёт JWT  │
                       └─────────────┘
                              │ JWT
                   ┌──────────┴──────────┐
                   ▼                     ▼
              Events.Api            Bookings.Api
                  │                     │
               eventsdb              bookingsdb
                  ▲                     │
                  │                     │ BookingConfirmed
                  └──── Kafka ◄─────────┘
```

1. **Users** регистрирует пользователя и выдаёт JWT через `POST /auth/login`.
2. **Bookings** создаёт бронь со статусом `Pending`; фоновый обработчик подтверждает её и публикует `BookingConfirmed` в топик `booking-confirmed`.
3. **Events** получает сообщение и уменьшает `AvailableSeats` у соответствующего мероприятия. Обработанные `BookingId` сохраняются для защиты от повторного списания места.

Формат Kafka-сообщения:

```json
{
  "BookingId": "22222222-2222-2222-2222-222222222222",
  "EventId": "11111111-1111-1111-1111-111111111111"
}
```

Ключ сообщения — `EventId`. Оба сервиса используют библиотеку `Messaging.Contracts`. Users выдаёт токены, а Events и Bookings проверяют их по одинаковым `Jwt:Secret`, `Jwt:Issuer` и `Jwt:Audience`.

## Запуск

Необходимы **Docker Desktop** и Docker Compose. Для сборки и тестов вне контейнеров — **.NET SDK 10**.

1. Клонируйте репозиторий и переключитесь на ветку проекта:

   ```bash
   git clone https://github.com/jjklvbb/ya.dotnet.git
   cd ya.dotnet
   git checkout sprint-9
   ```

2. Создайте файл `.env` в корне (он исключён из Git):

   ```env
   JWT_SECRET=replace-with-a-random-secret-longer-than-32-bytes
   ```

   Замените пример собственным секретом не короче 32 байт.

3. Запустите все компоненты:

   ```bash
   docker compose up -d --build
   docker compose ps
   ```

Docker Compose поднимает три API, три PostgreSQL, Kafka и ZooKeeper. Миграции EF Core применяются API при запуске.

| Сервис | Swagger | PostgreSQL (порт хоста) |
|---|---|---|
| Users | http://localhost:5101/swagger | 5433 |
| Events | http://localhost:5102/swagger | 5434 |
| Bookings | http://localhost:5103/swagger | 5435 |

Kafka: `localhost:9092` с хоста и `kafka:29092` внутри Docker.

Остановка без удаления данных: `docker compose down`.

## API и права доступа

| Сервис | Маршруты | Доступ |
|---|---|---|
| Users | `POST /auth/register`, `POST /auth/login` | Публичный |
| Events | `GET /events`, `GET /events/{id}` | Публичный |
| Events | `POST /events`, `PUT /events/{id}`, `DELETE /events/{id}` | Только Admin |
| Bookings | `POST /bookings`, `GET /bookings/{id}`, `DELETE /bookings/{id}` | Требуется JWT |

Для `DELETE /bookings/{id}` сервис проверяет владельца брони или роль Admin.

**Мероприятие** содержит `Id`, `Title`, `Description`, `StartAt`, `EndAt`, `TotalSeats`, `AvailableSeats`. При создании все места доступны; даты начала и окончания передаются в UTC. `GET /events` поддерживает фильтры `title`, `from`, `to` и пагинацию `page`, `pageSize`.

**Бронь** содержит идентификаторы мероприятия и пользователя, статус и даты создания/обработки. Доступные статусы: `Pending`, `Confirmed`, `Rejected`, `Cancelled`. Создание — `POST /bookings` с телом `{"eventId":"<guid>"}`. Для пользователя действует лимит **10 активных бронирований** (`Pending` и `Confirmed`). Идентификатор пользователя берётся из JWT.

В Swagger защищённых API используйте **Authorize** и токен, полученный через Users.

## Проверка работы

1. Через Users зарегистрируйте администратора и выполните вход.
2. С JWT администратора создайте мероприятие через Events с `totalSeats: 10` и будущими датами.
3. Создайте бронь через `POST /bookings`, указав `eventId` мероприятия.
4. Проверьте `GET /bookings/{id}`: статус должен стать `Confirmed`.
5. Проверьте `GET /events/{id}`: количество свободных мест должно уменьшиться **с 10 до 9**.

Это сквозная проверка доставки `BookingConfirmed` через Kafka. Положительный сценарий проверен локально.

## Сборка и тесты

```bash
dotnet build ya.dotnet.slnx
dotnet test ya.dotnet.slnx
```

В активном решении три проекта тестов: `Users.Tests`, `Events.Tests`, `Bookings.Tests` (18 модульных тестов, прошедших при последней проверке). Старые тесты монолита сохранены в `legacy/` и не входят в активное решение.

## Ограничения текущей реализации

- Подтверждение брони в Bookings и списание места в Events выполняются асинхронно. Сейчас нет полноценного согласования результата при отсутствии мероприятия или свободных мест.
- Отмена подтверждённой брони не публикует событие возврата места в Events.
- В Bookings публикация в Kafka и запись статуса в PostgreSQL не атомарны; возможна повторная доставка. Events проверяет `BookingId`, чтобы не списать место повторно.
- Некорректные данные Kafka логируются; сообщение о несуществующем мероприятии или отсутствии мест может повторяться без завершения обработки.
- Обработка некоторых доменных исключений Bookings пока возвращает HTTP 500 вместо соответствующих 403/404/409.

Проект предназначен для учебной демонстрации микросервисной архитектуры; конфигурация Docker и регистрация пользователей с произвольной ролью Admin не предназначены для промышленной эксплуатации.
