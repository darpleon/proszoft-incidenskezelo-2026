import { Text, Tooltip } from "@mantine/core";

export function IncidentRef({ id }: { id: string }) {
  return (
    <Tooltip label={id} position="bottom-start" openDelay={300}>
      <Text component="span" size="xs" fw={500} className="cursor-default tracking-wider text-text-3">
        {id.slice(0, 8).toUpperCase()}
      </Text>
    </Tooltip>
  );
}
