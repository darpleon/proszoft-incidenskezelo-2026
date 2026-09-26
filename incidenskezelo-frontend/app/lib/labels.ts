import type { IncidentPriority, IncidentStatus } from "~/interfaces/IIncident";

export const statusLabels: Record<IncidentStatus, string> = {
  New: "Új",
  Investigating: "Vizsgálat alatt",
  InProgress: "Kezelés alatt",
  Resolved: "Megoldott",
  Closed: "Lezárt",
};

export const priorityLabels: Record<IncidentPriority, string> = {
  Low: "Alacsony",
  Medium: "Közepes",
  High: "Magas",
  Critical: "Kritikus",
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
