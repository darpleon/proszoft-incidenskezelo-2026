import { Title } from "@mantine/core";
import type { ReactNode } from "react";
import { AppLogo } from "./AppLogo";

type AuthLayoutProps = {
  title: string;
  children: ReactNode;
  footer: ReactNode;
};

export function AuthLayout({ title, children, footer }: AuthLayoutProps) {
  return (
    <main className="flex min-h-dvh items-center justify-center bg-panel px-[22px] py-8 max-[640px]:px-4 max-[640px]:pt-[22px] max-[700px]:items-start">
      <div className="w-full max-w-[364px] max-[640px]:max-w-none">
        <div className="mb-8 flex items-center gap-2.5 max-[640px]:mb-[22px]">
          <AppLogo size={26} />
          <span className="text-sm font-semibold tracking-tight">Incident Manager</span>
        </div>

        <Title order={1} className="mb-[22px] text-2xl leading-tight font-semibold tracking-tight max-[640px]:text-[21px]">
          {title}
        </Title>

        {children}

        <p className="mt-[22px] border-t border-line pt-4 text-[13px] text-text-2">{footer}</p>
      </div>
    </main>
  );
}
