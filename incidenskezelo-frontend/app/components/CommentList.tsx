import type { IComment } from "~/interfaces/IComment";
import { formatAge } from "~/lib/format";
import { getInitials } from "~/lib/initials";

type CommentListProps = {
  comments: IComment[];
  currentUserId: number | undefined;
};

export function CommentList({ comments, currentUserId }: CommentListProps) {
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
      {comments.map((comment) => (
        <li key={comment.commentId} className="flex gap-3 border-t border-[#eff2f1] py-3.5 first:border-t-0 first:pt-0">
          <i className="grid size-7 shrink-0 place-items-center rounded-full bg-petrol-bg text-[10px] font-bold text-petrol not-italic">
            {getInitials(comment.authorName)}
          </i>
          <div className="min-w-0">
            <div className="flex flex-wrap items-baseline gap-2">
              <span className="text-sm font-semibold">
                {comment.authorName}
                {comment.authorUserId === currentUserId && <span className="font-normal text-text-3"> (you)</span>}
              </span>
              <span className="text-[11.5px] text-text-3" suppressHydrationWarning>
                {formatAge(comment.createdAtUtc)}
              </span>
            </div>
            <p className="mt-[3px] text-sm leading-relaxed break-words whitespace-pre-wrap">{comment.body}</p>
          </div>
        </li>
      ))}
    </ul>
  );
}
