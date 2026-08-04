const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i

export function isOptionalGuid(value: string) {
  const normalized = value.trim()
  return normalized === "" || guidPattern.test(normalized)
}

export function toOptionalUtcIso(value: string) {
  return value ? new Date(value).toISOString() : undefined
}

export function isValidUtcRange(from?: string, to?: string) {
  return !from || !to || from <= to
}
