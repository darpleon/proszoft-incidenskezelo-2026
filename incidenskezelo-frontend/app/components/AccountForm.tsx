import { Button, PasswordInput, TextInput } from "@mantine/core";
import { useState, type FormEvent, type ReactNode } from "react";
import { FormAlert } from "~/components/FormAlert";
import { isValidEmail, PASSWORD_MIN_LENGTH } from "~/lib/validation";

type FieldErrors = { name?: string; email?: string; password?: string; passwordConfirm?: string };

export type AccountFormValues = { name: string; email: string; password: string };

type AccountFormProps = {
  submitLabel: string;
  errorTitle: string;
  note: ReactNode;
  onSubmit: (values: AccountFormValues) => Promise<string | null>;
};

export function AccountForm({ submitLabel, errorTitle, note, onSubmit }: AccountFormProps) {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [passwordConfirm, setPasswordConfirm] = useState("");
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const passwordsMatch = password === passwordConfirm;

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    const errors: FieldErrors = {
      name: name.trim().length > 1 ? undefined : "Enter your name.",
      email: isValidEmail(email) ? undefined : "Enter a valid email address.",
      password: password.length >= PASSWORD_MIN_LENGTH ? undefined : `Use at least ${PASSWORD_MIN_LENGTH} characters.`,
      passwordConfirm: passwordsMatch ? undefined : "The two passwords don't match.",
    };
    setFieldErrors(errors);
    if (Object.values(errors).some(Boolean)) return;

    setIsSubmitting(true);
    setFormError(null);
    const error = await onSubmit({ name: name.trim(), email: email.trim(), password });
    setFormError(error);
    setIsSubmitting(false);
  };

  return (
    <>
      {formError && <FormAlert title={errorTitle} message={formError} />}

      <form onSubmit={handleSubmit} noValidate>
        <TextInput
          label="Full name"
          autoComplete="name"
          placeholder="Example Name"
          value={name}
          onChange={(event) => setName(event.currentTarget.value)}
          error={fieldErrors.name}
          mb="md"
        />
        <TextInput
          label="Email address"
          type="email"
          autoComplete="email"
          inputMode="email"
          placeholder="name@example.com"
          value={email}
          onChange={(event) => setEmail(event.currentTarget.value)}
          error={fieldErrors.email}
          mb="md"
        />
        <PasswordInput
          label="Password"
          autoComplete="new-password"
          placeholder={`At least ${PASSWORD_MIN_LENGTH} characters`}
          value={password}
          onChange={(event) => setPassword(event.currentTarget.value)}
          error={fieldErrors.password}
          mb="md"
        />
        <PasswordInput
          label="Confirm password"
          autoComplete="new-password"
          placeholder="••••••••"
          value={passwordConfirm}
          onChange={(event) => setPasswordConfirm(event.currentTarget.value)}
          error={fieldErrors.passwordConfirm}
          description={
            passwordConfirm && !fieldErrors.passwordConfirm ? (
              <span className={passwordsMatch ? "text-ok" : "text-crit"}>
                {passwordsMatch ? "The passwords match." : "The passwords don't match."}
              </span>
            ) : undefined
          }
          inputWrapperOrder={["label", "input", "description", "error"]}
          mb="md"
        />

        <div className="mb-4 rounded-lg bg-petrol-bg px-3 py-2.5 text-[13px] leading-normal text-text-2">{note}</div>

        <Button type="submit" fullWidth size="md" color="#0f717d" loading={isSubmitting} mt="xs">
          {submitLabel}
        </Button>
      </form>
    </>
  );
}
