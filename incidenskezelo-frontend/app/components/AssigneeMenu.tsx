import { Menu, UnstyledButton } from "@mantine/core";
import { IconCheck, IconChevronDown } from "@tabler/icons-react";
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
    <Menu position="bottom-start" width={218} shadow="md">
      <Menu.Target>
        <UnstyledButton className="-mx-2 -my-[3px] flex items-center gap-[7px] rounded px-2 py-[3px] text-sm font-semibold tracking-[-0.015em] hover:bg-line">
          {assignee ? (
            <>
              <Avatar name={getInitials(assignee.name)} active />
              {assignee.name}
            </>
          ) : (
            <span className="text-high">Unassigned</span>
          )}
          <IconChevronDown size={12} className="text-text-3" />
        </UnstyledButton>
      </Menu.Target>

      <Menu.Dropdown>
        <Menu.Label>Assign to</Menu.Label>
        {users.map((user) => (
          <Menu.Item
            key={user.userId}
            leftSection={<Avatar name={getInitials(user.name)} />}
            rightSection={user.userId === assigneeUserId && <IconCheck size={14} className="text-petrol" />}
            className={user.userId === assigneeUserId ? "font-semibold" : undefined}
            onClick={() => onChange(user.userId)}
          >
            {user.name}
          </Menu.Item>
        ))}
        <Menu.Divider />
        <Menu.Item leftSection={<Avatar name="–" />} disabled={assigneeUserId === null} onClick={() => onChange(null)}>
          Remove assignee
        </Menu.Item>
      </Menu.Dropdown>
    </Menu>
  );
}
