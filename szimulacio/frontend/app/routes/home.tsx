import { Container } from "@mantine/core";

import { BlogHeader } from "../components/BlogHeader";
import { PostListItem } from "../components/PostListItem";
import { posts } from "../data/posts";
import type { Route } from "./+types/home";

export function meta({}: Route.MetaArgs) {
  return [
    { title: "Blog" },
    {
      name: "description",
      content: "Receptek, képek, hobbik és egyéb hétköznapi dolgok.",
    },
  ];
}

export default function Home() {
  return (
    <>
      <BlogHeader />
      <Container size="sm" py="xl">
        {posts.map((post) => (
          <PostListItem key={post.postId} post={post} />
        ))}
      </Container>
    </>
  );
}
