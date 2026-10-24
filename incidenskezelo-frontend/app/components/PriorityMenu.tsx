import { DetailMenu, DetailMenuItem } from "./DetailMenu";
import { StatusRing } from "./StatusRing";
import type { IncidentPriority } from "~/interfaces/IIncident";
import { priorityLabels, priorityTextColors } from "~/lib/labels";

type PriorityMenuProps = {
  priority: IncidentPriority;
  onChange: (priority: IncidentPriority) => void;
};

const priorities: IncidentPriority[] = ["Low", "Medium", "High", "Critical"];

export function PriorityMenu({ priority, onChange }: PriorityMenuProps) {
  return (
    <DetailMenu
      label="Change priority"
      trigger={
        <span className={`flex items-center gap-[7px] ${priorityTextColors[priority]}`}>
          <StatusRing colorClass={priorityTextColors[priority]} />
          {priorityLabels[priority]}
        </span>
      }
    >
      {priorities.map((option) => (
        <DetailMenuItem
          key={option}
          selected={option === priority}
          leftSection={<StatusRing colorClass={priorityTextColors[option]} />}
          onClick={() => onChange(option)}
        >
          {priorityLabels[option]}
        </DetailMenuItem>
      ))}
    </DetailMenu>
  );
}
