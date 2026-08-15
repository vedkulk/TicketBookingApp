# Ticket Booking System — High Level Design

## 1. Overview
An end-to-end ticket booking platform (events/shows/seats) built to learn production-grade backend patterns: caching, distributed locking, async messaging, saga-based failure handling, auth, rate limiting, and distributed service hosting.

**Stack:** .NET Core (Web API), Angular, PostgreSQL, Redis, RabbitMQ, Service Fabric, Docker.

---

## 2. Goals
- Prevent double-booking under concurrent seat selection
- Handle partial failures gracefully (payment fails after seat hold, etc.)
- Support caching without serving stale seat availability
- Rate-limit and authenticate requests at the edge
- Be deployable as independently scalable services

---

## 3. High-Level Architecture

```
                        ┌─────────────────┐
                        │   Angular SPA     │
                        └────────┬─────────┘
                                 │ HTTPS
                        ┌────────▼─────────┐
                        │  Load Balancer /   │
                        │  Front Door        │
                        └────────┬─────────┘
                                 │
                        ┌────────▼─────────┐
                        │   API Gateway      │
                        │  (Ocelot)          │
                        │  - Auth validation │
                        │  - Rate limiting   │
                        └───┬───┬───┬───┬───┘
             ┌──────────────┘   │   │   └──────────────┐
             ▼                  ▼   ▼                   ▼
     ┌───────────────┐  ┌──────────────┐       ┌────────────────┐
     │ Identity Svc   │  │ Catalog Svc   │       │ Inventory Svc   │
     │ (JWT issuer)   │  │ (events/venue)│       │ (seat state,    │
     │                │  │ cached in     │       │ stateful SF     │
     │                │  │ Redis         │       │ service)        │
     └───────────────┘  └──────────────┘       └────────┬────────┘
                                                          │
                                                 ┌────────▼────────┐
                                                 │  Booking Svc     │
                                                 │  (orchestrates   │
                                                 │  hold→pay→confirm)│
                                                 └───┬─────────┬───┘
                                       ┌─────────────┘         └──────────┐
                                       ▼                                  ▼
                             ┌──────────────────┐             ┌──────────────────┐
                             │  Payment Svc       │             │  RabbitMQ         │
                             │  (idempotent txns) │             │  (async events)   │
                             └──────────────────┘             └─────────┬────────┘
                                                                          ▼
                                                                ┌──────────────────┐
                                                                │ Notification Svc   │
                                                                │ (email/SMS)         │
                                                                └──────────────────┘

     Shared: PostgreSQL (source of truth) · Redis (cache + locks + rate-limit counters)
```

---

## 4. Services

| Service | Type | Responsibility |
|---|---|---|
| **Identity** | Stateless | Login/register, JWT issuance, refresh tokens |
| **Catalog** | Stateless | Events, venues, showtimes — read-heavy, cached |
| **Inventory** | **Stateful (SF)** | Seat map + real-time availability, seat-hold locks |
| **Booking** | Stateless | Orchestrates the hold → payment → confirm saga |
| **Payment** | Stateless | Talks to payment gateway, idempotent by design |
| **Notification** | Stateless, async consumer | Sends confirmations via queue trigger |

---

## 5. Data Layer

- **PostgreSQL** — bookings, users, transactions, events (source of truth, ACID)
- **Redis** — three distinct jobs, don't conflate them:
  1. Cache-aside for Catalog reads (TTL + invalidate on write)
  2. Distributed lock for seat-hold during checkout (`SETNX` + TTL, ~5-10 min)
  3. Sliding-window counters for rate limiting at the gateway
- **RabbitMQ** — decouples Booking confirmation from Notification sending; enables retries without blocking the booking response

---

## 6. Critical Flow: Seat Booking (the core learning problem)

1. User selects seat → Inventory Service attempts Redis lock (`SETNX seat:{id} userId EX 600`)
2. Lock acquired → seat shown as "held" to other users (read from Redis, not DB)
3. Booking Service starts saga: create pending booking row → call Payment Service
4. **Payment succeeds** → confirm booking (DB write with optimistic concurrency/row version as the final safety net) → release Redis lock → publish `BookingConfirmed` event → RabbitMQ → Notification Service sends email
5. **Payment fails** → compensating transaction: release Redis lock, mark booking `Failed`, no DB seat state was ever committed
6. **Lock expires before payment completes** → treated as abandonment; seat returns to available pool automatically (TTL handles this, no manual cleanup needed)

This is why Redis lock + DB optimistic concurrency are both present — the lock prevents two users racing for the same seat *during* checkout; the DB check is the final source-of-truth guard in case of edge cases (clock skew, lock service hiccup).

---

## 7. Cross-Cutting Concerns

**Auth:** JWT bearer tokens (short-lived access + refresh), role claims for user/admin, validated at the gateway before requests reach services.

**Rate limiting:** Token-bucket per user/IP, counters stored in Redis so it works correctly across multiple scaled-out gateway instances (in-memory counters wouldn't survive horizontal scaling).

**Caching:** Cache-aside pattern for Catalog. Seat/inventory state is **never** cached beyond its lock TTL — caching availability data is exactly how double-booking bugs get introduced.

**Load balancing:** Front Door/L7 LB for client-facing traffic; Service Fabric's naming service + reverse proxy handles service-to-service routing.

---

## 8. Deployment

- Local dev: Docker Compose (API services + Angular + Postgres + Redis + RabbitMQ)
- Production target: Service Fabric cluster — Catalog/Booking/Payment/Notification as stateless services, Inventory as a stateful partitioned service
- CI/CD: build → test → containerize → deploy pipeline (added in hardening phase)

---

## 9. What Each Phase of the Learning Roadmap Maps To

| Roadmap Phase | HLD Section It Builds |
|---|---|
| Phase 1 (.NET foundations) | Catalog Service, basic CRUD |
| Phase 2 (Redis) | Caching + seat-hold locking |
| Phase 3 (Auth) | Identity Service, JWT |
| Phase 4 (Booking flow) | Booking Service saga, Payment integration, RabbitMQ |
| Phase 5 (Gateway) | API Gateway, rate limiting |
| Phase 6 (Service Fabric) | Stateful Inventory Service, cluster deployment |
| Phase 7 (Hardening) | Load testing, logging, CI/CD |

---

## 10. Open Design Questions (revisit as you build)

- Repository pattern on top of EF Core — worth it here, or added ceremony? (Decide in Phase 1, Week 3)
- Saga: orchestration (Booking Service directs each step) vs choreography (services react to events)? Roadmap assumes orchestration for simplicity — reconsider once you've built it once.
- Optimistic vs pessimistic concurrency at the DB layer for the final booking write — the roadmap defaults to optimistic (row version), pessimistic is worth trying once to feel the difference.
