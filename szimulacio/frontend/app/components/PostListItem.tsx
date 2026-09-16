import { Stack, Text, Title } from "@mantine/core";
import { Link } from "react-router";

import type { Post } from "../data/posts";
import { formatCommentCount, formatPostDate } from "../lib/format";

type PostListItemProps = {
  post: Post;
};

export function PostListItem({ post }: PostListItemProps) {
  return (
    <article
      className="grid gap-1 border-t border-[var(--mantine-color-gray-2)] py-5
        first:border-t-0 first:pt-0 sm:grid-cols-[7.5rem_1fr] sm:gap-5"
    >
      <Stack gap={2} className="tabular-nums sm:pt-1">
        <Text size="sm" c="dimmed">
          {formatPostDate(post.publishedAtUtc)}
        </Text>
        <Text size="xs" c="dimmed">
          {formatCommentCount(post.commentCount)}
        </Text>
      </Stack>

      <div>
        <Title
          order={2}
          className="mb-1 text-lg leading-snug font-semibold text-balance"
        >
          <Link
            to={`/post/${post.slug}`}
            className="text-black no-underline hover:text-brand hover:underline"
          >
            {post.title}
          </Link>
        </Title>
        <Text size="sm" c="dimmed">
          {post.excerpt}
        </Text>
      </div>
    </article>
  );
}
