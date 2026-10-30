import { useEffect } from "react";
import { useRevalidator } from "react-router";
import { autoRefreshIntervalMs } from "~/constants/constants";

const useAutoRefresh = () => {
  const { revalidate } = useRevalidator();

  useEffect(() => {
    const refresh = () => {
      if (document.visibilityState === "visible") {
        revalidate();
      }
    };

    const timer = window.setInterval(refresh, autoRefreshIntervalMs);
    document.addEventListener("visibilitychange", refresh);

    return () => {
      window.clearInterval(timer);
      document.removeEventListener("visibilitychange", refresh);
    };
  }, [revalidate]);
};

export default useAutoRefresh;
