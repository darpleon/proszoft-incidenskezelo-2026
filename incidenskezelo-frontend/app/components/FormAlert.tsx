import { Alert } from "@mantine/core";

type FormAlertProps = {
  title: string;
  message: string;
  color?: "red" | "teal";
};

export function FormAlert({ title, message, color = "red" }: FormAlertProps) {
  return (
    <Alert color={color} title={title} mb="md" role="alert">
      {message}
    </Alert>
  );
}
