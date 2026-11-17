import type { IncidentPriority, IncidentStatus } from "./IIncident";

export type CreatedRange = "" | "24h" | "7d" | "30d" | "custom";

export interface IIncidentFilter {
  status: IncidentStatus[];
  priority: IncidentPriority[];
  service: string[];
  assignee: string;
  created: CreatedRange;
  from: string;
  to: string;
  q: string;
}
