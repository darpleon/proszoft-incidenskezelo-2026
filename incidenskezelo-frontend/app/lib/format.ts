export function formatAge(fromUtc: string): string {
  const minutes = Math.floor((Date.now() - new Date(fromUtc).getTime()) / 60_000);

  if (minutes < 1) return "most";
  if (minutes < 60) return `${minutes} perce`;

  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours} órája`;

  return `${Math.floor(hours / 24)} napja`;
}

const timeFormat = new Intl.DateTimeFormat("hu-HU", {
  hour: "2-digit",
  minute: "2-digit",
  second: "2-digit",
  timeZone: "Europe/Budapest",
});

export function formatTime(utc: string): string {
  return timeFormat.format(new Date(utc));
}
