import {
  HubConnectionBuilder,
  LogLevel,
  type HubConnection,
  type IRetryPolicy,
  type RetryContext,
} from "@microsoft/signalr"

import { API_BASE_URL, AuthenticationRequiredError } from "@/lib/api/client"
import { getAccessToken } from "@/lib/auth/auth0"
import type { CustomerNotification } from "@/types"

type NotificationListener = (notification: CustomerNotification) => void

interface SharedCustomerConnection {
  connection: HubConnection
  listeners: Set<NotificationListener>
}

const notificationReceivedMethod = "notificationReceived"
const joinCustomerGroupMethod = "JoinCustomerGroup"
const reconnectDelaysMilliseconds = [0, 2_000, 10_000, 30_000]
const maximumReconnectDelayMilliseconds = 60_000

const sharedConnections = new Map<string, SharedCustomerConnection>()

const persistentRetryPolicy: IRetryPolicy = {
  nextRetryDelayInMilliseconds(context: RetryContext): number {
    return (
      reconnectDelaysMilliseconds[context.previousRetryCount] ??
      maximumReconnectDelayMilliseconds
    )
  },
}

export function subscribeToCustomerNotifications(
  customerId: string,
  listener: NotificationListener,
): () => void {
  const key = customerId.toLowerCase()
  let shared = sharedConnections.get(key)

  if (!shared) {
    shared = {
      connection: createCustomerNotificationConnection(key),
      listeners: new Set(),
    }
    sharedConnections.set(key, shared)
    startCustomerNotificationConnection(shared.connection, customerId).catch(
      () => undefined,
    )
  }

  shared.listeners.add(listener)
  const subscribed = shared

  return () => {
    subscribed.listeners.delete(listener)
    if (subscribed.listeners.size > 0) {
      return
    }

    if (sharedConnections.get(key) === subscribed) {
      sharedConnections.delete(key)
    }

    void subscribed.connection.stop().catch(() => undefined)
  }
}

function createCustomerNotificationConnection(
  normalizedCustomerId: string,
): HubConnection {
  const connection = new HubConnectionBuilder()
    .withUrl(
      `${API_BASE_URL.replace(/\/$/, "")}/gateway/hubs/notifications`,
      {
        withCredentials: false,
        accessTokenFactory: async () => {
          const token = await getAccessToken()
          if (!token) {
            throw new AuthenticationRequiredError()
          }

          return token
        },
      },
    )
    .withAutomaticReconnect(persistentRetryPolicy)
    .configureLogging(LogLevel.None)
    .build()

  connection.on(
    notificationReceivedMethod,
    (notification: CustomerNotification) => {
      if (notification.customerId.toLowerCase() !== normalizedCustomerId) {
        return
      }

      const shared = sharedConnections.get(normalizedCustomerId)
      shared?.listeners.forEach((listener) => listener(notification))
    },
  )
  connection.onreconnected(() =>
    joinCustomerGroup(connection, normalizedCustomerId),
  )

  return connection
}

async function startCustomerNotificationConnection(
  connection: HubConnection,
  customerId: string,
): Promise<void> {
  await connection.start()
  await joinCustomerGroup(connection, customerId)
}

function joinCustomerGroup(
  connection: HubConnection,
  customerId: string,
): Promise<void> {
  return connection.invoke(joinCustomerGroupMethod, customerId)
}
