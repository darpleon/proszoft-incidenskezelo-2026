import { Menu } from "@mantine/core";
import { DetailMenu, DetailMenuItem } from "./DetailMenu";
import { StatusRing } from "./StatusRing";
import type { IncidentStatus } from "~/interfaces/IIncident";
import { statusLabels, statusTextColors } from "~/lib/labels";

type StatusMenuProps = {
  status: IncidentStatus;
  onChange: (status: IncidentStatus) => void;
};

const workingStatuses: IncidentStatus[] = ["New", "Investigating", "InProgress", "Resolved"];

function StatusIcon({ status }: { status: IncidentStatus }) {
  return <StatusRing colorClass={statusTextColors[status]} filled={status !== "New"} />;
}

export function StatusMenu({ status, onChange }: StatusMenuProps) {
  const canClose = status === "Resolved" || status === "Closed";

  return (
    <DetailMenu
      label="Change status"
      trigger={
        <>
          <StatusIcon status={status} />
          {statusLabels[status]}
        </>
      }
    >
      {workingStatuses.map((option) => (
        <DetailMenuItem key={option} selected={option === status} leftSection={<StatusIcon status={option} />} onClick={() => onChange(option)}>
          {statusLabels[option]}
        </DetailMenuItem>
      ))}
      <Menu.Divider />
      {canClose ? (
        <DetailMenuItem selected={status === "Closed"} leftSection={<StatusIcon status="Closed" />} onClick={() => onChange("Closed")}>
          {statusLabels.Closed}
        </DetailMenuItem>
      ) : (
        <p className="m-[3px] rounded bg-[#fbf3e4] px-[9px] py-[7px] text-xs leading-snug text-text-2">
          An incident can only be closed once it is resolved.
        </p>
      )}
    </DetailMenu>
  );
}
