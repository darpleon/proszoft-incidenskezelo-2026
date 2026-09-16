import { Container, Group, Text, Title } from "@mantine/core";

export function BlogHeader() {
  return (
    <header className="border-b border-[var(--mantine-color-gray-2)] bg-white py-3">
      <Container size="sm">
        <Group justify="space-between" align="baseline">
          <Title order={1} className="text-lg tracking-tight">
            Blog
          </Title>
          <Text size="xs" c="dimmed" ff="monospace">
            blog-web
          </Text>
        </Group>
      </Container>
    </header>
  );
}
