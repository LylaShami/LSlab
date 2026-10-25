export function errorText(error: unknown): string {
  return String(error).replace(/^Error:\s*/, "");
}