import { TextInput, Title } from "@mantine/core";
import { useState } from "react";
import api from "~/api/api";
import { EmptyState } from "~/components/EmptyState";
import { EventsTable } from "~/components/EventsTable";
import type { IEvent } from "~/interfaces/IEvent";
import type { Route } from "./+types/events";

export function meta({}: Route.MetaArgs) {
  return [{ title: "Events | Incident Manager" }];
}

export async function clientLoader() {
  try {
    const response = await api.Event.getLatest();
    return { events: response.data, loadError: false };
  } catch {
    return { events: [] as IEvent[], loadError: true };
  }
}

function matchesSearch(event: IEvent, search: string) {
  const text = search.trim().toLowerCase();
  if (!text) return true;

  return `evt-${event.eventId}`.includes(text) || event.serviceCode.includes(text);
}

export default function Events({ loaderData }: Route.ComponentProps) {
  const { events, loadError } = loaderData;
  const [search, setSearch] = useState("");

  const visibleEvents = events
    .filter((event) => matchesSearch(event, search))
    .sort((a, b) => b.occurredAtUtc.localeCompare(a.occurredAtUtc));

  return (
    <div className="flex min-h-0 min-w-0 flex-1 flex-col">
      <div className="px-[22px] pt-[22px] max-[680px]:px-4">
        <Title order={1} className="text-2xl leading-tight font-semibold tracking-tight max-[680px]:text-xl">
          Events
        </Title>
      </div>

      <div className="flex flex-wrap items-center gap-3 px-[22px] pt-4 pb-3 max-[680px]:px-4">
        <p className="text-[11.5px] text-text-3">Latest {events.length} events</p>
        <TextInput
          value={search}
          onChange={(event) => setSearch(event.currentTarget.value)}
          placeholder="Search by ID or source"
          aria-label="Search by ID or source"
          size="sm"
          className="ml-auto min-w-[200px] flex-[0_1_260px] max-[680px]:ml-0 max-[680px]:basis-full"
        />
      </div>

      <div className="min-h-0 flex-1 overflow-auto px-[22px] pb-[22px] max-[920px]:overflow-visible max-[680px]:px-4">
        {loadError && <EmptyState>The events could not be loaded.</EmptyState>}
        {!loadError && events.length === 0 && <EmptyState>No events received yet.</EmptyState>}
        {events.length > 0 && visibleEvents.length === 0 && <EmptyState>No events match the search.</EmptyState>}
        {visibleEvents.length > 0 && <EventsTable events={visibleEvents} />}
      </div>
    </div>
  );
}
