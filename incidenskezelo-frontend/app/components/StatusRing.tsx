import type { IncidentPriority, IncidentStatus } from "~/interfaces/IIncident";
import { priorityTextColors, statusTextColors } from "~/lib/labels";

type StatusRingProps = {
  colorClass: string;
  filled?: boolean;
};

export function StatusRing({ colorClass, filled = true }: StatusRingProps) {
  return (
    <i
      className={`size-[9px] shrink-0 rounded-full border-current ${filled ? "border-[4.5px]" : "border-[2.5px]"} ${colorClass}`}
    />
  );
}

export function IncidentStatusRing({ status }: { status: IncidentStatus }) {
  return <StatusRing colorClass={statusTextColors[status]} filled={status !== "New"} />;
}

export function PriorityRing({ priority }: { priority: IncidentPriority }) {
  return <StatusRing colorClass={priorityTextColors[priority]} />;
}
