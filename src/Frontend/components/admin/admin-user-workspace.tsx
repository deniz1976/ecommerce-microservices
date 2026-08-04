"use client"

import { type FormEvent, useEffect, useState } from "react"
import { ChevronLeft, ChevronRight, Loader2, Search, UserRoundCheck, Users } from "lucide-react"
import Link from "next/link"

import { Button } from "@/components/ui/button"
import { getAdminUsers } from "@/lib/api/admin-users"
import { useI18n } from "@/lib/i18n/provider"
import type { AdminUser, PagedResult, Role, UserStatus } from "@/types"

type UserState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<AdminUser> }
  | { status: "unavailable" }

type RoleFilter = "all" | Role
type StatusFilter = "all" | `${UserStatus}`

const pageSize = 10

export function AdminUserWorkspace() {
  const { locale, t } = useI18n()
  const [users, setUsers] = useState<UserState>({ status: "loading" })
  const [searchInput, setSearchInput] = useState("")
  const [search, setSearch] = useState("")
  const [role, setRole] = useState<RoleFilter>("all")
  const [status, setStatus] = useState<StatusFilter>("all")
  const [pageNumber, setPageNumber] = useState(1)

  useEffect(() => {
    const controller = new AbortController()

    getAdminUsers({
      pageNumber,
      pageSize,
      search,
      role: role === "all" ? undefined : role,
      status: status === "all" ? undefined : Number(status) as UserStatus,
    }, controller.signal)
      .then((data) => {
        setUsers({ status: "ready", data })
      })
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) {
          setUsers({ status: "unavailable" })
        }
      })

    return () => controller.abort()
  }, [pageNumber, role, search, status])

  function handleSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setPageNumber(1)
    setSearch(searchInput.trim())
  }

  return (
    <section id="admin-users" className="mt-10" aria-labelledby="admin-users-title">
      <div>
        <h2 id="admin-users-title" className="font-heading text-xl font-semibold text-foreground">
          {t.admin.manageUsers}
        </h2>
        <p className="mt-1 text-sm text-muted-foreground">{t.admin.manageUsersDescription}</p>
      </div>

      <form onSubmit={handleSearch} className="mt-5 grid gap-3 rounded-lg border border-border bg-background p-4 lg:grid-cols-[minmax(0,1fr)_10rem_10rem_auto]">
        <label className="grid gap-2 text-sm font-medium text-foreground">
          {t.admin.searchUsers}
          <input
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
            placeholder={t.admin.searchUsersPlaceholder}
            className="h-10 rounded-md border border-input bg-background px-3 font-normal"
          />
        </label>
        <label className="grid gap-2 text-sm font-medium text-foreground">
          {t.admin.filterRole}
          <select
            value={role}
            onChange={(event) => {
              setRole(event.target.value as RoleFilter)
              setPageNumber(1)
            }}
            className="h-10 rounded-md border border-input bg-background px-3 font-normal"
          >
            <option value="all">{t.admin.allRoles}</option>
            <option value="Customer">{t.admin.userRole.Customer}</option>
            <option value="Seller">{t.admin.userRole.Seller}</option>
            <option value="Admin">{t.admin.userRole.Admin}</option>
          </select>
        </label>
        <label className="grid gap-2 text-sm font-medium text-foreground">
          {t.admin.filterUserStatus}
          <select
            value={status}
            onChange={(event) => {
              setStatus(event.target.value as StatusFilter)
              setPageNumber(1)
            }}
            className="h-10 rounded-md border border-input bg-background px-3 font-normal"
          >
            <option value="all">{t.admin.allUserStatuses}</option>
            <option value="1">{t.admin.userActive}</option>
            <option value="2">{t.admin.userDisabled}</option>
          </select>
        </label>
        <Button type="submit" className="self-end">
          <Search />
          {t.admin.searchAction}
        </Button>
      </form>

      <div className="mt-5 overflow-hidden rounded-lg border border-border bg-background">
        {users.status === "loading" ? (
          <div className="flex min-h-44 items-center justify-center">
            <Loader2 className="size-5 animate-spin text-muted-foreground" />
          </div>
        ) : users.status === "unavailable" ? (
          <UserMessage message={t.admin.usersUnavailable} />
        ) : users.data.items.length === 0 ? (
          <UserMessage message={t.admin.noMatchingUsers} />
        ) : (
          <>
            <div className="divide-y divide-border">
              {users.data.items.map((user) => (
                <article key={user.id} className="grid gap-3 px-5 py-4 md:grid-cols-[minmax(0,1fr)_auto_auto] md:items-center">
                  <div className="min-w-0">
                    <p className="truncate text-sm font-medium text-foreground">{user.displayName || user.email}</p>
                    <p className="mt-1 truncate text-xs text-muted-foreground">{user.email}</p>
                    <p className="mt-1 text-xs text-muted-foreground">
                      {t.admin.joinedAt}: {new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(new Date(user.createdAt))}
                    </p>
                    <Link href={`/admin/users/${user.id}`} className="mt-2 inline-flex text-xs font-medium text-primary hover:underline">
                      {t.admin.viewUserDetails}
                    </Link>
                  </div>
                  <div className="flex flex-wrap gap-1.5">
                    {user.roles.length === 0 ? (
                      <span className="rounded-md bg-muted px-2 py-1 text-xs text-muted-foreground">
                        {t.admin.onboardingPending}
                      </span>
                    ) : user.roles.map((userRole) => (
                      <span key={userRole} className="rounded-md bg-primary/10 px-2 py-1 text-xs font-medium text-primary">
                        {t.admin.userRole[userRole]}
                      </span>
                    ))}
                  </div>
                  <span className={`w-fit rounded-md px-2 py-1 text-xs font-medium ${
                    user.status === 1 ? "bg-primary/10 text-primary" : "bg-muted text-muted-foreground"
                  }`}>
                    {user.status === 1 ? t.admin.userActive : t.admin.userDisabled}
                  </span>
                </article>
              ))}
            </div>
            <div className="flex items-center justify-between gap-3 border-t border-border px-4 py-3">
              <p className="text-xs text-muted-foreground">
                {t.admin.userPageStatus
                  .replace("{page}", String(users.data.pageNumber))
                  .replace("{total}", String(Math.max(users.data.totalPages, 1)))
                  .replace("{count}", String(users.data.totalCount))}
              </p>
              <div className="flex gap-2">
                <Button
                  type="button"
                  variant="outline"
                  size="icon-sm"
                  disabled={users.data.pageNumber <= 1}
                  onClick={() => setPageNumber((current) => Math.max(1, current - 1))}
                  aria-label={t.admin.previousPage}
                >
                  <ChevronLeft />
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="icon-sm"
                  disabled={users.data.pageNumber >= users.data.totalPages}
                  onClick={() => setPageNumber((current) => current + 1)}
                  aria-label={t.admin.nextPage}
                >
                  <ChevronRight />
                </Button>
              </div>
            </div>
          </>
        )}
      </div>
      <p className="mt-3 flex items-center gap-2 text-xs text-muted-foreground">
        <UserRoundCheck className="size-4" />
        {t.admin.usersReadOnlyNote}
      </p>
    </section>
  )
}

function UserMessage({ message }: { message: string }) {
  return (
    <div className="flex min-h-44 items-center gap-3 px-5 text-sm text-muted-foreground">
      <Users className="size-4 shrink-0" />
      {message}
    </div>
  )
}
