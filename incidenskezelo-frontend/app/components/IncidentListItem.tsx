import { NavLink } from "react-router";
import { services } from "~/data/services";
import type { IIncident } from "~/interfaces/IIncident";
import { formatAge } from "~/lib/format";
import { priorityColors, statusLabels } from "~/lib/labels";

type IncidentListItemProps = {
  incident: IIncident;
};

export function IncidentListItem({ incident }: IncidentListItemProps) {
  const service = services.find((item) => item.serviceId === incident.serviceId);

  return (
    <NavLink
      to={`/incidents/${incident.incidentId}`}
      className={({ isActive }) =>
        `group relative block py-2.5 pr-4 pl-[13px] transition-colors motion-reduce:transition-none ${
          isActive ? "bg-panel" : "hover:bg-[#eff2f0]"
        }`
      }
    >
      <span className={`absolute top-2 bottom-2 left-0 w-[3px] rounded-r-sm ${priorityColors[incident.priority]}`} />
      <span className="absolute inset-y-0 right-0 hidden w-0.5 bg-petrol group-aria-[current=page]:block" />

      <span className="mb-1 block text-[13px] leading-snug font-medium tracking-[-0.01em] group-aria-[current=page]:font-semibold">
        {incident.title}
      </span>
      <span className="flex items-center gap-[7px] text-[11.5px] text-text-2">
        {incident.assigneeUserId === null ? (
          <span className="font-semibold text-high">Nincs felelős</span>
        ) : (
          <span>{statusLabels[incident.status]}</span>
        )}
        {service && <span>{service.code}</span>}
        <span className="ml-auto text-text-3" suppressHydrationWarning>
          {formatAge(incident.createdAtUtc)}
        </span>
      </span>
    </NavLink>
  );
}
