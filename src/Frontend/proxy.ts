import { auth0 } from "./lib/auth/auth0-server"

export async function proxy(request: Request) {
  return auth0.middleware(request)
}

export const config = {
  matcher: [
    "/((?!_next/static|_next/image|favicon.ico|sitemap.xml|robots.txt).*)",
  ],
}
