import { Menu } from "@mantine/core";
import { DetailMenu, DetailMenuItem, type DetailMenuVariant } from "./DetailMenu";
import { UserAvatar } from "./UserAvatar";
import type { IAssignableUser } from "~/interfaces/IUser";

type AssigneeMenuProps = {
  users: IAssignableUser[];
  assigneeUserId: number | null;
  assigneeName: string | null;
  onChange: (userId: number | null) => void;
  variant?: DetailMenuVariant;
};

export function AssigneeMenu({ users, assigneeUserId, assigneeName, onChange, variant }: AssigneeMenuProps) {
  return (
    <DetailMenu
      label="Assign to"
      variant={variant}
      trigger={
        assigneeName ? (
          <>
            <UserAvatar name={assigneeName} />
            {assigneeName}
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
