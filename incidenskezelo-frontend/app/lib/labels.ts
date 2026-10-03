import type { EventSeverity } from "~/interfaces/IEvent";
import type { IncidentPriority, IncidentStatus } from "~/interfaces/IIncident";

export const statusLabels: Record<IncidentStatus, string> = {
  New: "New",
  Investigating: "Investigating",
  InProgress: "In progress",
  Resolved: "Resolved",
  Closed: "Closed",
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
  New: "text-text-3",
  Investigating: "text-high",
  InProgress: "text-petrol",
  Resolved: "text-ok",
  Closed: "text-text-3",
};

export const priorityTextColors: Record<IncidentPriority, string> = {
  Low: "text-low",
  Medium: "text-med",
  High: "text-high",
  Critical: "text-crit",
};

export function isOpen(status: IncidentStatus) {
  return status !== "Resolved" && status !== "Closed";
}

export const severityLabels: Record<EventSeverity, string> = priorityLabels;

export const severityTextColors: Record<EventSeverity, string> = priorityTextColors;

export const eventTypeLabels: Record<string, string> = {
  DbConnectionError: "Database connection error",
  HighLatency: "High latency",
  ErrorRateIncrease: "Increased error rate",
  JobFailed: "Background job failed",
  CapacityIssue: "Capacity issue",
  ServiceRecovered: "Service recovered",
  CertificateExpiring: "Certificate expiring",
  DependencyFailure: "Dependency failure",
};
