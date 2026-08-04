import { getAccessToken } from "@/lib/auth/auth0"
import type { Locale } from "@/lib/i18n/dictionaries"
import { getStoredLocale } from "@/lib/i18n/locale"

export const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5080"

export class ApiError extends Error {
  status: number
  data: unknown

  constructor(message: string, status: number, data: unknown) {
    super(message)
    this.name = "ApiError"
    this.status = status
    this.data = data
  }
}

export class AuthenticationRequiredError extends Error {
  constructor() {
    super("Authentication is required for this request.")
    this.name = "AuthenticationRequiredError"
  }
}

interface RequestOptions extends Omit<RequestInit, "body"> {
  body?: unknown
  authenticated?: boolean
  locale?: Locale
}

export async function apiRequest<T>(
  path: string,
  { body, authenticated = false, locale, headers, ...init }: RequestOptions = {},
): Promise<T> {
  const requestHeaders = new Headers(headers)
  requestHeaders.set("Accept", "application/json")
  requestHeaders.set("Accept-Language", locale ?? getStoredLocale())
  const isFormData = typeof FormData !== "undefined" && body instanceof FormData

  if (body !== undefined && !isFormData) {
    requestHeaders.set("Content-Type", "application/json")
  }

  if (authenticated) {
    const token = await getAccessToken()
    if (!token) {
      throw new AuthenticationRequiredError()
    }

    requestHeaders.set("Authorization", `Bearer ${token}`)
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: requestHeaders,
    body: body === undefined
      ? undefined
      : isFormData
        ? body
        : JSON.stringify(body),
  })

  const isJson = response.headers
    .get("content-type")
    ?.includes("application/json")
  const data = isJson ? await response.json().catch(() => null) : null

  if (!response.ok) {
    throw new ApiError(
      `Request to ${path} failed with status ${response.status}`,
      response.status,
      data,
    )
  }

  return data as T
}
