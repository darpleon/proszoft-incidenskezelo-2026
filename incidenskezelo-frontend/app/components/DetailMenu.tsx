import { Menu, UnstyledButton } from "@mantine/core";
import { IconCheck, IconChevronDown } from "@tabler/icons-react";
import type { ReactNode } from "react";

export type DetailMenuVariant = "inline" | "field" | "button";

type DetailMenuProps = {
  label: string;
  trigger: ReactNode;
  variant?: DetailMenuVariant;
  position?: "bottom-start" | "bottom-end";
  disabled?: boolean;
  children: ReactNode;
};

const triggerClasses: Record<DetailMenuVariant, string> = {
  inline: "-mx-2 -my-[3px] rounded px-2 py-[3px] hover:bg-line",
  field: "h-9 w-full rounded border border-line bg-panel px-2.5 hover:bg-panel-2 aria-expanded:border-petrol",
  button:
    "h-[30px] rounded-r border border-l-0 border-line bg-panel px-3 hover:bg-panel-2 disabled:cursor-not-allowed disabled:bg-panel-2 disabled:text-text-3",
};

type DetailMenuItemProps = {
  selected?: boolean;
  disabled?: boolean;
  leftSection: ReactNode;
  onClick: () => void;
  children: ReactNode;
};

export function DetailMenu({ label, trigger, variant = "inline", position = "bottom-start", disabled, children }: DetailMenuProps) {
  return (
    <Menu position={position} width={variant === "field" ? "target" : 218} shadow="md" disabled={disabled}>
      <Menu.Target>
        <UnstyledButton
          disabled={disabled}
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
