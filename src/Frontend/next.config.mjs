const isDevelopment = process.env.NODE_ENV !== "production"
const apiOrigin = readHttpOrigin(
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5080",
  "NEXT_PUBLIC_API_BASE_URL",
)
const auth0Origin = readAuth0Origin(process.env.NEXT_PUBLIC_AUTH0_DOMAIN)
const websocketOrigin = toWebSocketOrigin(apiOrigin)

const connectSources = [
  "'self'",
  apiOrigin,
  websocketOrigin,
  auth0Origin,
].filter(Boolean)

const contentSecurityPolicy = [
  "default-src 'self'",
  `script-src 'self' 'unsafe-inline'${isDevelopment ? " 'unsafe-eval'" : ""}`,
  "style-src 'self' 'unsafe-inline'",
  "img-src 'self' blob: data: https://res.cloudinary.com",
  "font-src 'self' data:",
  `connect-src ${connectSources.join(" ")}`,
  `frame-src 'self'${auth0Origin ? ` ${auth0Origin}` : ""}`,
  "worker-src 'self' blob:",
  "object-src 'none'",
  "base-uri 'self'",
  "form-action 'self'",
  "frame-ancestors 'none'",
  "manifest-src 'self'",
  ...(!isDevelopment ? ["upgrade-insecure-requests"] : []),
].join("; ")

const securityHeaders = [
  {
    key: "Content-Security-Policy",
    value: contentSecurityPolicy,
  },
  {
    key: "Referrer-Policy",
    value: "strict-origin-when-cross-origin",
  },
  {
    key: "X-Content-Type-Options",
    value: "nosniff",
  },
  {
    key: "X-Frame-Options",
    value: "DENY",
  },
  {
    key: "Permissions-Policy",
    value: "camera=(), microphone=(), geolocation=(), payment=(), usb=()",
  },
  ...(!isDevelopment
    ? [
        {
          key: "Strict-Transport-Security",
          value: "max-age=63072000; includeSubDomains",
        },
      ]
    : []),
]

/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    unoptimized: true,
  },
  async headers() {
    return [
      {
        source: "/(.*)",
        headers: securityHeaders,
      },
    ]
  },
}

export default nextConfig

function readHttpOrigin(value, variableName) {
  let parsed
  try {
    parsed = new URL(value)
  } catch {
    throw new Error(`${variableName} must be an absolute HTTP or HTTPS URL.`)
  }

  if (
    !["http:", "https:"].includes(parsed.protocol) ||
    parsed.username ||
    parsed.password
  ) {
    throw new Error(
      `${variableName} must be an absolute HTTP or HTTPS URL without credentials.`,
    )
  }

  return parsed.origin
}

function readAuth0Origin(domain) {
  if (!domain) {
    return ""
  }

  if (
    domain.includes("/") ||
    domain.includes("\\") ||
    domain.includes("@") ||
    domain.includes("?") ||
    domain.includes("#")
  ) {
    throw new Error(
      "NEXT_PUBLIC_AUTH0_DOMAIN must be a host name without scheme, path, credentials, query, or fragment.",
    )
  }

  return readHttpOrigin(`https://${domain}`, "NEXT_PUBLIC_AUTH0_DOMAIN")
}

function toWebSocketOrigin(origin) {
  const parsed = new URL(origin)
  parsed.protocol = parsed.protocol === "https:" ? "wss:" : "ws:"
  return parsed.origin
}
