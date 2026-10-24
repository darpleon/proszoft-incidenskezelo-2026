import { ActionIcon } from "@mantine/core";
import { IconPencil } from "@tabler/icons-react";
import { useState } from "react";
import { CommentForm } from "./CommentForm";
import { UserAvatar } from "./UserAvatar";
import type { IComment } from "~/interfaces/IComment";
import { formatAge } from "~/lib/format";

type CommentListProps = {
  comments: IComment[];
  currentUserId: number | undefined;
  onEdit: (commentId: number, body: string) => void;
};

export function CommentList({ comments, currentUserId, onEdit }: CommentListProps) {
  const [editingCommentId, setEditingCommentId] = useState<number | null>(null);

  const saveEdit = (commentId: number, body: string) => {
    onEdit(commentId, body);
    setEditingCommentId(null);
  };

  if (comments.length === 0) {
    return (
      <div className="py-7 text-sm text-text-3">
        <b className="mb-0.5 block font-semibold text-text-2">No comments yet</b>
        Notes about findings, next steps or hand-overs go here.
      </div>
    );
  }

  return (
    <ul>
      {comments.map((comment) => {
        const isOwn = comment.authorUserId === currentUserId;
        const isEditing = comment.commentId === editingCommentId;

        return (
          <li key={comment.commentId} className="flex gap-3 border-t border-[#eff2f1] py-3.5 first:border-t-0 first:pt-0">
            <UserAvatar name={comment.authorName} size="md" />
            <div className="min-w-0 flex-1">
              <div className="flex flex-wrap items-baseline gap-2">
                <span className="text-sm font-semibold">
                  {comment.authorName}
                  {isOwn && <span className="font-normal text-text-3"> (you)</span>}
                </span>
                <span className="text-[11.5px] text-text-3" suppressHydrationWarning>
                  {formatAge(comment.createdAtUtc)}
                  {comment.editedAtUtc && ", edited"}
                </span>
                {isOwn && !isEditing && (
                  <ActionIcon
                    variant="subtle"
                    color="gray"
                    size="sm"
                    className="ml-auto self-center"
                    aria-label="Edit comment"
                    title="Edit comment"
                    onClick={() => setEditingCommentId(comment.commentId)}
                  >
                    <IconPencil size={14} />
                  </ActionIcon>
                )}
              </div>
              {isEditing ? (
                <div className="mt-2">
                  <CommentForm
                    initialBody={comment.body}
                    submitLabel="Save"
                    onSubmit={(body) => saveEdit(comment.commentId, body)}
                    onCancel={() => setEditingCommentId(null)}
                  />
                </div>
              ) : (
                <p className="mt-[3px] text-sm leading-relaxed break-words whitespace-pre-wrap">{comment.body}</p>
              )}
            </div>
          </li>
        );
      })}
    </ul>
  );
}
