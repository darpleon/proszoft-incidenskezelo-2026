import { Badge, Button, Select, Table, Text, Title } from "@mantine/core";
import { isAxiosError } from "axios";
import { useEffect, useState } from "react";
import { redirect } from "react-router";
import api from "~/api/api";
import { FormAlert } from "~/components/FormAlert";
import useAuth from "~/hooks/useAuth";
import type { IUser, UserRole } from "~/interfaces/IUser";
import { readAuthUser } from "~/lib/authToken";
import type { Route } from "./+types/users";

const roleOptions: { value: UserRole; label: string }[] = [
  { value: "Pending", label: "Pending" },
  { value: "Operator", label: "Operator" },
  { value: "Admin", label: "Admin" },
];

export function meta({}: Route.MetaArgs) {
  return [{ title: "Users | Incident Manager" }];
}

export function clientLoader() {
  if (readAuthUser()?.role !== "Admin") {
    throw redirect("/incidents");
  }
  return null;
}

function errorMessage(error: unknown) {
  if (isAxiosError(error) && error.response?.status === 400 && typeof error.response.data?.detail === "string") {
    return error.response.data.detail;
  }
  return "The change could not be saved. Please try again.";
}

export default function Users() {
  const { user: currentUser } = useAuth();
  const [users, setUsers] = useState<IUser[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [savingUserId, setSavingUserId] = useState<number | null>(null);

  useEffect(() => {
    api.User.getUsers()
      .then((response) => setUsers(response.data))
      .catch(() => setError("The user list could not be loaded."))
      .finally(() => setIsLoading(false));
  }, []);

  const replaceUser = (updated: IUser) => {
    setUsers((current) => current.map((item) => (item.userId === updated.userId ? updated : item)));
  };

  const runChange = async (userId: number, change: () => Promise<{ data: IUser }>) => {
    setSavingUserId(userId);
    setError(null);
    try {
      replaceUser((await change()).data);
    } catch (changeError) {
      setError(errorMessage(changeError));
    } finally {
      setSavingUserId(null);
    }
  };

  const pendingCount = users.filter((item) => item.role === "Pending" && item.isActive).length;

  return (
    <div className="flex min-h-0 min-w-0 flex-1 flex-col">
      <div className="px-[22px] pt-[22px] max-[680px]:px-4">
        <Title order={1} className="text-2xl leading-tight font-semibold tracking-tight max-[680px]:text-xl">
          Users
        </Title>
        <Text size="xs" className="mt-1 text-text-3">
          {pendingCount === 0 ? "No registrations waiting for approval." : `${pendingCount} waiting for approval.`}
        </Text>
      </div>

      <div className="min-h-0 flex-1 overflow-auto px-[22px] pt-4 pb-[22px] max-[920px]:overflow-visible max-[680px]:px-4">
        {error && <FormAlert title="Something went wrong" message={error} />}

        {!isLoading && (
          <div className="overflow-x-auto">
            <Table className="min-w-[640px] text-[13px]" verticalSpacing="sm" highlightOnHover>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Name</Table.Th>
                  <Table.Th>Email</Table.Th>
                  <Table.Th>Role</Table.Th>
                  <Table.Th>Status</Table.Th>
                  <Table.Th />
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {users.map((item) => {
                  const isSelf = item.userId === currentUser?.userId;
                  const isSaving = savingUserId === item.userId;

                  return (
                    <Table.Tr key={item.userId} className={item.role === "Pending" && item.isActive ? "bg-[#fdf7ec]" : undefined}>
                      <Table.Td className="font-medium">
                        {item.name}
                        {isSelf && <span className="ml-1.5 text-text-3">(you)</span>}
                      </Table.Td>
                      <Table.Td className="text-text-2">{item.email}</Table.Td>
                      <Table.Td>
                        <Select
                          size="xs"
                          w={130}
                          data={roleOptions}
                          value={item.role}
                          allowDeselect={false}
                          disabled={isSelf || isSaving}
                          aria-label={`Role of ${item.name}`}
                          onChange={(role) =>
                            role && role !== item.role && runChange(item.userId, () => api.User.updateRole(item.userId, role as UserRole))
                          }
                        />
                      </Table.Td>
                      <Table.Td>
                        <Badge variant="light" color={item.isActive ? "teal" : "gray"}>
                          {item.isActive ? "Active" : "Deactivated"}
                        </Badge>
                      </Table.Td>
                      <Table.Td className="text-right">
                        {!isSelf && (
                          <Button
                            size="xs"
                            variant="default"
                            loading={isSaving}
                            onClick={() => runChange(item.userId, () => api.User.setActive(item.userId, !item.isActive))}
                          >
                            {item.isActive ? "Deactivate" : "Reactivate"}
                          </Button>
                        )}
                      </Table.Td>
                    </Table.Tr>
                  );
                })}
              </Table.Tbody>
            </Table>
          </div>
        )}
      </div>
    </div>
  );
}
