import { Outlet } from "react-router";
import api from "~/api/api";
import { IncidentFilterBar } from "~/components/IncidentFilterBar";
import { IncidentList } from "~/components/IncidentList";
import { isDefaultFilter, readFilter, toApiParams } from "~/lib/incidentFilter";
import type { Route } from "./+types/incidents";

export function meta({}: Route.MetaArgs) {
  return [{ title: "Incidents | Incident Manager" }];
}

export async function clientLoader({ request }: Route.ClientLoaderArgs) {
  const filter = readFilter(new URL(request.url).searchParams);
  const [incidents, services, users] = await Promise.allSettled([
    api.Incident.getAll(toApiParams(filter)),
    api.Service.getAll(),
    api.User.getAssignable(),
  ]);

  return {
    filter,
    incidents: incidents.status === "fulfilled" ? incidents.value.data : [],
    loadError: incidents.status === "rejected",
    services: services.status === "fulfilled" ? services.value.data : [],
    users: users.status === "fulfilled" ? users.value.data : [],
  };
}

export default function Incidents({ loaderData }: Route.ComponentProps) {
  const { filter, incidents, loadError, services, users } = loaderData;

  return (
    <div className="flex min-h-0 w-full min-w-0 flex-col">
      <IncidentFilterBar filter={filter} count={incidents.length} services={services} users={users} />
      <div className="flex min-h-0 flex-1 max-[920px]:flex-col">
        <IncidentList incidents={incidents} loadError={loadError} filtered={!isDefaultFilter(filter)} />
        <main className="flex min-h-0 min-w-0 flex-1 flex-col">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
