export type IncidentStatus = "New" | "Investigating" | "InProgress" | "Resolved" | "Closed";

export type IncidentPriority = "Low" | "Medium" | "High" | "Critical";

export interface IIncident {
  incidentId: number;
  title: string;
  description: string;
  status: IncidentStatus;
  priority: IncidentPriority;
  serviceId: number;
  assigneeUserId: number | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  closedAtUtc: string | null;
}
