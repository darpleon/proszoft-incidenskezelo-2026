import { Outlet } from "react-router";
import { AppHeader } from "~/components/AppHeader";

export default function AppLayout() {
  return (
    <>
      <AppHeader />
      <div className="h-[calc(100vh-54px)] bg-canvas p-2.5 max-[920px]:h-auto max-[920px]:min-h-[calc(100vh-54px)] max-[920px]:p-2">
        <div className="relative flex h-full overflow-hidden rounded-[11px] bg-panel shadow-panel max-[920px]:h-auto max-[920px]:min-h-[calc(100vh-70px)]">
          <Outlet />
        </div>
      </div>
    </>
  );
}
