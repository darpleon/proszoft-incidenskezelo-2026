import { Anchor, Button, PasswordInput, TextInput } from "@mantine/core";
import { useState, type FormEvent } from "react";
import { Link, redirect, useNavigate } from "react-router";
import { AuthLayout } from "~/components/AuthLayout";
import { FormAlert } from "~/components/FormAlert";
import useAuth from "~/hooks/useAuth";
import { readAuthUser } from "~/lib/authToken";
import { needsSetup } from "~/lib/setup";
import { isValidEmail } from "~/lib/validation";
import type { Route } from "./+types/login";

export function meta({}: Route.MetaArgs) {
  return [{ title: "Sign in | Incident Manager" }];
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

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [fieldErrors, setFieldErrors] = useState<{ email?: string; password?: string }>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    const errors = {
      email: isValidEmail(email) ? undefined : "Enter a valid email address.",
      password: password.length > 0 ? undefined : "Password is required.",
    };
    setFieldErrors(errors);
    if (errors.email || errors.password) return;

    setIsSubmitting(true);
    const result = await login(email.trim(), password);
    setIsSubmitting(false);

    if (result.success) {
      navigate("/incidents");
    } else {
      setFormError(result.error);
    }
  };

  return (
    <AuthLayout
      title="Sign in"
      footer={
        <>
          Don't have an account?{" "}
          <Anchor component={Link} to="/register" c="#0f717d">
            Register
          </Anchor>
        </>
      }
    >
      {formError && <FormAlert title="Sign-in failed" message={formError} />}

      <form onSubmit={handleSubmit} noValidate>
        <TextInput
          label="Email address"
          type="email"
          autoComplete="username"
          inputMode="email"
          placeholder="name@example.com"
          value={email}
          onChange={(event) => setEmail(event.currentTarget.value)}
          error={fieldErrors.email}
          mb="md"
        />
        <PasswordInput
          label="Password"
          autoComplete="current-password"
          placeholder="••••••••"
          value={password}
          onChange={(event) => setPassword(event.currentTarget.value)}
          error={fieldErrors.password}
          mb="md"
        />
        <Button type="submit" fullWidth size="md" color="#0f717d" loading={isSubmitting} mt="xs">
          Sign in
        </Button>
      </form>
    </AuthLayout>
  );
}
