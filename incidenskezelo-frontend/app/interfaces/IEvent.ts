export type EventSeverity = "Low" | "Medium" | "High" | "Critical";

export type ProcessingState = "Processed" | "Duplicate" | "Failed";

export interface IEvent {
  eventId: number;
  incidentId: number | null;
  serviceId: number;
  eventType: string;
  severity: EventSeverity;
  occurredAtUtc: string;
  processingState: ProcessingState;
  summary: string;
  payload: Record<string, unknown>;
}
