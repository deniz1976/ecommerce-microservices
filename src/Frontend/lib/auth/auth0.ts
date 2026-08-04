"use client"

import { getAccessToken as getBffAccessToken } from "@auth0/nextjs-auth0/client"

export interface LoginOptions {
  returnTo?: string
  screenHint?: "login" | "signup"
}

export async function loginWithAuth0(
  options: LoginOptions = {},
): Promise<void> {
  const returnTo = normalizeLocalReturnPath(options.returnTo, "/")
  const query = new URLSearchParams({ returnTo })

  if (options.screenHint === "signup") {
    query.set("screen_hint", "signup")
  }

  window.location.assign(`/auth/login?${query.toString()}`)
}

export async function logoutFromAuth0(returnTo = "/login"): Promise<void> {
  const localReturnTo = normalizeLocalReturnPath(returnTo, "/login")
  const absoluteReturnTo = new URL(localReturnTo, window.location.origin)

  window.location.assign(
    `/auth/logout?returnTo=${encodeURIComponent(absoluteReturnTo.toString())}`,
  )
}

export async function getAccessToken(): Promise<string | null> {
  try {
    return await getBffAccessToken()
  } catch {
    return null
  }
}

export async function refreshAccessToken(): Promise<string> {
  const response = await fetch("/auth/refresh-access-token", {
    method: "POST",
    headers: {
      Accept: "application/json",
    },
  })

  if (!response.ok) {
    throw new Error("Access token refresh failed.")
  }

  return getBffAccessToken()
}

function normalizeLocalReturnPath(candidate: unknown, fallback: string): string {
  if (
    typeof candidate !== "string" ||
    !candidate.startsWith("/") ||
    candidate.startsWith("//") ||
    candidate.includes("\\")
  ) {
    return fallback
  }

  const resolved = new URL(candidate, window.location.origin)
  if (resolved.origin !== window.location.origin) {
    return fallback
  }

  return `${resolved.pathname}${resolved.search}${resolved.hash}`
}
