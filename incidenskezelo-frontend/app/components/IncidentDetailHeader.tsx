import { Title } from "@mantine/core";
import { AssigneeMenu } from "./AssigneeMenu";
import { DetailField } from "./DetailField";
import { IncidentRef } from "./IncidentRef";
import { InlineEdit } from "./InlineEdit";
import { PriorityMenu } from "./PriorityMenu";
import { StatusMenu } from "./StatusMenu";
import type { IIncident, IncidentPriority, IncidentStatus } from "~/interfaces/IIncident";
import type { IAssignableUser } from "~/interfaces/IUser";
import { formatAge } from "~/lib/format";
import { isOpen } from "~/lib/labels";
import { INCIDENT_TITLE_MAX_LENGTH } from "~/lib/validation";

type IncidentDetailHeaderProps = {
  incident: IIncident;
  assignableUsers: IAssignableUser[];
  onTitleSave: (title: string) => Promise<void>;
  onStatusChange: (status: IncidentStatus) => void;
  onPriorityChange: (priority: IncidentPriority) => void;
  onAssigneeChange: (assigneeUserId: number | null) => void;
};

export function IncidentDetailHeader({ incident, assignableUsers, onTitleSave, onStatusChange, onPriorityChange, onAssigneeChange }: IncidentDetailHeaderProps) {
  return (
    <div className="px-[22px] pt-[22px] max-[680px]:px-4">
      <IncidentRef id={incident.id} />
      <div className="mt-[3px] max-w-[60ch]">
        <InlineEdit
          value={incident.title}
          label="Title"
          maxLength={INCIDENT_TITLE_MAX_LENGTH}
          onSave={onTitleSave}
          inputClassName="h-auto min-h-0 py-1 text-2xl leading-tight font-semibold tracking-tight max-[680px]:text-xl"
        >
          <Title order={1} className="text-2xl leading-tight font-semibold tracking-tight max-[680px]:text-xl">
            {incident.title}
          </Title>
        </InlineEdit>
      </div>

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
