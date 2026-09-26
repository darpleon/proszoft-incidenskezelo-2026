import { TextInput, Title } from "@mantine/core";
import { useState } from "react";
import { EventFilterChips, type EventFilter } from "~/components/EventFilterChips";
import { EventsTable } from "~/components/EventsTable";
import { events } from "~/data/events";
import { services } from "~/data/services";
import type { IEvent } from "~/interfaces/IEvent";
import { matchesEventFilter } from "~/lib/events";
import type { Route } from "./+types/events";

export function meta({}: Route.MetaArgs) {
  return [{ title: "Események | Incidenskezelő" }];
}

function matchesSearch(event: IEvent, search: string) {
  const text = search.trim().toLowerCase();
  if (!text) return true;

  const serviceCode = services.find((service) => service.serviceId === event.serviceId)?.code ?? "";
  return `evt-${event.eventId}`.includes(text) || serviceCode.includes(text);
}

export default function Events() {
  const [filter, setFilter] = useState<EventFilter>("all");
  const [search, setSearch] = useState("");

  const sortedEvents = [...events].sort((a, b) => b.occurredAtUtc.localeCompare(a.occurredAtUtc));
  const counts: Record<EventFilter, number> = {
    all: events.length,
    bound: events.filter((event) => matchesEventFilter(event, "bound")).length,
    unbound: events.filter((event) => matchesEventFilter(event, "unbound")).length,
    duplicate: events.filter((event) => matchesEventFilter(event, "duplicate")).length,
    failed: events.filter((event) => matchesEventFilter(event, "failed")).length,
  };
  const visibleEvents = sortedEvents.filter(
    (event) => matchesEventFilter(event, filter) && matchesSearch(event, search),
  );

  return (
    <div className="flex min-h-0 min-w-0 flex-1 flex-col">
      <div className="px-[22px] pt-[22px] max-[680px]:px-4">
        <Title order={1} className="text-2xl leading-tight font-semibold tracking-tight max-[680px]:text-xl">
          Események
        </Title>
      </div>

      <div className="flex flex-wrap items-center gap-3 px-[22px] pt-4 pb-3 max-[680px]:px-4">
        <EventFilterChips value={filter} counts={counts} onChange={setFilter} />
        <TextInput
          value={search}
          onChange={(event) => setSearch(event.currentTarget.value)}
          placeholder="Keresés azonosító vagy forrás szerint"
          aria-label="Keresés azonosító vagy forrás szerint"
          size="sm"
          className="ml-auto min-w-[200px] flex-[0_1_260px] max-[680px]:ml-0 max-[680px]:basis-full"
        />
      </div>

      <p className="px-[22px] pb-3 text-[11.5px] text-text-3 max-[680px]:px-4">
        {counts.all} esemény, ebből {counts.unbound} nem tartozik incidenshez, {counts.failed} feldolgozása sikertelen volt
      </p>

      <div className="min-h-0 flex-1 overflow-auto px-[22px] pb-[22px] max-[920px]:overflow-visible max-[680px]:px-4">
        <EventsTable events={visibleEvents} showIncident />
      </div>
    </div>
  );
}
