import { Menu, UnstyledButton } from "@mantine/core";
import { IconChevronDown } from "@tabler/icons-react";
import type { ReactNode } from "react";

type FilterMenuProps = {
  label: string;
  value: string;
  active: boolean;
  closeOnItemClick?: boolean;
  children: ReactNode;
};

export function FilterMenu({ label, value, active, closeOnItemClick = false, children }: FilterMenuProps) {
  return (
    <Menu position="bottom-start" width={232} shadow="md" closeOnItemClick={closeOnItemClick}>
      <Menu.Target>
        <UnstyledButton
          className={`flex items-center gap-[5px] rounded-[5px] border px-2.5 py-[5px] text-[13px] whitespace-nowrap transition-colors motion-reduce:transition-none ${
            active
              ? "border-petrol-line bg-petrol-bg text-text"
              : "border-transparent text-text-2 hover:bg-panel-2 hover:text-text aria-expanded:border-line aria-expanded:bg-panel-2"
          }`}
        >
          {label}
          {value && <span className="font-semibold">{value}</span>}
          <IconChevronDown size={13} className="text-text-3" />
        </UnstyledButton>
      </Menu.Target>

      <Menu.Dropdown>{children}</Menu.Dropdown>
    </Menu>
  );
}
