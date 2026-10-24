import { Menu } from "@mantine/core";
import { DetailMenu, DetailMenuItem } from "./DetailMenu";
import { UserAvatar } from "./UserAvatar";
import type { IUser } from "~/interfaces/IUser";

type AssigneeMenuProps = {
  users: IUser[];
  assigneeUserId: number | null;
  onChange: (userId: number | null) => void;
};

export function AssigneeMenu({ users, assigneeUserId, onChange }: AssigneeMenuProps) {
  const assignee = users.find((user) => user.userId === assigneeUserId);

  return (
    <DetailMenu
      label="Assign to"
      trigger={
        assignee ? (
          <>
            <UserAvatar name={assignee.name} />
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
          leftSection={<UserAvatar name={user.name} muted />}
          onClick={() => onChange(user.userId)}
        >
          {user.name}
        </DetailMenuItem>
      ))}
      <Menu.Divider />
      <DetailMenuItem leftSection={<UserAvatar name="–" muted />} disabled={assigneeUserId === null} onClick={() => onChange(null)}>
        Remove assignee
      </DetailMenuItem>
    </DetailMenu>
  );
}
