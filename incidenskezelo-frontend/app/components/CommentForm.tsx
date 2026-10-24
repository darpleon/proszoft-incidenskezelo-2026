import { Button, Textarea } from "@mantine/core";
import { useState, type FormEvent, type KeyboardEvent } from "react";
import { COMMENT_MAX_LENGTH } from "~/lib/validation";

type CommentFormProps = {
  onSubmit: (body: string) => void;
};

export function CommentForm({ onSubmit }: CommentFormProps) {
  const [body, setBody] = useState("");
  const trimmedBody = body.trim();
  const charactersLeft = COMMENT_MAX_LENGTH - body.length;

  const submit = () => {
    if (!trimmedBody) return;
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
  };

  return (
    <form onSubmit={handleSubmit} className="mt-4 border-t border-line pt-4">
      <Textarea
        value={body}
        onChange={(event) => setBody(event.currentTarget.value)}
        onKeyDown={handleKeyDown}
        placeholder="Add a comment for the team…"
        aria-label="Comment"
        autosize
        minRows={3}
        maxRows={10}
        maxLength={COMMENT_MAX_LENGTH}
      />
      <div className="mt-2 flex items-center justify-between gap-2">
        <span className={`text-[11.5px] ${charactersLeft < 50 ? "text-crit" : "text-text-3"}`}>
          {charactersLeft < 200 ? `${charactersLeft} characters left` : "Ctrl+Enter to post"}
        </span>
        <Button type="submit" size="xs" color="#0f717d" disabled={!trimmedBody}>
          Post comment
        </Button>
      </div>
    </form>
  );
}
