import { Menu, UnstyledButton } from "@mantine/core";
import { IconCheck, IconChevronDown } from "@tabler/icons-react";
import type { ReactNode } from "react";

export type DetailMenuVariant = "inline" | "field";

type DetailMenuProps = {
  label: string;
  trigger: ReactNode;
  variant?: DetailMenuVariant;
  children: ReactNode;
};

const triggerClasses: Record<DetailMenuVariant, string> = {
  inline: "-mx-2 -my-[3px] rounded px-2 py-[3px] hover:bg-line",
  field: "h-9 w-full rounded border border-line bg-panel px-2.5 hover:bg-panel-2 aria-expanded:border-petrol",
};

type DetailMenuItemProps = {
  selected?: boolean;
  disabled?: boolean;
  leftSection: ReactNode;
  onClick: () => void;
  children: ReactNode;
};

export function DetailMenu({ label, trigger, variant = "inline", children }: DetailMenuProps) {
  return (
    <Menu position="bottom-start" width={variant === "field" ? "target" : 218} shadow="md">
      <Menu.Target>
        <UnstyledButton
          className={`flex items-center gap-[7px] text-sm font-semibold tracking-[-0.015em] ${triggerClasses[variant]}`}
        >
          {trigger}
          <IconChevronDown size={12} className={`text-text-3 ${variant === "field" ? "ml-auto" : ""}`} />
        </UnstyledButton>
      </Menu.Target>

      <Menu.Dropdown>
        <Menu.Label>{label}</Menu.Label>
        {children}
      </Menu.Dropdown>
    </Menu>
  );
}

export function DetailMenuItem({ selected, disabled, leftSection, onClick, children }: DetailMenuItemProps) {
  return (
    <Menu.Item
      leftSection={leftSection}
      rightSection={selected && <IconCheck size={14} className="text-petrol" />}
      className={selected ? "font-semibold" : undefined}
      disabled={disabled}
      onClick={onClick}
    >
      {children}
    </Menu.Item>
  );
}
