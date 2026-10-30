import { ActionIcon, Button, Textarea, TextInput } from "@mantine/core";
import { IconCheck, IconX } from "@tabler/icons-react";
import { useState, type KeyboardEvent, type ReactNode } from "react";

type InlineEditProps = {
  value: string;
  label: string;
  maxLength: number;
  onSave: (value: string) => Promise<void>;
  multiline?: boolean;
  inputClassName?: string;
  children: ReactNode;
};

export function InlineEdit({ value, label, maxLength, onSave, multiline = false, inputClassName, children }: InlineEditProps) {
  const [draft, setDraft] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  const startEditing = () => {
    setDraft(value);
    setError(null);
  };

  const cancel = () => {
    setDraft(null);
    setError(null);
  };

  const save = async () => {
    const trimmed = (draft ?? "").trim();
    if (!trimmed) {
      setError(`${label} is required.`);
      return;
    }
    if (trimmed === value) {
      cancel();
      return;
    }

    setIsSaving(true);
    await onSave(trimmed);
    setIsSaving(false);
    cancel();
  };

  const handleKeyDown = (event: KeyboardEvent) => {
    if (event.key === "Escape") {
      event.preventDefault();
      cancel();
    }
    if (event.key === "Enter" && (!multiline || event.ctrlKey || event.metaKey)) {
      event.preventDefault();
      save();
    }
  };

  if (draft === null) {
    return (
      <div
        role="button"
        tabIndex={0}
        aria-label={`Edit ${label.toLowerCase()}`}
        onClick={startEditing}
        onKeyDown={(event) => event.key === "Enter" && startEditing()}
        className="-mx-2 cursor-text rounded px-2 py-1 transition-colors hover:bg-[#eff2f1] focus-visible:outline-2 focus-visible:outline-petrol"
      >
        {children}
      </div>
    );
  }

  const inputProps = {
    value: draft,
    onChange: (event: { currentTarget: { value: string } }) => {
      setDraft(event.currentTarget.value);
      setError(null);
    },
    onKeyDown: handleKeyDown,
    error,
    maxLength,
    autoFocus: true,
    disabled: isSaving,
    "aria-label": label,
    classNames: { input: inputClassName },
  };

  if (multiline) {
    return (
      <div className="max-w-[70ch]">
        <Textarea {...inputProps} autosize minRows={4} maxRows={16} />
        <div className="mt-2 flex items-center gap-2">
          <Button size="xs" color="#0f717d" onClick={save} loading={isSaving}>
            Save
          </Button>
          <Button size="xs" variant="default" onClick={cancel} disabled={isSaving}>
            Cancel
          </Button>
          <span className="text-[11.5px] text-text-3">Ctrl+Enter to save, Esc to cancel</span>
        </div>
      </div>
    );
  }

  return (
    <div className="-mx-2">
      <TextInput {...inputProps} />
      <div className="mt-1.5 flex justify-end gap-1.5">
        <ActionIcon variant="filled" color="#0f717d" size="md" aria-label="Save" onClick={save} loading={isSaving}>
          <IconCheck size={16} />
        </ActionIcon>
        <ActionIcon variant="default" size="md" aria-label="Cancel" onClick={cancel} disabled={isSaving}>
          <IconX size={16} />
        </ActionIcon>
      </div>
    </div>
  );
}
