namespace OutboxSaga.Architecture;

/// <summary>
/// STAFF ARCHITECT DECISION RECORD (ADR)
///
/// Subject: Consistency and Idempotency in Distributed Sagas
///
/// 1. CONTEXT:
/// We are building a choreographed saga using Kafka as a message broker.
/// In distributed systems, network failures are inevitable.
///
/// 2. DECISION:
/// We have implemented the DUAL PATTERN: INBOX + OUTBOX.
///
/// 3. RATIONALE:
/// - OUTBOX: Solves the "Atomicity Problem" (updating DB and notifying the world).
///   By saving the event in the same transaction as the business state, we guarantee
///   that if the order is saved, the event IS saved.
///
/// - INBOX: Solves the "Duplicate Processing Problem".
///   Kafka delivers "At Least Once". If a consumer crashes AFTER processing
///   but BEFORE committing the offset, it will receive the same message again.
///   The Inbox table (idempotency check) ensures we don't charge the customer twice
///   or ship the same product twice.
///
/// - TRACING (Correlation & Causation IDs):
///   Essential for Sagas. Without them, you cannot reconstruct the business flow
///   across independent logs.
///   CorrelationId = The "Story" (Order #123)
///   CausationId = The "Trigger" (Message #ABC generated Message #DEF)
///
/// 4. TRADE-OFFS:
/// - Pros: High consistency, reliability, and observability.
/// - Cons: Increased write latency (extra tables) and complexity.
/// </summary>
public static class ArchitectureNotes { }
