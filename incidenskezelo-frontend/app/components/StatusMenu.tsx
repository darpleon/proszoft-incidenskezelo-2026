import { Menu } from "@mantine/core";
import { DetailMenu, DetailMenuItem } from "./DetailMenu";
import { IncidentStatusRing } from "./StatusRing";
import type { IncidentStatus } from "~/interfaces/IIncident";
import { allowedNextStatuses, statusLabels } from "~/lib/labels";

type StatusMenuProps = {
  status: IncidentStatus;
  onChange: (status: IncidentStatus) => void;
};

const workingStatuses: IncidentStatus[] = ["Open", "Investigating", "InProgress", "Verifying", "Resolved"];

export function StatusMenu({ status, onChange }: StatusMenuProps) {
  const renderItem = (option: IncidentStatus) => (
    <DetailMenuItem
      key={option}
      selected={option === status}
      disabled={option !== status && !allowedNextStatuses[status].includes(option)}
      leftSection={<IncidentStatusRing status={option} />}
      onClick={() => onChange(option)}
    >
      {statusLabels[option]}
    </DetailMenuItem>
  );

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
      {workingStatuses.map(renderItem)}
      <Menu.Divider />
      {renderItem("Declined")}
    </DetailMenu>
  );
}
