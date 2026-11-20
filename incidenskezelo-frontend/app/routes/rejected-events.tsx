import { TextInput, Title } from "@mantine/core";
import { useState } from "react";
import api from "~/api/api";
import { EmptyState } from "~/components/EmptyState";
import { RejectedEventsTable } from "~/components/RejectedEventsTable";
import type { IRejectedEvent } from "~/interfaces/IRejectedEvent";
import type { Route } from "./+types/rejected-events";

export function meta({}: Route.MetaArgs) {
  return [{ title: "Rejected events | Incident Manager" }];
}

export async function clientLoader() {
  try {
    const response = await api.RejectedEvent.getLatest();
    return { rejectedEvents: response.data, loadError: false };
  } catch {
    return { rejectedEvents: [] as IRejectedEvent[], loadError: true };
  }
}

function matchesSearch(rejected: IRejectedEvent, search: string) {
  const text = search.trim().toLowerCase();
  if (!text) return true;

  return `${rejected.error} ${rejected.rawPayload}`.toLowerCase().includes(text);
}

export default function RejectedEvents({ loaderData }: Route.ComponentProps) {
  const { rejectedEvents, loadError } = loaderData;
  const [search, setSearch] = useState("");

  const visibleRejectedEvents = rejectedEvents.filter((rejected) => matchesSearch(rejected, search));

  return (
    <div className="flex min-h-0 min-w-0 flex-1 flex-col">
      <div className="px-[22px] pt-[22px] max-[680px]:px-4">
        <Title order={1} className="text-2xl leading-tight font-semibold tracking-tight max-[680px]:text-xl">
          Rejected events
        </Title>
      </div>

      <div className="flex flex-wrap items-center gap-3 px-[22px] pt-4 pb-3 max-[680px]:px-4">
        <p className="text-[11.5px] text-text-3">Latest {rejectedEvents.length} rejected events</p>
        <TextInput
          value={search}
          onChange={(event) => setSearch(event.currentTarget.value)}
          placeholder="Search by error or payload"
          aria-label="Search by error or payload"
          size="sm"
          className="ml-auto min-w-[200px] flex-[0_1_260px] max-[680px]:ml-0 max-[680px]:basis-full"
        />
      </div>

      <div className="min-h-0 flex-1 overflow-auto px-[22px] pb-[22px] max-[920px]:overflow-visible max-[680px]:px-4">
        {loadError && <EmptyState>The rejected events could not be loaded.</EmptyState>}
        {!loadError && rejectedEvents.length === 0 && <EmptyState>No rejected events.</EmptyState>}
        {rejectedEvents.length > 0 && visibleRejectedEvents.length === 0 && (
          <EmptyState>No rejected events match the search.</EmptyState>
        )}
        {visibleRejectedEvents.length > 0 && <RejectedEventsTable rejectedEvents={visibleRejectedEvents} />}
      </div>
    </div>
  );
}
