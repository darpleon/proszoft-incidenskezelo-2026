import { Button, Textarea } from "@mantine/core";
import { useState, type FormEvent, type KeyboardEvent } from "react";
import { COMMENT_MAX_LENGTH } from "~/lib/validation";

type CommentFormProps = {
  onSubmit: (body: string) => void;
  initialBody?: string;
  submitLabel?: string;
  onCancel?: () => void;
};

export function CommentForm({ onSubmit, initialBody = "", submitLabel = "Post comment", onCancel }: CommentFormProps) {
  const [body, setBody] = useState(initialBody);
  const trimmedBody = body.trim();
  const canSubmit = trimmedBody !== "" && trimmedBody !== initialBody;
  const charactersLeft = COMMENT_MAX_LENGTH - body.length;

  const submit = () => {
    if (!canSubmit) return;
    onSubmit(trimmedBody);
    setBody("");
  };

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault();
    submit();
  };

  const handleKeyDown = (event: KeyboardEvent<HTMLTextAreaElement>) => {
    if (event.key === "Enter" && (event.ctrlKey || event.metaKey)) {
      event.preventDefault();
      submit();
    }

    if (event.key === "Escape" && onCancel) {
      event.preventDefault();
      onCancel();
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <Textarea
        value={body}
        onChange={(event) => setBody(event.currentTarget.value)}
        onKeyDown={handleKeyDown}
        placeholder="Add a comment for the team…"
        aria-label="Comment"
        autoFocus={onCancel !== undefined}
        autosize
        minRows={3}
        maxRows={10}
        maxLength={COMMENT_MAX_LENGTH}
      />
      <div className="mt-2 flex items-center justify-between gap-2">
        <span className={`text-[11.5px] ${charactersLeft < 50 ? "text-crit" : "text-text-3"}`}>
          {charactersLeft < 200 ? `${charactersLeft} characters left` : `Ctrl+Enter to ${onCancel ? "save" : "post"}`}
        </span>
        <div className="flex gap-2">
          {onCancel && (
            <Button variant="default" size="xs" onClick={onCancel}>
              Cancel
            </Button>
          )}
          <Button type="submit" size="xs" color="#0f717d" disabled={!canSubmit}>
            {submitLabel}
          </Button>
        </div>
      </div>
    </form>
  );
}
