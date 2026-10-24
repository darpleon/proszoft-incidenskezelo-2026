export interface IComment {
  commentId: number;
  incidentId: number;
  authorUserId: number;
  authorName: string;
  body: string;
  createdAtUtc: string;
  editedAtUtc: string | null;
}
