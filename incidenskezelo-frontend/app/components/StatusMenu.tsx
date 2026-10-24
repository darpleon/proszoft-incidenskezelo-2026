import { Menu } from "@mantine/core";
import { DetailMenu, DetailMenuItem } from "./DetailMenu";
import { IncidentStatusRing } from "./StatusRing";
import type { IncidentStatus } from "~/interfaces/IIncident";
import { statusLabels } from "~/lib/labels";

type StatusMenuProps = {
  status: IncidentStatus;
  onChange: (status: IncidentStatus) => void;
};

const workingStatuses: IncidentStatus[] = ["New", "Investigating", "InProgress", "Resolved"];

export function StatusMenu({ status, onChange }: StatusMenuProps) {
  const canClose = status === "Resolved" || status === "Closed";

  return (
    <DetailMenu
      label="Change status"
      trigger={
        <>
          <IncidentStatusRing status={status} />
          {statusLabels[status]}
        </>
      }
    >
      {workingStatuses.map((option) => (
        <DetailMenuItem key={option} selected={option === status} leftSection={<IncidentStatusRing status={option} />} onClick={() => onChange(option)}>
          {statusLabels[option]}
        </DetailMenuItem>
      ))}
      <Menu.Divider />
      {canClose ? (
        <DetailMenuItem selected={status === "Closed"} leftSection={<IncidentStatusRing status="Closed" />} onClick={() => onChange("Closed")}>
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
