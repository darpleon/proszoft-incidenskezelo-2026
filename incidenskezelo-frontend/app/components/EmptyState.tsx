import { Text } from "@mantine/core";
import type { ReactNode } from "react";

export function EmptyState({ children }: { children: ReactNode }) {
  return (
    <Text size="sm" className="px-4 py-6 text-center text-text-3">
      {children}
    </Text>
  );
}
