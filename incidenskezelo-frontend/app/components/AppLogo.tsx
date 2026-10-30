export function AppLogo({ size = 25 }: { size?: number }) {
  return <img src="/favicon.svg" alt="" width={size} height={size} className="shrink-0" />;
}
