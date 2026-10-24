import { Text, Title } from "@mantine/core";
import { AssigneeMenu } from "./AssigneeMenu";
import { DetailField } from "./DetailField";
import { PriorityMenu } from "./PriorityMenu";
import { StatusMenu } from "./StatusMenu";
import { users } from "~/data/users";
import type { IEvent } from "~/interfaces/IEvent";
import type { IIncident, IncidentChanges } from "~/interfaces/IIncident";
import { formatAge } from "~/lib/format";
import { isOpen } from "~/lib/labels";

type IncidentDetailHeaderProps = {
  incident: IIncident;
  events: IEvent[];
  onChange: (changes: IncidentChanges) => void;
};

export function IncidentDetailHeader({ incident, events, onChange }: IncidentDetailHeaderProps) {
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
          <StatusMenu status={incident.status} onChange={(status) => onChange({ status })} />
        </DetailField>

        <DetailField label="Priority">
          <PriorityMenu priority={incident.priority} onChange={(priority) => onChange({ priority })} />
        </DetailField>

        <DetailField label="Assignee">
          <AssigneeMenu users={users} assigneeUserId={incident.assigneeUserId} onChange={(assigneeUserId) => onChange({ assigneeUserId })} />
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
