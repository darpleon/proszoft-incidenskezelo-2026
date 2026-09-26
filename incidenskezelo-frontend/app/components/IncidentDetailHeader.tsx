import { Text, Title } from "@mantine/core";
import { DetailField } from "./DetailField";
import { StatusRing } from "./StatusRing";
import { users } from "~/data/users";
import type { IIncident } from "~/interfaces/IIncident";
import { formatAge } from "~/lib/format";
import { getInitials } from "~/lib/initials";
import {
  isOpen,
  priorityLabels,
  priorityTextColors,
  statusLabels,
  statusTextColors,
} from "~/lib/labels";

type IncidentDetailHeaderProps = {
  incident: IIncident;
};

export function IncidentDetailHeader({ incident }: IncidentDetailHeaderProps) {
  const assignee = users.find((user) => user.userId === incident.assigneeUserId);

  return (
    <div className="px-[22px] pt-[22px] max-[680px]:px-4">
      <Text size="xs" fw={500} className="tracking-wider text-text-3">
        INC-{incident.incidentId}
      </Text>
      <Title order={1} className="mt-[3px] max-w-[34ch] text-2xl leading-tight font-semibold tracking-tight max-[680px]:text-xl">
        {incident.title}
      </Title>

      <div className="mt-[22px] flex flex-wrap items-start gap-y-3">
        <DetailField label="Állapot">
          <StatusRing colorClass={statusTextColors[incident.status]} filled={incident.status !== "New"} />
          {statusLabels[incident.status]}
        </DetailField>

        <DetailField label="Prioritás">
          <span className={`flex items-center gap-[7px] ${priorityTextColors[incident.priority]}`}>
            <StatusRing colorClass={priorityTextColors[incident.priority]} />
            {priorityLabels[incident.priority]}
          </span>
        </DetailField>

        <DetailField label="Felelős">
          {assignee ? (
            <>
              <i className="grid size-[21px] place-items-center rounded-full bg-petrol-bg text-[9px] font-bold text-petrol not-italic">
                {getInitials(assignee.name)}
              </i>
              {assignee.name}
            </>
          ) : (
            <span className="text-high">Nincs felelős</span>
          )}
        </DetailField>

        <DetailField label="Nyitva">
          <span className={isOpen(incident.status) ? "text-crit" : "font-medium text-text-2"}>
            {formatAge(incident.createdAtUtc)}
          </span>
        </DetailField>
      </div>
    </div>
  );
}
