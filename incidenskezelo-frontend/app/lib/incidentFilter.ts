import type { IncidentPriority, IncidentStatus } from "~/interfaces/IIncident";
import type { CreatedRange, IIncidentFilter } from "~/interfaces/IIncidentFilter";

export const openStatuses: IncidentStatus[] = ["Open", "Investigating", "InProgress", "Verifying"];

export const closedStatuses: IncidentStatus[] = ["Resolved", "Declined"];

export const allStatuses: IncidentStatus[] = [...openStatuses, ...closedStatuses];

const createdRanges: CreatedRange[] = ["", "24h", "7d", "30d", "custom"];

const rangeHours: Record<string, number> = { "24h": 24, "7d": 7 * 24, "30d": 30 * 24 };

export function sameItems<T>(a: T[], b: T[]) {
  return a.length === b.length && a.every((item) => b.includes(item));
}

export function readFilter(params: URLSearchParams): IIncidentFilter {
  const created = params.get("created") ?? "";
  const status = params.getAll("status").filter(Boolean) as IncidentStatus[];

  return {
    status: !params.has("status") ? openStatuses : status.length === 0 ? allStatuses : status,
    priority: params.getAll("priority") as IncidentPriority[],
    service: params.getAll("service"),
    assignee: params.get("assignee") ?? "",
    created: createdRanges.includes(created as CreatedRange) ? (created as CreatedRange) : "",
    from: params.get("from") ?? "",
    to: params.get("to") ?? "",
    q: params.get("q") ?? "",
  };
}

export function writeFilter(filter: IIncidentFilter): URLSearchParams {
  const params = new URLSearchParams();

  if (filter.status.length === 0 || filter.status.length === allStatuses.length) params.append("status", "");
  else if (!sameItems(filter.status, openStatuses)) filter.status.forEach((status) => params.append("status", status));

  filter.priority.forEach((priority) => params.append("priority", priority));
  filter.service.forEach((service) => params.append("service", service));
  if (filter.assignee) params.set("assignee", filter.assignee);
  if (filter.created) params.set("created", filter.created);
  if (filter.created === "custom" && filter.from) params.set("from", filter.from);
  if (filter.created === "custom" && filter.to) params.set("to", filter.to);
  if (filter.q) params.set("q", filter.q);

  return params;
}

export function toApiParams(filter: IIncidentFilter): URLSearchParams {
  const params = new URLSearchParams();

  if (filter.status.length < allStatuses.length) filter.status.forEach((status) => params.append("status", status));
  filter.priority.forEach((priority) => params.append("priority", priority));
  filter.service.forEach((service) => params.append("service", service));
  if (filter.assignee) params.set("assignee", filter.assignee);
  if (filter.q.trim()) params.set("q", filter.q.trim());

  if (rangeHours[filter.created]) {
    params.set("from", new Date(Date.now() - rangeHours[filter.created] * 3_600_000).toISOString());
  }

  if (filter.created === "custom") {
    const [from, to] = filter.from && filter.to && filter.from > filter.to ? [filter.to, filter.from] : [filter.from, filter.to];
    if (from) params.set("from", new Date(`${from}T00:00:00`).toISOString());
    if (to) params.set("to", new Date(`${to}T23:59:59.999`).toISOString());
  }

  return params;
}

export function isDefaultFilter(filter: IIncidentFilter) {
  return writeFilter(filter).toString() === "";
}
