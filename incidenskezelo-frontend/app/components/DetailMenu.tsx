import { Menu, UnstyledButton } from "@mantine/core";
import { IconCheck, IconChevronDown } from "@tabler/icons-react";
import type { ReactNode } from "react";

type DetailMenuProps = {
  label: string;
  trigger: ReactNode;
  children: ReactNode;
};

type DetailMenuItemProps = {
  selected?: boolean;
  disabled?: boolean;
  leftSection: ReactNode;
  onClick: () => void;
  children: ReactNode;
};

export function DetailMenu({ label, trigger, children }: DetailMenuProps) {
  return (
    <Menu position="bottom-start" width={218} shadow="md">
      <Menu.Target>
        <UnstyledButton className="-mx-2 -my-[3px] flex items-center gap-[7px] rounded px-2 py-[3px] text-sm font-semibold tracking-[-0.015em] hover:bg-line">
          {trigger}
          <IconChevronDown size={12} className="text-text-3" />
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
