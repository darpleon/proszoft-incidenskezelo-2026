import {
  ColorSchemeScript,
  Container,
  MantineProvider,
  Text,
  Title,
  mantineHtmlProps,
} from "@mantine/core";
import {
  isRouteErrorResponse,
  Links,
  Meta,
  Outlet,
  Scripts,
  ScrollRestoration,
} from "react-router";

import type { Route } from "./+types/root";

import "./app.css";

export const links: Route.LinksFunction = () => [
  { rel: "icon", href: "/favicon.svg", type: "image/svg+xml" },
];

export function Layout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="hu" {...mantineHtmlProps}>
      <head>
        <meta charSet="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <ColorSchemeScript forceColorScheme="light" />
        <Meta />
        <Links />
      </head>
      <body>
        <MantineProvider forceColorScheme="light">{children}</MantineProvider>
        <ScrollRestoration />
        <Scripts />
      </body>
    </html>
  );
}

export default function App() {
  return <Outlet />;
}

export function ErrorBoundary({ error }: Route.ErrorBoundaryProps) {
  let title = "Hiba történt";
  let message = "Váratlan hiba lépett fel.";

  if (isRouteErrorResponse(error)) {
    title = error.status === 404 ? "404" : "Hiba";
    message =
      error.status === 404
        ? "A keresett oldal nem található."
        : error.statusText || message;
  }

  return (
    <Container size="sm" py="xl">
      <Title order={1}>{title}</Title>
      <Text c="dimmed" mt="xs">
        {message}
      </Text>
    </Container>
  );
}
