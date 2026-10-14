import type { IUser } from "~/interfaces/IUser";

export const users: IUser[] = [
  { userId: 3, name: "Kovács Anna", email: "anna@example.com", role: "Operator", isActive: true, createdAtUtc: "2026-09-01T08:00:00Z" },
  { userId: 4, name: "Tóth Bence", email: "bence@example.com", role: "Operator", isActive: true, createdAtUtc: "2026-09-01T08:00:00Z" },
  { userId: 5, name: "Szabó Dóra", email: "dora@example.com", role: "Operator", isActive: true, createdAtUtc: "2026-09-01T08:00:00Z" },
];
