import "server-only"

import { Auth0Client } from "@auth0/nextjs-auth0/server"

const audience = process.env.AUTH0_AUDIENCE

export const auth0 = new Auth0Client({
  authorizationParameters: {
    scope: "openid profile email offline_access",
    ...(audience ? { audience } : {}),
  },
  enableAccessTokenEndpoint: true,
  signInReturnToPath: "/",
  session: {
    rolling: true,
    absoluteDuration: 60 * 60 * 8,
    inactivityDuration: 60 * 60,
    cookie: {
      sameSite: "lax",
      secure: process.env.NODE_ENV === "production",
      path: "/",
    },
  },
  tokenRefreshBuffer: 60,
})
