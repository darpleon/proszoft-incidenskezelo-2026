import { Button, Title } from "@mantine/core";
import { IconPlus } from "@tabler/icons-react";
import { Link, useLocation, useMatch } from "react-router";
import { EmptyState } from "./EmptyState";
import { IncidentListItem } from "./IncidentListItem";
import type { IIncident } from "~/interfaces/IIncident";

type IncidentListProps = {
  incidents: IIncident[];
  loadError: boolean;
  filtered: boolean;
};

export function IncidentList({ incidents, loadError, filtered }: IncidentListProps) {
  const { search } = useLocation();
  const isCreating = useMatch("/incidents/new") !== null;

  return (
    <section className="flex w-[312px] shrink-0 flex-col border-r border-list-line bg-list-bg max-[1280px]:w-[272px] max-[920px]:max-h-[42vh] max-[920px]:w-full max-[920px]:border-r-0 max-[920px]:border-b">
      <div className="flex items-center justify-between gap-3 border-b border-list-line px-4 pt-[13px] pb-[11px]">
        <Title order={2} className="text-sm font-semibold tracking-tight">
          Incidents
        </Title>
        <Button
          component={Link}
          to={{ pathname: "/incidents/new", search }}
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

      <div className="min-h-0 flex-1 overflow-y-auto pt-1 pb-2">
        {incidents.map((incident) => (
          <IncidentListItem key={incident.id} incident={incident} />
        ))}
        {loadError && <EmptyState>The incidents could not be loaded.</EmptyState>}
        {!loadError && incidents.length === 0 && (
          <EmptyState>{filtered ? "No incidents match these filters." : "No active incidents."}</EmptyState>
        )}
      </div>
    </section>
  );
}
