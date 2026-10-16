import { Anchor, Text } from "@mantine/core";
import { isAxiosError } from "axios";
import { useState } from "react";
import { Link, redirect } from "react-router";
import api from "~/api/api";
import { AccountForm, type AccountFormValues } from "~/components/AccountForm";
import { AuthLayout } from "~/components/AuthLayout";
import { readAuthUser } from "~/lib/authToken";
import { needsSetup } from "~/lib/setup";
import type { Route } from "./+types/register";

export function meta({}: Route.MetaArgs) {
  return [{ title: "Register | Incident Manager" }];
}

export async function clientLoader() {
  if (readAuthUser()) {
    throw redirect("/incidents");
  }
  if (await needsSetup()) {
    throw redirect("/setup");
  }
  return null;
}

export default function Register() {
  const [isRegistered, setIsRegistered] = useState(false);

  const handleRegister = async ({ name, email, password }: AccountFormValues) => {
    try {
      await api.User.register(name, email, password);
      setIsRegistered(true);
      return null;
    } catch (error) {
      const status = isAxiosError(error) ? error.response?.status : undefined;
      return status === 409
        ? "An account with this email address already exists."
        : "Registration is not available right now. Please try again later.";
    }
  };

  const footer = (
    <>
      Already have an account?{" "}
      <Anchor component={Link} to="/login" c="#0f717d">
        Sign in
      </Anchor>
    </>
  );

  if (isRegistered) {
    return (
      <AuthLayout title="Registration received" footer={footer}>
        <Text size="sm" className="text-text-2">
          An admin needs to approve your account before you can sign in. You can sign in once your role has been assigned.
        </Text>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout title="Register" footer={footer}>
      <AccountForm
        submitLabel="Create account"
        errorTitle="Registration failed"
        note={
          <>
            New accounts need <b className="font-semibold text-text">admin approval</b> before they can sign in.
          </>
        }
        onSubmit={handleRegister}
      />
    </AuthLayout>
  );
}
