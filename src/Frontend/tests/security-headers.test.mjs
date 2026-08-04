import assert from "node:assert/strict"
import test from "node:test"

test("production headers restrict browser capabilities and external origins", async () => {
  process.env.NODE_ENV = "production"
  process.env.NEXT_PUBLIC_API_BASE_URL = "https://api.example.test/gateway"

  const config = await import("../next.config.mjs?security-headers=production")
  const headers = await readHeaders(config.default)
  const csp = headers.get("Content-Security-Policy")

  assert.match(csp, /default-src 'self'/)
  assert.match(csp, /connect-src 'self' https:\/\/api\.example\.test wss:\/\/api\.example\.test/)
  assert.doesNotMatch(csp, /auth0\.test/)
  assert.match(csp, /frame-src 'self'/)
  assert.match(csp, /worker-src 'self' blob:/)
  assert.match(csp, /object-src 'none'/)
  assert.match(csp, /frame-ancestors 'none'/)
  assert.match(csp, /upgrade-insecure-requests/)
  assert.doesNotMatch(csp, /'unsafe-eval'/)
  assert.equal(headers.get("X-Content-Type-Options"), "nosniff")
  assert.equal(headers.get("X-Frame-Options"), "DENY")
  assert.equal(headers.get("Referrer-Policy"), "strict-origin-when-cross-origin")
  assert.equal(
    headers.get("Permissions-Policy"),
    "camera=(), microphone=(), geolocation=(), payment=(), usb=()",
  )
  assert.equal(
    headers.get("Strict-Transport-Security"),
    "max-age=63072000; includeSubDomains",
  )
})

test("development headers preserve local Next.js debugging without HSTS", async () => {
  process.env.NODE_ENV = "development"
  process.env.NEXT_PUBLIC_API_BASE_URL = "http://localhost:5080"

  const config = await import("../next.config.mjs?security-headers=development")
  const headers = await readHeaders(config.default)
  const csp = headers.get("Content-Security-Policy")

  assert.match(csp, /connect-src 'self' http:\/\/localhost:5080 ws:\/\/localhost:5080/)
  assert.match(csp, /'unsafe-eval'/)
  assert.doesNotMatch(csp, /upgrade-insecure-requests/)
  assert.equal(headers.has("Strict-Transport-Security"), false)
})

test("invalid public origins fail closed during configuration", async () => {
  process.env.NODE_ENV = "production"
  process.env.NEXT_PUBLIC_API_BASE_URL = "https://user:secret@api.example.test"

  await assert.rejects(
    import("../next.config.mjs?security-headers=invalid-api"),
    /without credentials/,
  )
})

async function readHeaders(config) {
  const rules = await config.headers()
  assert.equal(rules.length, 1)
  assert.equal(rules[0].source, "/(.*)")
  return new Map(rules[0].headers.map(({ key, value }) => [key, value]))
}
