import { Menu, Text, TextInput, UnstyledButton } from "@mantine/core";
import { useDebouncedValue } from "@mantine/hooks";
import { IconSearch } from "@tabler/icons-react";
import { useEffect, useState } from "react";
import { useSearchParams } from "react-router";
import { DetailMenuItem } from "./DetailMenu";
import { FilterMenu } from "./FilterMenu";
import { IncidentStatusRing, PriorityRing } from "./StatusRing";
import useAuth from "~/hooks/useAuth";
import type { IncidentPriority, IncidentStatus } from "~/interfaces/IIncident";
import type { CreatedRange, IIncidentFilter } from "~/interfaces/IIncidentFilter";
import type { IService } from "~/interfaces/IService";
import type { IAssignableUser } from "~/interfaces/IUser";
import { allStatuses, closedStatuses, isDefaultFilter, openStatuses, sameItems, writeFilter } from "~/lib/incidentFilter";
import { priorityLabels, statusLabels } from "~/lib/labels";

type IncidentFilterBarProps = {
  filter: IIncidentFilter;
  count: number;
  services: IService[];
  users: IAssignableUser[];
};

const allPriorities: IncidentPriority[] = ["Critical", "High", "Medium", "Low"];
const createdLabels: Record<CreatedRange, string> = {
  "": "Any time",
  "24h": "Last 24 hours",
  "7d": "Last 7 days",
  "30d": "Last 30 days",
  custom: "Custom range",
};

function toggle<T>(items: T[], item: T) {
  return items.includes(item) ? items.filter((value) => value !== item) : [...items, item];
}

function summary(labels: string[]) {
  if (labels.length <= 1) return labels[0] ?? "";
  return `${labels[0]} +${labels.length - 1}`;
}

export function IncidentFilterBar({ filter, count, services, users }: IncidentFilterBarProps) {
  const [, setSearchParams] = useSearchParams();
  const { user } = useAuth();
  const [search, setSearch] = useState(filter.q);
  const [debouncedSearch] = useDebouncedValue(search, 300);
  const myId = user ? String(user.userId) : "";

  const update = (changes: Partial<IIncidentFilter>, replace = false) => {
    setSearchParams(writeFilter({ ...filter, ...changes }), { replace });
  };

  useEffect(() => {
    if (debouncedSearch.trim() !== filter.q) {
      update({ q: debouncedSearch.trim() }, true);
    }
  }, [debouncedSearch]);

  const reset = () => {
    setSearch("");
    setSearchParams(new URLSearchParams());
  };

  const renderStatusItem = (status: IncidentStatus) => (
    <DetailMenuItem
      key={status}
      selected={filter.status.includes(status)}
      leftSection={<IncidentStatusRing status={status} />}
      onClick={() => update({ status: toggle(filter.status, status) })}
    >
      {statusLabels[status]}
    </DetailMenuItem>
  );

  const isOpenOnly = sameItems(filter.status, openStatuses);
  const hasAllActive = openStatuses.every((status) => filter.status.includes(status));
  const statusLabelList = hasAllActive
    ? ["Active", ...closedStatuses.filter((status) => filter.status.includes(status)).map((status) => statusLabels[status])]
    : filter.status.map((status) => statusLabels[status]);
  const statusValue = filter.status.length === 0 || filter.status.length === allStatuses.length ? "All" : summary(statusLabelList);

  const assigneeValue =
    filter.assignee === ""
      ? ""
      : filter.assignee === "none"
        ? "Unassigned"
        : filter.assignee === myId
          ? "Me"
          : (users.find((option) => String(option.userId) === filter.assignee)?.name ?? "Unknown user");

  const createdValue =
    filter.created === "custom" ? `${filter.from || "…"} – ${filter.to || "…"}` : filter.created ? createdLabels[filter.created] : "";

  return (
    <div className="flex flex-wrap items-center gap-1.5 border-b border-line bg-panel px-3 py-2.5">
      <TextInput
        value={search}
        onChange={(event) => setSearch(event.currentTarget.value)}
        placeholder="Search title"
        aria-label="Search incident titles"
        size="sm"
        leftSection={<IconSearch size={15} />}
        className="mr-1.5 w-[260px] max-[920px]:mr-0 max-[920px]:w-full"
      />

      <FilterMenu label="Status" value={statusValue} active={!isOpenOnly}>
        <Menu.Label>Active</Menu.Label>
        {openStatuses.map(renderStatusItem)}
        <Menu.Label>Closed</Menu.Label>
        {closedStatuses.map(renderStatusItem)}
        <Menu.Divider />
        <Menu.Item onClick={() => update({ status: openStatuses })}>Active only</Menu.Item>
        <Menu.Item onClick={() => update({ status: allStatuses })}>All statuses</Menu.Item>
      </FilterMenu>

      <FilterMenu label="Priority" value={summary(filter.priority.map((priority) => priorityLabels[priority]))} active={filter.priority.length > 0}>
        {allPriorities.map((priority) => (
          <DetailMenuItem
            key={priority}
            selected={filter.priority.includes(priority)}
            leftSection={<PriorityRing priority={priority} />}
            onClick={() => update({ priority: toggle(filter.priority, priority) })}
          >
            {priorityLabels[priority]}
          </DetailMenuItem>
        ))}
        {filter.priority.length > 0 && (
          <>
            <Menu.Divider />
            <Menu.Item onClick={() => update({ priority: [] })}>Clear</Menu.Item>
          </>
        )}
      </FilterMenu>

      <FilterMenu label="Service" value={summary(filter.service)} active={filter.service.length > 0}>
        {services.length === 0 && <Menu.Label>The services could not be loaded.</Menu.Label>}
        {services.map((service) => (
          <DetailMenuItem
            key={service.code}
            selected={filter.service.includes(service.code)}
            leftSection={null}
            onClick={() => update({ service: toggle(filter.service, service.code) })}
          >
            {service.code}
          </DetailMenuItem>
        ))}
        {filter.service.length > 0 && (
          <>
            <Menu.Divider />
            <Menu.Item onClick={() => update({ service: [] })}>Clear</Menu.Item>
          </>
        )}
      </FilterMenu>

      <FilterMenu label="Assignee" value={assigneeValue} active={filter.assignee !== ""} closeOnItemClick>
        <DetailMenuItem selected={filter.assignee === ""} leftSection={null} onClick={() => update({ assignee: "" })}>
          Anyone
        </DetailMenuItem>
        {myId && (
          <DetailMenuItem selected={filter.assignee === myId} leftSection={null} onClick={() => update({ assignee: myId })}>
            Me
          </DetailMenuItem>
        )}
        <DetailMenuItem selected={filter.assignee === "none"} leftSection={null} onClick={() => update({ assignee: "none" })}>
          Unassigned
        </DetailMenuItem>
        <Menu.Divider />
        {users
          .filter((option) => String(option.userId) !== myId)
          .map((option) => (
            <DetailMenuItem
              key={option.userId}
              selected={filter.assignee === String(option.userId)}
              leftSection={null}
              onClick={() => update({ assignee: String(option.userId) })}
            >
              {option.name}
            </DetailMenuItem>
          ))}
      </FilterMenu>

      <FilterMenu label="Created" value={createdValue} active={filter.created !== ""}>
        {(Object.keys(createdLabels) as CreatedRange[]).map((range) => (
          <DetailMenuItem key={range} selected={filter.created === range} leftSection={null} onClick={() => update({ created: range })}>
            {createdLabels[range]}
          </DetailMenuItem>
        ))}
        {filter.created === "custom" && (
          <div className="flex flex-col gap-1.5 px-2 pt-1 pb-1.5">
            <TextInput
              type="date"
              size="xs"
              label="From"
              value={filter.from}
              max={filter.to || undefined}
              onChange={(event) => update({ from: event.currentTarget.value }, true)}
            />
            <TextInput
              type="date"
              size="xs"
              label="To"
              value={filter.to}
              min={filter.from || undefined}
              onChange={(event) => update({ to: event.currentTarget.value }, true)}
            />
          </div>
        )}
      </FilterMenu>

      <div className="ml-auto flex items-center gap-3 max-[920px]:ml-0 max-[920px]:w-full">
        <Text size="xs" className="text-text-2">
          <span className="font-semibold text-text">{count}</span> {count === 1 ? "incident" : "incidents"}
        </Text>
        {!isDefaultFilter(filter) && (
          <UnstyledButton onClick={reset} className="text-xs font-semibold text-petrol hover:underline">
            Reset
          </UnstyledButton>
        )}
      </div>
    </div>
  );
}
