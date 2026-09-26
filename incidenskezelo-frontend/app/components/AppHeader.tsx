import { Button, Group, Text } from "@mantine/core";
import { NavLink } from "react-router";
import { BackendStatus } from "./BackendStatus";
import { currentUser } from "~/data/currentUser";

const navItems = [
  { to: "/incidents", label: "Incidensek" },
  { to: "/events", label: "Események" },
];

function getInitials(name: string) {
  return name
    .split(" ")
    .map((part) => part[0])
    .join("");
}

export function AppHeader() {
  return (
    <header className="flex h-[54px] items-center gap-6 bg-nav-bg px-4 text-nav-text">
      <Group gap={9} wrap="nowrap">
        <div className="grid size-[25px] place-items-center rounded-[5px] bg-petrol text-xs font-bold text-white">
          I
        </div>
        <Text fw={600} className="text-nav-text-hi tracking-tight">
          Incidenskezelő
        </Text>
      </Group>

      <nav className="flex items-center gap-1">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `rounded-[5px] px-3 py-[7px] text-sm transition-colors hover:bg-nav-bg-2 hover:text-nav-text-hi ${
                isActive ? "bg-nav-bg-2 font-semibold text-nav-text-hi" : ""
              }`
            }
          >
            {item.label}
          </NavLink>
        ))}
      </nav>

      <Group gap="md" ml="auto" wrap="nowrap">
        <BackendStatus />
        <div className="h-6 w-px bg-nav-line" />
        <Group gap={9} wrap="nowrap">
          <div className="grid size-7 place-items-center rounded-full border border-nav-line bg-nav-bg-2 text-[10px] font-bold text-nav-text-hi">
            {getInitials(currentUser.name)}
          </div>
          <div className="flex flex-col leading-tight">
            <Text size="sm" fw={500} className="text-nav-text-hi">
              {currentUser.name}
            </Text>
            <Text size="xs" className="text-nav-dim">
              {currentUser.role}
            </Text>
          </div>
        </Group>
        <Button variant="default" size="xs" className="border-nav-line bg-transparent text-nav-text hover:bg-nav-bg-2 hover:text-nav-text-hi">
          Kilépés
        </Button>
      </Group>
    </header>
  );
}
