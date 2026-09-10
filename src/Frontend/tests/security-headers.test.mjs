import assert from "node:assert/strict"
import { readFile } from "node:fs/promises"
import test from "node:test"

test("production headers restrict browser capabilities and external origins", async () => {
  process.env.NODE_ENV = "production"
  process.env.NEXT_PUBLIC_API_BASE_URL = "https://api.example.test/gateway"

  const config = await import("../next.config.mjs?security-headers=production")
  const headers = await readHeaders(config.default)
  assert.equal(headers.has("Content-Security-Policy"), false)
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
  assert.equal(headers.has("Strict-Transport-Security"), false)
})

test("proxy creates a per-request nonce policy without unsafe inline scripts", async () => {
  const source = await readFile(new URL("../proxy.ts", import.meta.url), "utf8")

  assert.match(source, /crypto\.randomUUID\(\)/)
  assert.match(source, /'nonce-\$\{nonce\}'/)
  assert.match(source, /'strict-dynamic'/)
  assert.doesNotMatch(source, /'unsafe-inline'.*script/)
  assert.match(source, /response\.headers\.set\("Content-Security-Policy"/)
  assert.match(source, /requestHeaders\.set\("x-nonce"/)
})

async function readHeaders(config) {
  const rules = await config.headers()
  assert.equal(rules.length, 1)
  assert.equal(rules[0].source, "/(.*)")
  return new Map(rules[0].headers.map(({ key, value }) => [key, value]))
}
