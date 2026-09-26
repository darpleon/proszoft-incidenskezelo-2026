import { Outlet } from "react-router";
import { IncidentList } from "~/components/IncidentList";
import { incidents } from "~/data/incidents";
import type { Route } from "./+types/incidents";

export function meta({}: Route.MetaArgs) {
  return [{ title: "Incidensek | Incidenskezelő" }];
}

export default function Incidents() {
  return (
    <div className="flex w-full min-w-0 max-[920px]:flex-col">
      <IncidentList incidents={incidents} />
      <main className="flex min-h-0 min-w-0 flex-1 flex-col">
        <Outlet />
      </main>
    </div>
  );
}
