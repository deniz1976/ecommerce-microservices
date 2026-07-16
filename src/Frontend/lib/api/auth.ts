import { apiRequest } from "@/lib/api/client"
import type { UpdateRolePayload, UserProfile } from "@/types"

export function getProfile(): Promise<UserProfile> {
  return apiRequest<UserProfile>("/gateway/auth/me", {
    authenticated: true,
  })
}

export function updateRole(payload: UpdateRolePayload): Promise<UserProfile> {
  return apiRequest<UserProfile>("/gateway/auth/me/role", {
    method: "PUT",
    authenticated: true,
    body: payload,
  })
}
