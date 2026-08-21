import { randomUUID } from "node:crypto"

import {
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr"

const accessToken = process.env.RuntimeChecks__AccessToken
if (!accessToken) {
  throw new Error("RuntimeChecks__AccessToken is required.")
}

const gatewayBaseUrl =
  process.env.RuntimeChecks__GatewayBaseUrl ?? "http://localhost:15080"
const hubUrl = `${gatewayBaseUrl.replace(/\/$/, "")}/gateway/hubs/notifications`

const anonymousConnection = new HubConnectionBuilder()
  .withUrl(hubUrl)
  .configureLogging(LogLevel.None)
  .build()

let anonymousRejected = false
try {
  await anonymousConnection.start()
} catch {
  anonymousRejected = true
} finally {
  await anonymousConnection.stop().catch(() => undefined)
}

if (!anonymousRejected) {
  throw new Error("Anonymous Notification SignalR connection was not rejected.")
}

const authenticatedConnection = new HubConnectionBuilder()
  .withUrl(hubUrl, {
    accessTokenFactory: () => accessToken,
  })
  .configureLogging(LogLevel.None)
  .build()

try {
  await authenticatedConnection.start()
  await authenticatedConnection.invoke("JoinCustomerGroup", randomUUID())
} finally {
  await authenticatedConnection.stop().catch(() => undefined)
}

console.log("Notification SignalR anonymous rejection and authenticated group join passed.")
