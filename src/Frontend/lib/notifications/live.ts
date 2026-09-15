import {
  HubConnectionBuilder,
  LogLevel,
  type HubConnection,
} from "@microsoft/signalr"

import { API_BASE_URL, AuthenticationRequiredError } from "@/lib/api/client"
import { getAccessToken } from "@/lib/auth/auth0"
import type { CustomerNotification } from "@/types"

const notificationReceivedMethod = "notificationReceived"
const joinCustomerGroupMethod = "JoinCustomerGroup"

export function createCustomerNotificationConnection(
  customerId: string,
  onNotification: (notification: CustomerNotification) => void,
): HubConnection {
  const normalizedCustomerId = customerId.toLowerCase()
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
    .withAutomaticReconnect([0, 2_000, 10_000, 30_000])
    .configureLogging(LogLevel.None)
    .build()

  connection.on(
    notificationReceivedMethod,
    (notification: CustomerNotification) => {
      if (notification.customerId.toLowerCase() === normalizedCustomerId) {
        onNotification(notification)
      }
    },
  )
  connection.onreconnected(() => joinCustomerGroup(connection, customerId))

  return connection
}

export async function startCustomerNotificationConnection(
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
