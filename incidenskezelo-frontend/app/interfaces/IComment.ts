export interface IComment {
  commentIndex: number;
  actorUserId: number;
  actorName: string;
  body: string;
  createdAtUtc: string;
  editedAtUtc: string | null;
  bodyRevision: number;
}
