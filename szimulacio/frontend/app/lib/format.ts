const postDateFormat = new Intl.DateTimeFormat("hu-HU", {
  year: "numeric",
  month: "short",
  day: "numeric",
  timeZone: "UTC",
});

export function formatPostDate(publishedAtUtc: string): string {
  return postDateFormat.format(new Date(publishedAtUtc));
}

export function formatCommentCount(count: number): string {
  return count === 0 ? "nincs hozzászólás" : `${count} hozzászólás`;
}
