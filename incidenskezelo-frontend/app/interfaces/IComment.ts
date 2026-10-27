export interface IComment {
  commentId: number;
  incidentId: string;
  authorUserId: number;
  authorName: string;
  body: string;
  createdAtUtc: string;
  editedAtUtc: string | null;
}
