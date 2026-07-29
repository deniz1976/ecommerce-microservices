import { apiRequest } from "@/lib/api/client"
import type { AdminUser, PagedResult, Role, UserStatus } from "@/types"

export interface AdminUserQuery {
  pageNumber?: number
  pageSize?: number
  search?: string
  role?: Role
  status?: UserStatus
}

export function getAdminUsers(query: AdminUserQuery): Promise<PagedResult<AdminUser>> {
  const parameters = new URLSearchParams({
    pageNumber: String(query.pageNumber ?? 1),
    pageSize: String(query.pageSize ?? 20),
  })

  if (query.search?.trim()) parameters.set("search", query.search.trim())
  if (query.role) parameters.set("role", query.role)
  if (query.status !== undefined) parameters.set("status", String(query.status))

  return apiRequest<PagedResult<AdminUser>>(`/gateway/users?${parameters.toString()}`, {
    authenticated: true,
  })
}
