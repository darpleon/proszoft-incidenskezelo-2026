import { Menu } from "@mantine/core";
import { DetailMenu, DetailMenuItem } from "./DetailMenu";
import type { IUser } from "~/interfaces/IUser";
import { getInitials } from "~/lib/initials";

type AssigneeMenuProps = {
  users: IUser[];
  assigneeUserId: number | null;
  onChange: (userId: number | null) => void;
};

function Avatar({ name, active }: { name: string; active?: boolean }) {
  return (
    <i
      className={`grid size-[21px] shrink-0 place-items-center rounded-full text-[9px] font-bold not-italic ${
        active ? "bg-petrol-bg text-petrol" : "bg-line text-text-2"
      }`}
    >
      {name}
    </i>
  );
}

export function AssigneeMenu({ users, assigneeUserId, onChange }: AssigneeMenuProps) {
  const assignee = users.find((user) => user.userId === assigneeUserId);

  return (
    <DetailMenu
      label="Assign to"
      trigger={
        assignee ? (
          <>
            <Avatar name={getInitials(assignee.name)} active />
            {assignee.name}
          </>
        ) : (
          <span className="text-high">Unassigned</span>
        )
      }
    >
      {users.map((user) => (
        <DetailMenuItem
          key={user.userId}
          selected={user.userId === assigneeUserId}
          leftSection={<Avatar name={getInitials(user.name)} />}
          onClick={() => onChange(user.userId)}
        >
          {user.name}
        </DetailMenuItem>
      ))}
      <Menu.Divider />
      <DetailMenuItem leftSection={<Avatar name="–" />} disabled={assigneeUserId === null} onClick={() => onChange(null)}>
        Remove assignee
      </DetailMenuItem>
    </DetailMenu>
  );
}
