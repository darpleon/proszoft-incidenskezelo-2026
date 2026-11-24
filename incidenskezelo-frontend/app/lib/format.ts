export function formatAge(fromUtc: string): string {
  const minutes = Math.floor((Date.now() - new Date(fromUtc).getTime()) / 60_000);

  if (minutes < 1) return "just now";
  if (minutes < 60) return `${minutes} min ago`;

  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours} h ago`;

  return `${Math.floor(hours / 24)} d ago`;
}

const timeFormat = new Intl.DateTimeFormat("en-GB", {
  hour: "2-digit",
  minute: "2-digit",
  second: "2-digit",
  timeZone: "Europe/Budapest",
});

export function formatTime(utc: string): string {
  return timeFormat.format(new Date(utc));
}

const dayKeyFormat = new Intl.DateTimeFormat("en-CA", { timeZone: "Europe/Budapest" });

const dayFormat = new Intl.DateTimeFormat("en-GB", {
  day: "numeric",
  month: "short",
  year: "numeric",
  timeZone: "Europe/Budapest",
});

export function formatDay(utc: string): string {
  const day = dayKeyFormat.format(new Date(utc));
  const today = dayKeyFormat.format(new Date());
  const yesterday = dayKeyFormat.format(new Date(Date.now() - 86_400_000));

  if (day === today) return "Today";
  if (day === yesterday) return "Yesterday";
  return dayFormat.format(new Date(utc));
}
