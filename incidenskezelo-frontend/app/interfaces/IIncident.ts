export type IncidentStatus = "Open" | "Investigating" | "InProgress" | "Verifying" | "Resolved" | "Declined";

export type IncidentPriority = "Low" | "Medium" | "High" | "Critical";

export interface IIncidentRevisions {
  title: number;
  description: number;
  priority: number;
  status: number;
  assignee: number;
}

export interface IIncident {
  id: string;
  title: string;
  description: string;
  status: IncidentStatus;
  priority: IncidentPriority;
  assigneeUserId: number | null;
  assigneeName: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  closedAtUtc: string | null;
  revisions: IIncidentRevisions;
}
