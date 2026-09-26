import { type RouteConfig, index, layout, route } from "@react-router/dev/routes";

export default [
  index("routes/home.tsx"),
  layout("routes/layout.tsx", [
    route("incidents", "routes/incidents.tsx"),
    route("events", "routes/events.tsx"),
  ]),
] satisfies RouteConfig;
