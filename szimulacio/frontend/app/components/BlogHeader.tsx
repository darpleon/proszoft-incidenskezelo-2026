import { Container, Group, Text, Title } from "@mantine/core";

export function BlogHeader() {
  return (
    <header className="border-b border-header-line bg-header py-3">
      <Container size="sm">
        <Group justify="space-between" align="baseline">
          <Title order={1} className="text-brand text-lg tracking-tight">
            Blog
          </Title>
          <Text size="xs" ff="monospace" className="text-header-muted">
            blog-web
          </Text>
        </Group>
      </Container>
    </header>
  );
}
