import api from "~/api/api";

export async function needsSetup() {
  try {
    return (await api.User.getSetupStatus()).data.needsSetup;
  } catch {
    return false;
  }
}
