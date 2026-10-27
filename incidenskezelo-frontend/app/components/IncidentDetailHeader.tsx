import { Title } from "@mantine/core";
import { AssigneeMenu } from "./AssigneeMenu";
import { DetailField } from "./DetailField";
import { IncidentRef } from "./IncidentRef";
import { PriorityMenu } from "./PriorityMenu";
import { StatusMenu } from "./StatusMenu";
import type { IIncident, IncidentPriority, IncidentStatus } from "~/interfaces/IIncident";
import type { IAssignableUser } from "~/interfaces/IUser";
import { formatAge } from "~/lib/format";
import { isOpen } from "~/lib/labels";

type IncidentDetailHeaderProps = {
  incident: IIncident;
  assignableUsers: IAssignableUser[];
  onStatusChange: (status: IncidentStatus) => void;
  onPriorityChange: (priority: IncidentPriority) => void;
  onAssigneeChange: (assigneeUserId: number | null) => void;
};

export function IncidentDetailHeader({ incident, assignableUsers, onStatusChange, onPriorityChange, onAssigneeChange }: IncidentDetailHeaderProps) {
  return (
    <div className="px-[22px] pt-[22px] max-[680px]:px-4">
      <IncidentRef id={incident.id} />
      <Title order={1} className="mt-[3px] max-w-[34ch] text-2xl leading-tight font-semibold tracking-tight max-[680px]:text-xl">
        {incident.title}
      </Title>

      <div className="mt-[22px] flex flex-wrap items-start gap-y-3">
        <DetailField label="Status">
          <StatusMenu status={incident.status} onChange={onStatusChange} />
        </DetailField>

        <DetailField label="Priority">
          <PriorityMenu priority={incident.priority} onChange={onPriorityChange} />
        </DetailField>

        <DetailField label="Assignee">
          <AssigneeMenu
            users={assignableUsers}
            assigneeUserId={incident.assigneeUserId}
            assigneeName={incident.assigneeName}
            onChange={onAssigneeChange}
          />
        </DetailField>

        <DetailField label="Open for">
          <span className={isOpen(incident.status) ? "text-crit" : "font-medium text-text-2"} suppressHydrationWarning>
            {formatAge(incident.createdAtUtc)}
          </span>
        </DetailField>
      </div>
    </div>
  );
}
