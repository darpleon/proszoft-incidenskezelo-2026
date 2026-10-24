import { Text, Title } from "@mantine/core";
import { AssigneeMenu } from "./AssigneeMenu";
import { DetailField } from "./DetailField";
import { StatusRing } from "./StatusRing";
import { users } from "~/data/users";
import type { IEvent } from "~/interfaces/IEvent";
import type { IIncident } from "~/interfaces/IIncident";
import { formatAge } from "~/lib/format";
import {
  isOpen,
  priorityLabels,
  priorityTextColors,
  statusLabels,
  statusTextColors,
} from "~/lib/labels";

type IncidentDetailHeaderProps = {
  incident: IIncident;
  events: IEvent[];
  onAssigneeChange: (userId: number | null) => void;
};

export function IncidentDetailHeader({ incident, events, onAssigneeChange }: IncidentDetailHeaderProps) {
  const duplicateCount = events.filter((event) => event.processingState === "Duplicate").length;
  const linkedCount = events.length - duplicateCount;

  return (
    <div className="px-[22px] pt-[22px] max-[680px]:px-4">
      <Text size="xs" fw={500} className="tracking-wider text-text-3">
        INC-{incident.incidentId}
      </Text>
      <Title order={1} className="mt-[3px] max-w-[34ch] text-2xl leading-tight font-semibold tracking-tight max-[680px]:text-xl">
        {incident.title}
      </Title>

      <div className="mt-[22px] flex flex-wrap items-start gap-y-3">
        <DetailField label="Status">
          <StatusRing colorClass={statusTextColors[incident.status]} filled={incident.status !== "New"} />
          {statusLabels[incident.status]}
        </DetailField>

        <DetailField label="Priority">
          <span className={`flex items-center gap-[7px] ${priorityTextColors[incident.priority]}`}>
            <StatusRing colorClass={priorityTextColors[incident.priority]} />
            {priorityLabels[incident.priority]}
          </span>
        </DetailField>

        <DetailField label="Assignee">
          <AssigneeMenu users={users} assigneeUserId={incident.assigneeUserId} onChange={onAssigneeChange} />
        </DetailField>

        <DetailField label="Open for">
          <span className={isOpen(incident.status) ? "text-crit" : "font-medium text-text-2"} suppressHydrationWarning>
            {formatAge(incident.createdAtUtc)}
          </span>
        </DetailField>

        <DetailField label="Events">
          <span className="font-medium text-text-2">
            {linkedCount} linked{duplicateCount > 0 && `, ${duplicateCount} discarded`}
          </span>
        </DetailField>
      </div>
    </div>
  );
}
