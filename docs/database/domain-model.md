# StayHub — Domain Model Documentation

## Table of Contents

- [1. Bounded Context Map](#1-bounded-context-map)
- [2. Ubiquitous Language](#2-ubiquitous-language)
- [3. Identity Context](#3-identity-context)
- [4. Hotel Context](#4-hotel-context)
- [5. Booking Context](#5-booking-context)
- [6. Payment Context](#6-payment-context)
- [7. Review Context](#7-review-context)
- [8. Notification Context](#8-notification-context)
- [9. Analytics Context](#9-analytics-context)
- [10. Cross-Context Relationships](#10-cross-context-relationships)

---

## 1. Bounded Context Map

Each bounded context is a microservice with its own domain model, database, and language. The same real-world concept may exist differently in different contexts.

```
┌──────────────────────────────────────────────────────────────────────────────────┐
│                         StayHub Bounded Context Map                              │
│                                                                                  │
│                          ┌─────────────────┐                                     │
│                          │    Identity      │                                     │
│                          │    Context       │                                     │
│                          │                  │                                     │
│                          │  User, Role,     │                                     │
│                          │  RefreshToken    │                                     │
│                          └────────┬─────────┘                                     │
│                                   │                                               │
│                    UserRegistered  │  (Integration Event)                         │
│                                   │                                               │
│         ┌─────────────────────────┼─────────────────────────┐                    │
│         │                         │                         │                    │
│         ▼                         ▼                         ▼                    │
│  ┌──────────────┐         ┌──────────────┐         ┌──────────────┐              │
│  │    Hotel     │◀───────▶│   Booking    │────────▶│   Payment    │              │
│  │   Context    │ Avail.  │   Context    │ Pay for │   Context    │              │
│  │              │ Check   │              │ booking │              │              │
│  │ Hotel, Room, │ (sync)  │ Booking,     │ (async) │ Payment,     │              │
│  │ Amenity,     │         │ DateRange,   │         │ Refund,      │              │
│  │ Photo,       │         │ GuestDetails │         │ Transaction  │              │
│  │ Location     │         │              │         │              │              │
│  └──────┬───────┘         └──────┬───────┘         └──────────────┘              │
│         │                        │                                               │
│         │ HotelApproved          │ BookingConfirmed                               │
│         │ ReviewCreated          │ BookingCancelled                               │
│         │                        │ PaymentCompleted                               │
│         ▼                        ▼                                               │
│  ┌──────────────┐         ┌──────────────┐         ┌──────────────┐              │
│  │   Review     │         │ Notification │         │  Analytics   │              │
│  │   Context    │────────▶│   Context    │◀────────│   Context    │              │
│  │              │ Review  │              │         │              │              │
│  │ Review,      │ Created │ Notification,│ Consumes│ Revenue,     │              │
│  │ Rating,      │ (async) │ EmailTemplate│ ALL     │ Occupancy,   │              │
│  │ OwnerReply   │         │              │ events  │ BookingStats │              │
│  └──────────────┘         └──────────────┘         └──────────────┘              │
│                                                                                  │
│  Legend:                                                                         │
│    ──▶  Async (RabbitMQ integration event)                                      │
│    ◀──▶ Sync (HTTP API call)                                                    │
│    ◀──  Consumes events from multiple contexts                                  │
└──────────────────────────────────────────────────────────────────────────────────┘
```

### Context Relationship Types

| Relationship | Type | Description |
|-------------|------|-------------|
| Booking → Hotel | **Sync (HTTP)** | Check room availability before creating a booking |
| Booking → Payment | **Async (Event)** | `BookingCreatedEvent` triggers payment initiation |
| Payment → Booking | **Async (Event)** | `PaymentCompletedEvent` confirms the booking |
| Review → Hotel | **Async (Event)** | `ReviewCreatedEvent` updates hotel rating aggregate |
| ALL → Notification | **Async (Event)** | All domain events that require user notification |
| ALL → Analytics | **Async (Event)** | All domain events that need reporting/tracking |

---

## 2. Ubiquitous Language

DDD requires a shared vocabulary. These terms mean the same thing to developers, product owners, and domain experts.

### Identity Context

| Term | Definition |
|------|-----------|
| **User** | A person registered on the platform with email and password |
| **Role** | A permission level: Guest, HotelOwner, or Admin |
| **Guest** | A user who browses and books hotels |
| **Hotel Owner** | A user who lists and manages hotel properties |
| **Admin** | A platform operator who approves hotels and manages the system |
| **Access Token** | A short-lived JWT (15 min) used to authenticate API requests |
| **Refresh Token** | A long-lived token (7 days) used to obtain new access tokens |

### Hotel Context

| Term | Definition |
|------|-----------|
| **Hotel** | A property listed on the platform (aggregate root) |
| **Room** | A bookable unit within a hotel with type, capacity, and pricing |
| **Room Type** | Classification: Standard, Deluxe, Suite, Presidential, etc. |
| **Amenity** | A feature of a hotel or room (Wi-Fi, pool, parking, breakfast) |
| **Photo** | An image associated with a hotel or room |
| **Location** | Geographic coordinates (latitude/longitude) + address of a hotel |
| **Star Rating** | Hotel classification (1-5 stars) set by the owner |
| **Approval Status** | Lifecycle of a hotel listing: Pending → Approved / Rejected |
| **Availability** | Whether a room is bookable for a specific date range |
| **Base Price** | The standard per-night rate for a room (before any adjustments) |

### Booking Context

| Term | Definition |
|------|-----------|
| **Booking** | A reservation made by a guest for a room at a hotel (aggregate root) |
| **Date Range** | Check-in and check-out dates for a stay |
| **Booking Status** | Lifecycle: Pending → Confirmed → CheckedIn → Completed / Cancelled |
| **Guest Details** | Name and contact info of the person staying (may differ from the booker) |
| **Total Price** | Calculated: (number of nights) × (room base price) + taxes/fees |
| **Cancellation Policy** | Rules for refund amounts based on when cancellation occurs |
| **Confirmation Code** | A human-readable unique code (e.g., STH-2026-ABC123) |

### Payment Context

| Term | Definition |
|------|-----------|
| **Payment** | A financial transaction for a booking (aggregate root) |
| **Payment Status** | Lifecycle: Pending → Processing → Completed → Failed / Refunded |
| **Payment Intent** | A Stripe object representing a payment attempt |
| **Refund** | A full or partial return of payment to the guest |
| **Transaction** | A record of money movement (payment or refund) |

### Review Context

| Term | Definition |
|------|-----------|
| **Review** | A guest's assessment of a hotel after a completed stay (aggregate root) |
| **Rating** | Numeric score (1-5) per category + overall |
| **Rating Categories** | Cleanliness, Service, Location, Value for Money, Comfort |
| **Owner Reply** | A hotel owner's response to a review |
| **Average Rating** | Aggregated score across all reviews for a hotel |

### Notification Context

| Term | Definition |
|------|-----------|
| **Notification** | A message sent to a user (email, in future: push, SMS) |
| **Email Template** | A Razor-based template for a specific notification type |
| **Delivery Status** | Pending → Sent → Failed |

### Analytics Context

| Term | Definition |
|------|-----------|
| **Revenue** | Total payment amount for a period |
| **Occupancy Rate** | Percentage of available room-nights that were booked |
| **Booking Trend** | Time-series data of booking counts over a period |
| **KPI** | Key Performance Indicator (revenue, bookings, avg rating, etc.) |

---

## 3. Identity Context

### Entity Diagram

```
┌─────────────────────────────────────┐
│              User                    │
│  (Aggregate Root)                    │
├─────────────────────────────────────┤
│  Id            : Guid               │
│  Email         : string             │
│  PasswordHash  : string             │
│  FirstName     : string             │
│  LastName      : string             │
│  PhoneNumber   : string?            │
│  AvatarUrl     : string?            │
│  EmailConfirmed: bool               │
│  IsActive      : bool               │
│  Roles         : List<UserRole>     │
│  RefreshTokens : List<RefreshToken> │
│  + Audit columns                    │
│  + Soft delete columns              │
├─────────────────────────────────────┤
│  + Register(email, password, name)  │
│  + ConfirmEmail()                   │
│  + UpdateProfile(...)               │
│  + ChangePassword(old, new)         │
│  + AssignRole(role)                 │
│  + RemoveRole(role)                 │
│  + Deactivate()                     │
└─────────────────────────────────────┘
          │ 1
          │
          │ *
┌─────────────────────────────────────┐
│          RefreshToken                │
│  (Entity)                            │
├─────────────────────────────────────┤
│  Id            : Guid               │
│  Token         : string             │
│  ExpiresAt     : DateTime           │
│  CreatedAt     : DateTime           │
│  CreatedByIp   : string             │
│  RevokedAt     : DateTime?          │
│  RevokedByIp   : string?            │
│  ReplacedByToken: string?           │
│  IsActive      : bool (computed)    │
├─────────────────────────────────────┤
│  + Revoke(ip, replacementToken)     │
│  + IsExpired : bool                 │
└─────────────────────────────────────┘
```

### Domain Events

| Event | Trigger | Consumers |
|-------|---------|-----------|
| `UserRegisteredEvent` | New user registration | Notification (welcome email) |
| `UserEmailConfirmedEvent` | Email confirmed | — |
| `UserDeactivatedEvent` | Admin deactivates user | — |

---

## 4. Hotel Context

### Entity Diagram

```
┌─────────────────────────────────────────┐
│                Hotel                     │
│  (Aggregate Root)                        │
├─────────────────────────────────────────┤
│  Id              : Guid                 │
│  OwnerId         : Guid (→ Identity)    │
│  Name            : string               │
│  Description     : string               │
│  StarRating      : int (1-5)            │
│  Address         : Address (VO)         │
│  Location        : GeoCoordinate (VO)   │
│  ContactEmail    : string               │
│  ContactPhone    : string               │
│  ApprovalStatus  : ApprovalStatus (Enum)│
│  AverageRating   : decimal?             │
│  TotalReviews    : int                  │
│  Rooms           : List<Room>           │
│  Photos          : List<HotelPhoto>     │
│  Amenities       : List<HotelAmenity>   │
│  + Audit columns                        │
│  + Soft delete columns                  │
│  + RowVersion                           │
├─────────────────────────────────────────┤
│  + Create(owner, name, desc, ...)       │
│  + Update(name, desc, ...)              │
│  + AddRoom(room)                        │
│  + RemoveRoom(roomId)                   │
│  + AddPhoto(photo)                      │
│  + RemovePhoto(photoId)                 │
│  + ReorderPhotos(photoIds)              │
│  + AddAmenity(amenity)                  │
│  + Approve()                            │
│  + Reject(reason)                       │
│  + UpdateRating(avg, count)             │
└──────────┬──────────────────────────────┘
           │ 1
           │
           │ *
┌──────────────────────────────────────┐
│              Room                     │
│  (Entity — child of Hotel)            │
├──────────────────────────────────────┤
│  Id            : Guid                │
│  HotelId       : Guid                │
│  RoomNumber    : string              │
│  RoomType      : RoomType (Enum)     │
│  Name          : string              │
│  Description   : string?             │
│  BasePrice     : Money (VO)          │
│  Capacity      : int                 │
│  BedType       : BedType (Enum)      │
│  BedCount      : int                 │
│  SizeInSqMeters: decimal?            │
│  IsActive      : bool                │
│  Photos        : List<RoomPhoto>     │
│  Amenities     : List<RoomAmenity>   │
│  + Audit columns                     │
├──────────────────────────────────────┤
│  + UpdateDetails(...)                │
│  + SetPrice(money)                   │
│  + Activate() / Deactivate()         │
└──────────────────────────────────────┘

Value Objects:
┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐
│    Address (VO)   │  │ GeoCoordinate(VO) │  │    Money (VO)     │
├───────────────────┤  ├───────────────────┤  ├───────────────────┤
│ Street   : string │  │ Latitude  : double│  │ Amount   : decimal│
│ City     : string │  │ Longitude : double│  │ Currency : string │
│ State    : string?│  └───────────────────┘  └───────────────────┘
│ Country  : string │
│ ZipCode  : string │
└───────────────────┘

Enums:
┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐
│  ApprovalStatus   │  │    RoomType       │  │    BedType        │
├───────────────────┤  ├───────────────────┤  ├───────────────────┤
│ Pending           │  │ Standard          │  │ Single            │
│ Approved          │  │ Deluxe            │  │ Double            │
│ Rejected          │  │ Suite             │  │ Queen             │
└───────────────────┘  │ Presidential      │  │ King              │
                       │ Family            │  │ Twin              │
                       │ Dormitory         │  │ Bunk              │
                       └───────────────────┘  └───────────────────┘
```

### Domain Events

| Event | Trigger | Consumers |
|-------|---------|-----------|
| `HotelCreatedEvent` | New hotel listed | Analytics |
| `HotelApprovedEvent` | Admin approves hotel | Notification (owner email) |
| `HotelRejectedEvent` | Admin rejects hotel | Notification (owner email) |
| `RoomPriceChangedEvent` | Owner changes room price | Analytics |

---

## 5. Booking Context

### Entity Diagram

```
┌─────────────────────────────────────────────┐
│                 Booking                      │
│  (Aggregate Root)                            │
├─────────────────────────────────────────────┤
│  Id                 : Guid                  │
│  ConfirmationCode   : string                │
│  GuestUserId        : Guid (→ Identity)     │
│  HotelId            : Guid (→ Hotel)        │
│  HotelName          : string (denormalized) │
│  RoomId             : Guid (→ Hotel)        │
│  RoomName           : string (denormalized) │
│  Status             : BookingStatus (Enum)  │
│  DateRange          : DateRange (VO)        │
│  GuestDetails       : GuestDetails (VO)    │
│  TotalPrice         : Money (VO)            │
│  NumberOfGuests     : int                   │
│  SpecialRequests    : string?               │
│  CancellationPolicy : CancellationPolicy   │
│  CancelledAt        : DateTime?             │
│  CancellationReason : string?               │
│  + Audit columns                            │
│  + Soft delete columns                      │
│  + RowVersion                               │
├─────────────────────────────────────────────┤
│  + Create(guest, hotel, room, dates, ...)   │
│  + Confirm()        → BookingConfirmedEvent │
│  + CheckIn()        → BookingCheckedInEvent │
│  + Complete()       → BookingCompletedEvent │
│  + Cancel(reason)   → BookingCancelledEvent │
│  + CalculateRefund(): Money                 │
└─────────────────────────────────────────────┘

Value Objects:
┌─────────────────────────┐  ┌─────────────────────────┐
│     DateRange (VO)      │  │    GuestDetails (VO)    │
├─────────────────────────┤  ├─────────────────────────┤
│ CheckIn   : DateOnly    │  │ FirstName  : string     │
│ CheckOut  : DateOnly    │  │ LastName   : string     │
│ + Nights  : int         │  │ Email      : string     │
│ + Overlaps(other): bool │  │ Phone      : string?    │
│ + Contains(date) : bool │  └─────────────────────────┘
└─────────────────────────┘

State Machine:
┌─────────┐    Confirm()    ┌───────────┐   CheckIn()   ┌───────────┐
│ Pending │───────────────▶│ Confirmed │─────────────▶│ CheckedIn │
└────┬────┘                └─────┬─────┘              └─────┬─────┘
     │                          │                          │
     │ Cancel()                 │ Cancel()                 │ Complete()
     ▼                          ▼                          ▼
┌───────────┐             ┌───────────┐             ┌───────────┐
│ Cancelled │             │ Cancelled │             │ Completed │
└───────────┘             └───────────┘             └───────────┘

Cancellation Policy:
┌──────────────────────────────────────────────┐
│        CancellationPolicy (Enum)             │
├──────────────────────────────────────────────┤
│ Flexible    — Full refund up to 1 day before │
│ Moderate    — Full refund up to 5 days before│
│ Strict      — 50% refund up to 7 days before│
│ NonRefundable — No refund after confirmation │
└──────────────────────────────────────────────┘
```

### Domain Events

| Event | Trigger | Consumers |
|-------|---------|-----------|
| `BookingCreatedEvent` | Guest creates booking | Payment (create charge), Notification |
| `BookingConfirmedEvent` | Payment succeeds | Notification (confirmation email), Analytics |
| `BookingCancelledEvent` | Guest/admin cancels | Payment (refund), Notification, Analytics |
| `BookingCheckedInEvent` | Guest checks in | Analytics |
| `BookingCompletedEvent` | Stay is finished | Review (enable review), Notification, Analytics |

---

## 6. Payment Context

### Entity Diagram

```
┌─────────────────────────────────────────────┐
│                Payment                       │
│  (Aggregate Root)                            │
├─────────────────────────────────────────────┤
│  Id                  : Guid                 │
│  BookingId           : Guid (→ Booking)     │
│  GuestUserId         : Guid (→ Identity)    │
│  Amount              : Money (VO)           │
│  Status              : PaymentStatus (Enum) │
│  StripePaymentIntentId: string              │
│  StripeChargeId      : string?              │
│  PaymentMethod       : string?              │
│  FailureReason       : string?              │
│  PaidAt              : DateTime?            │
│  Refunds             : List<Refund>         │
│  + Audit columns                            │
│  + RowVersion                               │
├─────────────────────────────────────────────┤
│  + Create(bookingId, amount)                │
│  + MarkProcessing(intentId)                 │
│  + MarkCompleted(chargeId)                  │
│  + MarkFailed(reason)                       │
│  + CreateRefund(amount, reason)             │
└──────────┬──────────────────────────────────┘
           │ 1
           │
           │ *
┌─────────────────────────────────────────────┐
│                 Refund                       │
│  (Entity — child of Payment)                 │
├─────────────────────────────────────────────┤
│  Id                  : Guid                 │
│  PaymentId           : Guid                 │
│  Amount              : Money (VO)           │
│  Reason              : string               │
│  StripeRefundId      : string               │
│  Status              : RefundStatus (Enum)  │
│  RefundedAt          : DateTime?            │
│  + Audit columns                            │
├─────────────────────────────────────────────┤
│  + MarkCompleted(stripeRefundId)            │
│  + MarkFailed(reason)                       │
└─────────────────────────────────────────────┘

State Machine:
┌─────────┐  MarkProcessing()  ┌────────────┐  MarkCompleted()  ┌───────────┐
│ Pending │──────────────────▶│ Processing │────────────────▶│ Completed │
└────┬────┘                   └─────┬──────┘                 └─────┬─────┘
     │                              │                              │
     │       MarkFailed()           │ MarkFailed()                 │ Refund()
     ▼                              ▼                              ▼
┌─────────┐                   ┌─────────┐                   ┌──────────┐
│ Failed  │                   │ Failed  │                   │ Refunded │
└─────────┘                   └─────────┘                   └──────────┘
```

### Domain Events

| Event | Trigger | Consumers |
|-------|---------|-----------|
| `PaymentCompletedEvent` | Stripe confirms charge | Booking (confirm), Notification (receipt) |
| `PaymentFailedEvent` | Stripe reports failure | Booking (cancel), Notification (failure email) |
| `RefundCompletedEvent` | Refund processed | Notification (refund email), Analytics |

---

## 7. Review Context

### Entity Diagram

```
┌─────────────────────────────────────────────┐
│                 Review                       │
│  (Aggregate Root)                            │
├─────────────────────────────────────────────┤
│  Id                : Guid                   │
│  HotelId           : Guid (→ Hotel)         │
│  GuestUserId       : Guid (→ Identity)      │
│  BookingId         : Guid (→ Booking)       │
│  Rating            : Rating (VO)            │
│  Title             : string                 │
│  Content           : string                 │
│  StayDate          : DateRange (VO)         │
│  OwnerReply        : OwnerReply? (VO)       │
│  + Audit columns                            │
│  + Soft delete columns                      │
├─────────────────────────────────────────────┤
│  + Create(hotel, guest, booking, rating...) │
│  + Update(rating, title, content)           │
│  + AddOwnerReply(content)                   │
│  + UpdateOwnerReply(content)                │
└─────────────────────────────────────────────┘

Value Objects:
┌───────────────────────────────┐  ┌───────────────────────────┐
│         Rating (VO)           │  │     OwnerReply (VO)       │
├───────────────────────────────┤  ├───────────────────────────┤
│ Overall      : decimal (1-5)  │  │ Content    : string       │
│ Cleanliness  : int (1-5)     │  │ RepliedAt  : DateTime     │
│ Service      : int (1-5)     │  └───────────────────────────┘
│ Location     : int (1-5)     │
│ ValueForMoney: int (1-5)     │
│ Comfort      : int (1-5)    │
│ + Average    : decimal       │
└───────────────────────────────┘

Business Rules:
  - A guest can only review a hotel AFTER a completed stay (BookingStatus == Completed)
  - One review per booking
  - Only the hotel owner can reply to a review
  - Overall rating is auto-calculated as the average of category ratings
```

### Domain Events

| Event | Trigger | Consumers |
|-------|---------|-----------|
| `ReviewCreatedEvent` | Guest submits review | Hotel (update avg rating), Notification (notify owner), Analytics |
| `ReviewUpdatedEvent` | Guest edits review | Hotel (recalculate avg rating) |
| `OwnerReplyAddedEvent` | Owner replies to review | Notification (notify guest) |

---

## 8. Notification Context

### Entity Diagram

```
┌─────────────────────────────────────────────┐
│             Notification                     │
│  (Aggregate Root)                            │
├─────────────────────────────────────────────┤
│  Id               : Guid                   │
│  UserId           : Guid (→ Identity)       │
│  Type             : NotificationType (Enum) │
│  Channel          : NotificationChannel     │
│  Subject          : string                  │
│  Body             : string                  │
│  RecipientEmail   : string                  │
│  Status           : DeliveryStatus (Enum)   │
│  SentAt           : DateTime?               │
│  FailureReason    : string?                 │
│  RetryCount       : int                     │
│  RelatedEntityId  : Guid?                   │
│  RelatedEntityType: string?                 │
│  + Audit columns                            │
├─────────────────────────────────────────────┤
│  + MarkSent()                               │
│  + MarkFailed(reason)                       │
│  + IncrementRetry()                         │
└─────────────────────────────────────────────┘

Enums:
┌────────────────────────┐  ┌───────────────────┐  ┌──────────────────┐
│   NotificationType     │  │ NotificationChannel│  │ DeliveryStatus   │
├────────────────────────┤  ├───────────────────┤  ├──────────────────┤
│ WelcomeEmail           │  │ Email             │  │ Pending          │
│ EmailConfirmation      │  │ (future: Push)    │  │ Sent             │
│ BookingConfirmation    │  │ (future: SMS)     │  │ Failed           │
│ BookingCancellation    │  └───────────────────┘  └──────────────────┘
│ PaymentReceipt         │
│ RefundProcessed        │
│ ReviewReminder         │
│ HotelApproved          │
│ HotelRejected          │
│ OwnerReplyNotification │
└────────────────────────┘
```

---

## 9. Analytics Context

### Read Models (Projections)

The Analytics context doesn't have traditional aggregates — it consumes events from other contexts and builds **read-optimized projections**.

```
┌─────────────────────────────────────────────┐
│          DailyBookingStats                   │
├─────────────────────────────────────────────┤
│  Date              : DateOnly               │
│  TotalBookings     : int                    │
│  TotalCancellations: int                    │
│  TotalRevenue      : decimal                │
│  AverageBookingValue: decimal               │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│          HotelPerformance                    │
├─────────────────────────────────────────────┤
│  HotelId           : Guid                   │
│  Period            : DateOnly               │
│  BookingCount      : int                    │
│  Revenue           : decimal                │
│  OccupancyRate     : decimal                │
│  AverageRating     : decimal                │
│  CancellationRate  : decimal                │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│          PlatformKpi                         │
├─────────────────────────────────────────────┤
│  Period            : DateOnly               │
│  TotalUsers        : int                    │
│  TotalHotels       : int                    │
│  TotalBookings     : int                    │
│  TotalRevenue      : decimal                │
│  TopDestinations   : List<string>           │
└─────────────────────────────────────────────┘

Consumes events:
  BookingCreated, BookingConfirmed, BookingCancelled, BookingCompleted
  PaymentCompleted, RefundCompleted
  ReviewCreated
  HotelCreated, HotelApproved
  UserRegistered
```

---

## 10. Cross-Context Relationships

### How Contexts Reference Each Other

No context directly accesses another's database. References are by ID only.

```
Identity Context
  └── User.Id ─────────────────────────────────────────────┐
                                                            │
Hotel Context                                               │
  └── Hotel.OwnerId ──────── references ──────── User.Id ◀─┤
                                                            │
Booking Context                                             │
  ├── Booking.GuestUserId ── references ──────── User.Id ◀─┤
  ├── Booking.HotelId ────── references ──────── Hotel.Id   │
  └── Booking.RoomId ─────── references ──────── Room.Id    │
                                                            │
Payment Context                                             │
  ├── Payment.BookingId ──── references ──────── Booking.Id │
  └── Payment.GuestUserId ── references ──────── User.Id ◀─┘

Review Context
  ├── Review.HotelId ─────── references ──────── Hotel.Id
  ├── Review.GuestUserId ─── references ──────── User.Id
  └── Review.BookingId ────── references ──────── Booking.Id

Notification Context
  └── Notification.UserId ── references ──────── User.Id
```

### Denormalized Data

Some contexts store copies of data from other contexts to avoid frequent HTTP calls:

| Context | Denormalized Field | Source | Updated Via |
|---------|-------------------|--------|-------------|
| Booking | `HotelName` | Hotel Service | Copied at booking creation |
| Booking | `RoomName` | Hotel Service | Copied at booking creation |
| Hotel | `AverageRating`, `TotalReviews` | Review Service | `ReviewCreatedEvent` |

This is a conscious trade-off: slight data staleness in exchange for zero inter-service calls on reads.

---

*This document defines the domain model that all services will implement. It serves as the contract between the architecture documentation and the actual code.*
