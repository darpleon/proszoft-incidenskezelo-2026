import { Button, Group, Text } from "@mantine/core";
import { NavLink } from "react-router";
import { BackendStatus } from "./BackendStatus";
import { currentUser } from "~/data/currentUser";
import { getInitials } from "~/lib/initials";

const navItems = [
  { to: "/incidents", label: "Incidensek" },
  { to: "/events", label: "Események" },
];

export function AppHeader() {
  return (
    <header className="flex h-[54px] items-center gap-6 bg-nav-bg px-4 text-nav-text max-[680px]:gap-3 max-[680px]:px-3">
      <Group gap={9} wrap="nowrap">
        <div className="grid size-[25px] place-items-center rounded-[5px] bg-petrol text-xs font-bold text-white">
          I
        </div>
        <Text fw={600} className="text-nav-text-hi tracking-tight max-[680px]:hidden">
          Incidenskezelő
        </Text>
      </Group>

      <nav className="flex items-center gap-1">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              `rounded-[5px] px-3 py-[7px] text-sm whitespace-nowrap transition-colors motion-reduce:transition-none hover:bg-nav-bg-2 hover:text-nav-text-hi max-[680px]:px-2.5 max-[680px]:text-[13px] ${
                isActive ? "bg-nav-bg-2 font-semibold text-nav-text-hi" : ""
              }`
            }
          >
            {item.label}
          </NavLink>
        ))}
      </nav>

      <Group gap="md" ml="auto" wrap="nowrap">
        <div className="max-[1080px]:hidden">
          <BackendStatus />
        </div>
        <div className="h-6 w-px bg-nav-line max-[680px]:hidden" />
        <Group gap={9} wrap="nowrap">
          <div className="grid size-7 place-items-center rounded-full border border-nav-line bg-nav-bg-2 text-[10px] font-bold text-nav-text-hi">
            {getInitials(currentUser.name)}
          </div>
          <div className="flex flex-col leading-tight max-[1080px]:hidden">
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
