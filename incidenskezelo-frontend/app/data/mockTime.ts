const ONE_HOUR = 3_600_000;

const mockNow = Math.floor(Date.now() / ONE_HOUR) * ONE_HOUR;

export function minutesAgo(minutes: number) {
  return new Date(mockNow - minutes * 60_000).toISOString();
}
