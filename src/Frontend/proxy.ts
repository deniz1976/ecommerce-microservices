import { auth0 } from "./lib/auth/auth0-server"
import { NextRequest } from "next/server"

export async function proxy(request: NextRequest) {
  const nonce = btoa(crypto.randomUUID())
  const requestHeaders = new Headers(request.headers)
  requestHeaders.set("x-nonce", nonce)
  requestHeaders.set("Content-Security-Policy", createContentSecurityPolicy(nonce))

  const response = await auth0.middleware(
    new NextRequest(request, { headers: requestHeaders }),
  )
  response.headers.set("Content-Security-Policy", createContentSecurityPolicy(nonce))
  return response
}

export const config = {
  matcher: [
    "/((?!_next/static|_next/image|favicon.ico|sitemap.xml|robots.txt).*)",
  ],
}

function createContentSecurityPolicy(nonce: string): string {
  const apiOrigin = readHttpOrigin(
    process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:15080",
  )
  const websocketOrigin = apiOrigin.replace(/^http/, "ws")
  const developmentScripts = process.env.NODE_ENV === "production" ? "" : " 'unsafe-eval'"

  return [
    "default-src 'self'",
    `script-src 'self' 'nonce-${nonce}' 'strict-dynamic'${developmentScripts}`,
    "style-src 'self' 'unsafe-inline'",
    "img-src 'self' blob: data: https://res.cloudinary.com",
    "font-src 'self' data:",
    `connect-src 'self' ${apiOrigin} ${websocketOrigin}`,
    "frame-src 'self'",
    "worker-src 'self' blob:",
    "object-src 'none'",
    "base-uri 'self'",
    "form-action 'self'",
    "frame-ancestors 'none'",
    "manifest-src 'self'",
    ...(process.env.NODE_ENV === "production" ? ["upgrade-insecure-requests"] : []),
  ].join("; ")
}

function readHttpOrigin(value: string): string {
  const parsed = new URL(value)
  if (!["http:", "https:"].includes(parsed.protocol) || parsed.username || parsed.password) {
    throw new Error("NEXT_PUBLIC_API_BASE_URL must be an HTTP(S) origin without credentials.")
  }

  return parsed.origin
}
