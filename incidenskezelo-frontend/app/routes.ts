import { type RouteConfig, index, layout, route } from "@react-router/dev/routes";

export default [
  index("routes/home.tsx"),
  route("login", "routes/login.tsx"),
  route("register", "routes/register.tsx"),
  route("setup", "routes/setup.tsx"),
  layout("routes/layout.tsx", [
    route("incidents", "routes/incidents.tsx", [
      index("routes/incidents-index.tsx"),
      route("new", "routes/incident-new.tsx"),
      route(":incidentId", "routes/incident-detail.tsx"),
    ]),
    route("events", "routes/events.tsx"),
    route("users", "routes/users.tsx"),
  ]),
] satisfies RouteConfig;
