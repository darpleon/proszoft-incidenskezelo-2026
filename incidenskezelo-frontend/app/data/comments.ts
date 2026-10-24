import type { IComment } from "~/interfaces/IComment";
import { minutesAgo } from "./mockTime";

export const comments: IComment[] = [
  {
    commentId: 1,
    incidentId: 2041,
    authorUserId: 3,
    authorName: "Kovács Anna",
    body: "Connection pool is full on postgres-main. orders-api keeps 200 idle connections open after the last deploy.\nRestarting orders-api pods one by one.",
    createdAtUtc: minutesAgo(16),
  },
  {
    commentId: 2,
    incidentId: 2041,
    authorUserId: 4,
    authorName: "Tóth Bence",
    body: "Rollback of orders-api started, will update in 10 minutes.",
    createdAtUtc: minutesAgo(9),
  },
  {
    commentId: 3,
    incidentId: 2040,
    authorUserId: 5,
    authorName: "Szabó Dóra",
    body: "Checked the batch-worker logs, the job fails on the same input file every time.",
    createdAtUtc: minutesAgo(40),
  },
];
