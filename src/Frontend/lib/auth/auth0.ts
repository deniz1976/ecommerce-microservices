import { createAuth0Client, type Auth0Client } from "@auth0/auth0-spa-js"

export interface Auth0Config {
  domain: string
  clientId: string
  audience: string
}

export interface LoginOptions {
  returnTo?: string
  screenHint?: "login" | "signup"
}

let auth0ClientPromise: Promise<Auth0Client> | null = null
let redirectHandled = false

export function getAuth0Config(): Auth0Config {
  return {
    domain: process.env.NEXT_PUBLIC_AUTH0_DOMAIN ?? "",
    clientId: process.env.NEXT_PUBLIC_AUTH0_CLIENT_ID ?? "",
    audience: process.env.NEXT_PUBLIC_AUTH0_AUDIENCE ?? "",
  }
}

export function isAuth0Configured(): boolean {
  const { domain, clientId, audience } = getAuth0Config()
  return domain.length > 0 && clientId.length > 0 && audience.length > 0
}

export async function loginWithAuth0(options: LoginOptions = {}): Promise<void> {
  const client = await getAuth0Client()

  await client.loginWithRedirect({
    appState: {
      returnTo: options.returnTo ?? "/",
    },
    authorizationParams: {
      audience: getAuth0Config().audience,
      redirect_uri: window.location.origin,
      screen_hint: options.screenHint === "signup" ? "signup" : undefined,
    },
  })
}

export async function logoutFromAuth0(returnTo = "/login"): Promise<void> {
  const client = await getAuth0Client()

  await client.logout({
    logoutParams: {
      returnTo: `${window.location.origin}${returnTo}`,
    },
  })
}

export async function getAccessToken(): Promise<string | null> {
  const client = await getAuth0Client()
  await handleAuth0RedirectIfNeeded(client)

  const isAuthenticated = await client.isAuthenticated()
  if (!isAuthenticated) {
    return null
  }

  return client.getTokenSilently({
    authorizationParams: {
      audience: getAuth0Config().audience,
    },
  })
}

async function getAuth0Client(): Promise<Auth0Client> {
  if (!isAuth0Configured()) {
    throw new Error("Auth0 public environment variables are not configured.")
  }

  auth0ClientPromise ??= createAuth0Client({
    domain: getAuth0Config().domain,
    clientId: getAuth0Config().clientId,
    authorizationParams: {
      audience: getAuth0Config().audience,
      redirect_uri: window.location.origin,
    },
    cacheLocation: "localstorage",
    useRefreshTokens: true,
  })

  return auth0ClientPromise
}

async function handleAuth0RedirectIfNeeded(client: Auth0Client): Promise<void> {
  if (redirectHandled || typeof window === "undefined") {
    return
  }

  const params = new URLSearchParams(window.location.search)
  if (!params.has("code") || !params.has("state")) {
    redirectHandled = true
    return
  }

  const result = await client.handleRedirectCallback()
  redirectHandled = true

  const returnTo =
    typeof result.appState?.returnTo === "string"
      ? result.appState.returnTo
      : "/"

  if (returnTo !== window.location.pathname) {
    window.location.replace(returnTo)
    return
  }

  window.history.replaceState({}, document.title, returnTo)
}
