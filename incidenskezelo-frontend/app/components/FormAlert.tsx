import { Alert } from "@mantine/core";

type FormAlertProps = {
  title: string;
  message: string;
  color?: "red" | "teal";
  onClose?: () => void;
};

export function FormAlert({ title, message, color = "red", onClose }: FormAlertProps) {
  return (
    <Alert color={color} title={title} mb="md" role="alert" withCloseButton={onClose !== undefined} onClose={onClose}>
      {message}
    </Alert>
  );
}
