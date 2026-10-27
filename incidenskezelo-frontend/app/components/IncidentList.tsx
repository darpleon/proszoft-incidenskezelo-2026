import { Button, SegmentedControl, Text, Title } from "@mantine/core";
import { IconPlus } from "@tabler/icons-react";
import { useState } from "react";
import { Link, useMatch } from "react-router";
import { EmptyState } from "./EmptyState";
import { IncidentListItem } from "./IncidentListItem";
import useAuth from "~/hooks/useAuth";
import type { IIncident } from "~/interfaces/IIncident";
import { isOpen } from "~/lib/labels";

type IncidentListProps = {
  incidents: IIncident[];
  loadError: boolean;
};

type OwnerFilter = "all" | "mine";

export function IncidentList({ incidents, loadError }: IncidentListProps) {
  const [ownerFilter, setOwnerFilter] = useState<OwnerFilter>("all");
  const { user } = useAuth();
  const isCreating = useMatch("/incidents/new") !== null;

  const visibleIncidents = incidents
    .filter((incident) => ownerFilter === "all" || incident.assigneeUserId === user?.userId)
    .sort((a, b) => b.createdAtUtc.localeCompare(a.createdAtUtc));
  const openCount = visibleIncidents.filter((incident) => isOpen(incident.status)).length;

  return (
    <section className="flex w-[312px] shrink-0 flex-col border-r border-list-line bg-list-bg max-[1280px]:w-[272px] max-[920px]:max-h-[42vh] max-[920px]:w-full max-[920px]:border-r-0 max-[920px]:border-b">
      <div className="flex items-center justify-between gap-3 border-b border-list-line px-4 pt-[13px] pb-[11px]">
        <Title order={2} className="text-sm font-semibold tracking-tight">
          Incidents
        </Title>
        <Button
          component={Link}
          to="/incidents/new"
          size="compact-xs"
          radius="sm"
          color="#0f717d"
          aria-label="New incident"
          leftSection={<IconPlus size={14} />}
          className={isCreating ? "ring-2 ring-petrol-bg ring-offset-2 ring-offset-list-bg" : undefined}
        >
          New
        </Button>
      </div>

      <div className="flex items-center justify-between gap-3 px-4 pt-2.5 pb-1">
        <Text size="xs" className="text-text-2">
          {openCount} open
        </Text>
        <SegmentedControl
          size="xs"
          value={ownerFilter}
          onChange={(value) => setOwnerFilter(value as OwnerFilter)}
          data={[
            { value: "all", label: "All" },
            { value: "mine", label: "Mine" },
          ]}
        />
      </div>

      <div className="min-h-0 flex-1 overflow-y-auto pb-2">
        {visibleIncidents.map((incident) => (
          <IncidentListItem key={incident.id} incident={incident} />
        ))}
        {loadError && <EmptyState>The incidents could not be loaded.</EmptyState>}
        {!loadError && visibleIncidents.length === 0 && <EmptyState>No incidents to show.</EmptyState>}
      </div>
    </section>
  );
}
