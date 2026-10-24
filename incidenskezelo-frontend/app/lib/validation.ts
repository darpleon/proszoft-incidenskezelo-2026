export const PASSWORD_MIN_LENGTH = 10;

export function isValidEmail(email: string) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim());
}

export const COMMENT_MAX_LENGTH = 2000;
