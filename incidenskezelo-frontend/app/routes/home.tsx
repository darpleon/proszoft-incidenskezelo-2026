import type { Route } from "./+types/home";

export function meta({}: Route.MetaArgs) {
  return [
    { title: "Incidenskezelő" },
    { name: "description", content: "Incidenskezelő alkalmazás" },
  ];
}

export default function Home() {
  return <h1>Incidenskezelő</h1>;
}
