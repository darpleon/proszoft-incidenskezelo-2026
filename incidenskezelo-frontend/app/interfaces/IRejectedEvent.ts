export type RejectedEventSource = "Queue" | "Http";

export type RejectedEventReason = "Malformed" | "Invalid";

export interface IRejectedEvent {
  rejectedEventId: number;
  source: RejectedEventSource;
  reason: RejectedEventReason;
  error: string;
  rawPayload: string;
  receivedAtUtc: string;
}
