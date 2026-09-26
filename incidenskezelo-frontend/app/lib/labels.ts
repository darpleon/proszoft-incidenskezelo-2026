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

export function isOpen(status: IncidentStatus) {
  return status !== "Resolved" && status !== "Closed";
}
