export type IncidentChangeKind =
  | "created"
  | "status"
  | "title"
  | "description"
  | "priority"
  | "assignee"
  | "eventLinked"
  | "aiAnalysis";

export interface IIncidentChange {
  kind: IncidentChangeKind;
  occurredAtUtc: string;
  oldValue: string | null;
  newValue: string | null;
  actorUserId: number | null;
  actorName: string | null;
}
