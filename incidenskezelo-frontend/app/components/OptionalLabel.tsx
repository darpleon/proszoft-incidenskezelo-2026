export function OptionalLabel({ children }: { children: string }) {
  return (
    <>
      {children} <span className="font-normal text-text-3">optional</span>
    </>
  );
}
