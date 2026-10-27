import { Outlet } from "react-router";
import api from "~/api/api";
import { IncidentList } from "~/components/IncidentList";
import type { IIncident } from "~/interfaces/IIncident";
import type { Route } from "./+types/incidents";

export function meta({}: Route.MetaArgs) {
  return [{ title: "Incidents | Incident Manager" }];
}

export async function clientLoader() {
  try {
    const response = await api.Incident.getAll();
    return { incidents: response.data, loadError: false };
  } catch {
    return { incidents: [] as IIncident[], loadError: true };
  }
}

export default function Incidents({ loaderData }: Route.ComponentProps) {
  return (
    <div className="flex w-full min-w-0 max-[920px]:flex-col">
      <IncidentList incidents={loaderData.incidents} loadError={loaderData.loadError} />
      <main className="flex min-h-0 min-w-0 flex-1 flex-col">
        <Outlet />
      </main>
    </div>
  );
}
