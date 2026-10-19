export type UserRole = "Pending" | "Operator" | "Admin";

export interface IUser {
  userId: number;
  name: string;
  email: string;
  role: UserRole;
  isActive: boolean;
  createdAtUtc: string;
}

export interface IAuthUser {
  userId: number;
  name: string;
  email: string;
  role: UserRole;
}

export interface ILoginResponse {
  token: string;
  expiresAtUtc: string;
  user: IUser;
}
