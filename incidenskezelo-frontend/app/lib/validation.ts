export const PASSWORD_MIN_LENGTH = 10;

export function isValidEmail(email: string) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim());
}

export const COMMENT_MAX_LENGTH = 2000;

export const INCIDENT_TITLE_MAX_LENGTH = 200;

export const INCIDENT_DESCRIPTION_MAX_LENGTH = 4000;
