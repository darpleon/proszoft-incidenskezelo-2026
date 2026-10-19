import { isAxiosError } from "axios";
import { redirect, useNavigate } from "react-router";
import api from "~/api/api";
import { AccountForm, type AccountFormValues } from "~/components/AccountForm";
import { AuthLayout } from "~/components/AuthLayout";
import useAuth from "~/hooks/useAuth";
import { needsSetup } from "~/lib/setup";
import type { Route } from "./+types/setup";

export function meta({}: Route.MetaArgs) {
  return [{ title: "Set up | Incident Manager" }];
}

export async function clientLoader() {
  if (!(await needsSetup())) {
    throw redirect("/login");
  }
  return null;
}

export default function Setup() {
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSetup = async ({ name, email, password }: AccountFormValues) => {
    try {
      await api.User.createFirstAdmin(name, email, password);
    } catch (error) {
      const status = isAxiosError(error) ? error.response?.status : undefined;
      if (status === 409) {
        navigate("/login");
        return null;
      }
      return "The admin account could not be created. Please try again later.";
    }

    const result = await login(email, password);
    navigate(result.success ? "/users" : "/login");
    return null;
  };

  return (
    <AuthLayout title="Create the admin account" footer="This page is only available until the first admin account exists.">
      <AccountForm
        submitLabel="Create admin account"
        errorTitle="Setup failed"
        note={
          <>
            This account gets the <b className="font-semibold text-text">Admin</b> role. You can approve other users after signing in.
          </>
        }
        onSubmit={handleSetup}
      />
    </AuthLayout>
  );
}
