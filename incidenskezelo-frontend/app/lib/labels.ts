import type { EventSeverity } from "~/interfaces/IEvent";
import type { IncidentPriority, IncidentStatus } from "~/interfaces/IIncident";

export const statusLabels: Record<IncidentStatus, string> = {
  Open: "Open",
  Investigating: "Investigating",
  InProgress: "In progress",
  Verifying: "Verifying",
  Resolved: "Resolved",
  Declined: "Declined",
};

export const allowedNextStatuses: Record<IncidentStatus, IncidentStatus[]> = {
  Open: ["Investigating", "Declined"],
  Investigating: ["InProgress", "Declined"],
  InProgress: ["Verifying", "Declined"],
  Verifying: ["Resolved", "InProgress"],
  Resolved: ["InProgress"],
  Declined: ["InProgress"],
};

export const priorityLabels: Record<IncidentPriority, string> = {
  Low: "Low",
  Medium: "Medium",
  High: "High",
  Critical: "Critical",
};

export const priorityColors: Record<IncidentPriority, string> = {
  Low: "bg-low",
  Medium: "bg-med",
  High: "bg-high",
  Critical: "bg-crit",
};

export const statusTextColors: Record<IncidentStatus, string> = {
  Open: "text-text-3",
  Investigating: "text-high",
  InProgress: "text-petrol",
  Verifying: "text-med",
  Resolved: "text-ok",
  Declined: "text-text-3",
};

export const priorityTextColors: Record<IncidentPriority, string> = {
  Low: "text-low",
  Medium: "text-med",
  High: "text-high",
  Critical: "text-crit",
};

export function isOpen(status: IncidentStatus) {
  return status !== "Resolved" && status !== "Declined";
}

export const severityLabels: Record<EventSeverity, string> = {
  Info: "Info",
  Warning: "Warning",
  Error: "Error",
  Critical: "Critical",
};

export const severityTextColors: Record<EventSeverity, string> = {
  Info: "text-low",
  Warning: "text-med",
  Error: "text-high",
  Critical: "text-crit",
};

export const eventTypeLabels: Record<string, string> = {
  ServiceDown: "Service down",
  ServiceRecovered: "Service recovered",
  HighLatency: "High latency",
  HighErrorRate: "High error rate",
  DbConnectionError: "Database connection error",
  JobFailed: "Background job failed",
  CapacityIssue: "Capacity issue",
  DependencyFailure: "Dependency failure",
};
