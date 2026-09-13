import type { Route } from "./+types/home";

export function meta({}: Route.MetaArgs) {
  return [
    { title: "Szimuláció" },
    { name: "description", content: "Szimuláció alkalmazás" },
  ];
}

export default function Home() {
  return <h1>Szimuláció</h1>;
}
