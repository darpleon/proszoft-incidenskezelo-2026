export type EventSeverity = "Info" | "Warning" | "Error" | "Critical";

export interface IEvent {
  eventId: number;
  sourceEventId: string;
  serviceCode: string;
  eventType: string;
  severity: EventSeverity;
  occurredAtUtc: string;
  receivedAtUtc: string;
  summary: string | null;
  payloadJson: string | null;
  correlationKey: string | null;
  incidentId: string | null;
}
