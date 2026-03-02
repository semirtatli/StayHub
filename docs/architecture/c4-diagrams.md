# StayHub — C4 Architecture Diagrams

The [C4 Model](https://c4model.com/) provides a hierarchical set of diagrams for describing software architecture at different levels of abstraction.

- **Level 1 — System Context**: Shows StayHub and its users/external systems
- **Level 2 — Container**: Shows the major deployable units (services, databases, message broker)
- **Level 3 — Component**: Shows the internal structure of a single service

---

## Level 1: System Context Diagram

*"What is StayHub and who uses it?"*

```
                              ┌─────────────────┐
                              │   Guest (User)  │
                              │                 │
                              │ Searches hotels, │
                              │ makes bookings, │
                              │ pays, reviews   │
                              └────────┬────────┘
                                       │
                                       │ HTTPS
                                       ▼
┌─────────────────┐           ┌─────────────────┐           ┌─────────────────┐
│  Hotel Owner    │──────────▶│                 │◀──────────│     Admin       │
│    (User)       │  HTTPS    │    StayHub      │  HTTPS    │    (User)       │
│                 │           │                 │           │                 │
│ Lists hotels,   │           │ Hotel           │           │ Approves hotels,│
│ manages rooms,  │           │ Marketplace     │           │ manages users,  │
│ views bookings  │           │ Platform        │           │ views analytics │
└─────────────────┘           └───────┬─────────┘           └─────────────────┘
                                      │
                          ┌───────────┼───────────┐
                          │           │           │
                          ▼           ▼           ▼
                   ┌──────────┐┌──────────┐┌──────────┐
                   │  Stripe  ││ SendGrid ││  Azure   │
                   │          ││  / SMTP  ││  Blob    │
                   │ Payments ││  Emails  ││  Storage │
                   └──────────┘└──────────┘└──────────┘
```

**Key relationships:**
- **Guest** → StayHub: Browses hotels, creates bookings, makes payments, writes reviews
- **Hotel Owner** → StayHub: Lists hotels, manages rooms/pricing, views booking calendar
- **Admin** → StayHub: Approves hotel listings, manages users, views platform analytics
- **StayHub** → Stripe: Processes payments and refunds
- **StayHub** → SendGrid/SMTP: Sends transactional emails
- **StayHub** → Azure Blob Storage: Stores hotel/room photos

---

## Level 2: Container Diagram

*"What are the major deployable units inside StayHub?"*

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│                                    StayHub Platform                                   │
│                                                                                       │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐  │
│  │                         React Frontend (SPA)                                    │  │
│  │                  Vite · TypeScript · Tailwind · React Query                     │  │
│  │            Served via Nginx container · Port 3000                               │  │
│  └──────────────────────────────────┬──────────────────────────────────────────────┘  │
│                                     │ HTTPS                                           │
│  ┌──────────────────────────────────▼──────────────────────────────────────────────┐  │
│  │                    YARP API Gateway · Port 5000                                 │  │
│  │         Reverse Proxy · Rate Limiting · CORS · Auth Forwarding                 │  │
│  │               Health Check Aggregation · Request Logging                        │  │
│  └────┬─────────┬─────────┬─────────┬─────────┬─────────┬─────────┬───────────────┘  │
│       │         │         │         │         │         │         │                    │
│       ▼         ▼         ▼         ▼         ▼         ▼         ▼                    │
│  ┌────────┐┌────────┐┌────────┐┌────────┐┌────────┐┌────────┐┌────────┐              │
│  │Identity││ Hotel  ││Booking ││Payment ││Review  ││Notify  ││Analyt. │              │
│  │Service ││Service ││Service ││Service ││Service ││Worker  ││Service │              │
│  │ :5101  ││ :5102  ││ :5103  ││ :5104  ││ :5105  ││ :5106  ││ :5107  │              │
│  │        ││        ││        ││        ││        ││        ││        │              │
│  │ASP.NET ││ASP.NET ││ASP.NET ││ASP.NET ││ASP.NET ││Worker  ││ASP.NET │              │
│  │Core API││Core API││Core API││Core API││Core API││Service ││Core API│              │
│  └───┬────┘└───┬────┘└───┬────┘└───┬────┘└───┬────┘└───┬────┘└───┬────┘              │
│      │         │         │         │         │         │         │                    │
│      ▼         ▼         ▼         ▼         ▼         ▼         ▼                    │
│  ┌────────┐┌────────┐┌────────┐┌────────┐┌────────┐┌────────┐┌────────┐              │
│  │Identity││Hotel   ││Booking ││Payment ││Review  ││Notify  ││Analyt. │              │
│  │  DB    ││  DB    ││  DB    ││  DB    ││  DB    ││  DB    ││  DB    │              │
│  │SQL Svr ││SQL Svr ││SQL Svr ││SQL Svr ││SQL Svr ││SQL Svr ││SQL Svr │              │
│  └────────┘└────────┘└────────┘└────────┘└────────┘└────────┘└────────┘              │
│                                                                                       │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐  │
│  │                         RabbitMQ Message Broker                                  │  │
│  │            MassTransit · Pub/Sub · Integration Events · Outbox                  │  │
│  │                  Port 5672 (AMQP) · Port 15672 (Management UI)                  │  │
│  └─────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                       │
│  ┌─────────────────────────────────────────────────────────────────────────────────┐  │
│  │                         Seq Log Aggregator (Dev)                                │  │
│  │                     Structured Log Search & Dashboards                          │  │
│  │                              Port 5341                                          │  │
│  └─────────────────────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────────────────────┘

External:
   Stripe API ←→ Payment Service (webhooks + REST)
   SendGrid   ←→ Notification Worker (SMTP/REST)
   Azure Blob ←→ Hotel Service (photos)
```

**Container descriptions:**

| Container | Technology | Purpose |
|-----------|-----------|---------|
| React Frontend | React 19, Vite, TypeScript | Single-page application served by Nginx |
| API Gateway | ASP.NET Core + YARP | Single entry point, routes to services, cross-cutting concerns |
| Identity Service | ASP.NET Core + Identity | User registration, auth, JWT tokens, roles |
| Hotel Service | ASP.NET Core + EF Core | Hotel CRUD, rooms, search, availability, approval |
| Booking Service | ASP.NET Core + EF Core | Reservation lifecycle, state management, cancellations |
| Payment Service | ASP.NET Core + Stripe.net | Payment processing, webhooks, refunds |
| Review Service | ASP.NET Core + EF Core | Reviews, ratings, aggregation |
| Notification Worker | .NET Worker Service | Consumes events, sends templated emails |
| Analytics Service | ASP.NET Core + EF Core | Read-optimized reporting and KPIs |
| SQL Server (×7) | Microsoft SQL Server | One database per service (logical isolation) |
| RabbitMQ | RabbitMQ 3.13+ | Async messaging between services |
| Seq | Datalust Seq | Centralized structured log search (development) |

---

## Level 3: Component Diagram — Booking Service (Example)

*"What's inside the Booking Service?"*

The Booking Service is shown as an example because it has the richest domain logic (state machines, availability checks, cancellation policies, outbox pattern).

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Booking Service                                      │
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────────┐  │
│  │                        API Layer                                       │  │
│  │                                                                        │  │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌────────────────────┐   │  │
│  │  │ BookingsController│  │ Exception        │  │ Auth Middleware   │   │  │
│  │  │                  │  │ Handling         │  │ (JWT validation)  │   │  │
│  │  │ POST /bookings   │  │ Middleware       │  │                    │   │  │
│  │  │ GET /bookings    │  │                  │  │ Role + Resource    │   │  │
│  │  │ PUT /cancel      │  │ ProblemDetails   │  │ authorization     │   │  │
│  │  │ GET /bookings/pdf│  │ RFC 7807         │  │                    │   │  │
│  │  └────────┬─────────┘  └──────────────────┘  └────────────────────┘   │  │
│  └───────────┼────────────────────────────────────────────────────────────┘  │
│              │ IMediator.Send()                                               │
│  ┌───────────▼────────────────────────────────────────────────────────────┐  │
│  │                     Application Layer                                  │  │
│  │                                                                        │  │
│  │  Commands (Write Side)              Queries (Read Side)                │  │
│  │  ┌──────────────────────┐           ┌──────────────────────┐          │  │
│  │  │ CreateBookingCommand │           │ GetBookingsQuery     │          │  │
│  │  │ + Handler            │           │ + Handler            │          │  │
│  │  │                      │           │                      │          │  │
│  │  │ CancelBookingCommand │           │ GetBookingByIdQuery  │          │  │
│  │  │ + Handler            │           │ + Handler            │          │  │
│  │  │                      │           │                      │          │  │
│  │  │ ConfirmBookingCommand│           │ GetBookingPdfQuery   │          │  │
│  │  │ + Handler            │           │ + Handler            │          │  │
│  │  └──────────────────────┘           └──────────────────────┘          │  │
│  │                                                                        │  │
│  │  Pipeline Behaviors                 Validators                        │  │
│  │  ┌──────────────────────┐           ┌──────────────────────┐          │  │
│  │  │ ValidationBehavior   │           │ CreateBookingValidator│          │  │
│  │  │ LoggingBehavior      │           │ CancelBookingValidator│          │  │
│  │  │ TransactionBehavior  │           └──────────────────────┘          │  │
│  │  └──────────────────────┘                                             │  │
│  └───────────┬────────────────────────────────────────────────────────────┘  │
│              │                                                               │
│  ┌───────────▼────────────────────────────────────────────────────────────┐  │
│  │                       Domain Layer                                     │  │
│  │                                                                        │  │
│  │  Aggregate Root           Value Objects          Domain Events         │  │
│  │  ┌──────────────────┐     ┌────────────────┐     ┌─────────────────┐  │  │
│  │  │ Booking           │     │ DateRange       │     │BookingCreated   │  │  │
│  │  │                   │     │ Money           │     │BookingConfirmed │  │  │
│  │  │ - Status          │     │ GuestDetails    │     │BookingCancelled │  │  │
│  │  │ - CheckIn/Out     │     └────────────────┘     │BookingCompleted │  │  │
│  │  │ - TotalPrice      │                            └─────────────────┘  │  │
│  │  │ - GuestDetails    │     Specifications                              │  │
│  │  │                   │     ┌────────────────┐     Interfaces          │  │
│  │  │ + Confirm()       │     │ ActiveBookings │     ┌─────────────────┐  │  │
│  │  │ + Cancel(policy) │     │ ByGuestSpec    │     │IBookingRepo     │  │  │
│  │  │ + CheckIn()      │     │ ByHotelSpec    │     │IUnitOfWork      │  │  │
│  │  │ + Complete()     │     └────────────────┘     └─────────────────┘  │  │
│  │  └──────────────────┘                                                  │  │
│  └───────────┬────────────────────────────────────────────────────────────┘  │
│              │ implements                                                     │
│  ┌───────────▼────────────────────────────────────────────────────────────┐  │
│  │                    Infrastructure Layer                                 │  │
│  │                                                                        │  │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌────────────────────┐   │  │
│  │  │ BookingDbContext  │  │ BookingRepository│  │ OutboxPublisher   │   │  │
│  │  │                  │  │                  │  │ (Background Job)  │   │  │
│  │  │ EF Core          │  │ Implements       │  │                    │   │  │
│  │  │ Configurations   │  │ IBookingRepo     │  │ Publishes events   │   │  │
│  │  │ Interceptors     │  │                  │  │ from OutboxMessages│   │  │
│  │  └──────────────────┘  └──────────────────┘  └────────────────────┘   │  │
│  │                                                                        │  │
│  │  ┌──────────────────┐  ┌──────────────────┐                           │  │
│  │  │ HotelServiceClient│  │ MassTransit     │                           │  │
│  │  │                  │  │ Consumers        │                           │  │
│  │  │ HTTP client to   │  │                  │                           │  │
│  │  │ Hotel Service    │  │ PaymentCompleted │                           │  │
│  │  │ (availability)   │  │ → Confirm booking│                           │  │
│  │  └──────────────────┘  └──────────────────┘                           │  │
│  └────────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Request Flow: Guest Books a Hotel Room

This sequence shows how a single booking flows through the system:

```
Guest          React        Gateway      Hotel Svc    Booking Svc   Payment Svc   Notification   RabbitMQ
  │              │             │             │             │              │              │            │
  │─ Search ────▶│─ GET ──────▶│─ route ────▶│             │              │              │            │
  │              │◀── rooms ───│◀── rooms ───│             │              │              │            │
  │              │             │             │             │              │              │            │
  │─ Book ──────▶│─ POST ─────▶│─ route ─────────────────▶│              │              │            │
  │              │             │             │◀── check ───│              │              │            │
  │              │             │             │── avail ───▶│              │              │            │
  │              │             │             │             │──publish────────────────────────────────▶│
  │              │             │             │             │  BookingCreated                          │
  │              │◀── booking ─│◀─────────── │◀── 201 ────│              │              │            │
  │              │             │             │             │              │◀── consume ──│◀───────────│
  │              │             │             │             │              │              │            │
  │─ Pay ───────▶│─ POST ─────▶│─ route ────────────────────────────────▶│              │            │
  │              │             │             │             │              │──publish─────────────────▶│
  │              │             │             │             │              │ PaymentCompleted          │
  │              │◀── confirm ─│◀────────────────────────────── 200 ────│              │            │
  │              │             │             │             │◀─ consume ───│              │◀───────────│
  │              │             │             │             │─ Confirm() ─▶│              │            │
  │              │             │             │             │──publish────────────────────────────────▶│
  │              │             │             │             │ BookingConfirmed                         │
  │              │             │             │             │              │              │◀───────────│
  │              │             │             │             │              │              │─ Email ───▶│
  │              │             │             │             │              │              │  (confirm) │
```

---

*These diagrams follow the C4 Model by Simon Brown. They are maintained as text diagrams for version control friendliness. For interactive versions, see the Mermaid diagrams in the `docs/diagrams/` folder.*
