"use client"

import { type FormEvent, useEffect, useState } from "react"
import { SearchIcon, UserRoundCheckIcon } from "lucide-react"
import Link from "next/link"

import { DataTable, type DataTableColumn } from "@/components/patterns/data-table"
import { FilterBar, FilterField } from "@/components/patterns/filter-bar"
import { StatusBadge } from "@/components/patterns/status-badge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { SelectNative } from "@/components/ui/select-native"
import { getAdminUsers } from "@/lib/api/admin-users"
import { formatDate } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import { userStatusLabel, userStatusTone } from "@/lib/i18n/status"
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
  const [reloadToken, setReloadToken] = useState(0)

  useEffect(() => {
    const controller = new AbortController()

    getAdminUsers(
      {
        pageNumber,
        pageSize,
        search,
        role: role === "all" ? undefined : role,
        status: status === "all" ? undefined : (Number(status) as UserStatus),
      },
      controller.signal,
    )
      .then((data) => {
        setUsers({ status: "ready", data })
      })
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) {
          setUsers({ status: "unavailable" })
        }
      })

    return () => controller.abort()
  }, [pageNumber, reloadToken, role, search, status])

  function handleSearch(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setUsers({ status: "loading" })
    setPageNumber(1)
    setSearch(searchInput.trim())
  }

  function clearFilters() {
    setUsers({ status: "loading" })
    setSearchInput("")
    setSearch("")
    setRole("all")
    setStatus("all")
    setPageNumber(1)
  }

  const columns: DataTableColumn<AdminUser>[] = [
    {
      id: "user",
      header: t.admin.searchUsers,
      cell: (user) => (
        <div className="flex min-w-0 flex-col gap-0.5">
          <Link
            href={`/admin/users/${user.id}`}
            className="truncate font-medium text-primary hover:underline"
          >
            {user.displayName || user.email}
          </Link>
          <span className="truncate text-xs text-muted-foreground">{user.email}</span>
        </div>
      ),
    },
    {
      id: "roles",
      header: t.admin.filterRole,
      cell: (row) =>
        row.roles.length === 0 ? (
          <span className="text-xs text-muted-foreground">{t.admin.onboardingPending}</span>
        ) : (
          <div className="flex flex-wrap gap-1.5">
            {row.roles.map((userRole) => (
              <span
                key={userRole}
                className="rounded-md bg-primary/10 px-2 py-1 text-xs font-medium text-primary"
              >
                {t.admin.userRole[userRole]}
              </span>
            ))}
          </div>
        ),
    },
    {
      id: "status",
      header: t.admin.filterUserStatus,
      cell: (row) => (
        <StatusBadge
          label={userStatusLabel(row.status, t.status)}
          tone={userStatusTone(row.status)}
        />
      ),
    },
    {
      id: "createdAt",
      header: t.admin.joinedAt,
      className: "text-muted-foreground tabular-nums",
      cell: (row) => formatDate(row.createdAt, locale),
    },
  ]

  return (
    <section id="admin-users" className="mt-10 flex flex-col gap-5" aria-labelledby="admin-users-title">
      <div>
        <h2 id="admin-users-title" className="font-heading text-xl font-semibold">
          {t.admin.manageUsers}
        </h2>
        <p className="mt-1 text-sm text-muted-foreground">{t.admin.manageUsersDescription}</p>
      </div>

      <form onSubmit={handleSearch}>
        <FilterBar
          onClear={clearFilters}
          hasActiveFilters={search !== "" || role !== "all" || status !== "all"}
        >
          <FilterField label={t.admin.searchUsers} className="min-w-64 flex-1">
            <Input
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
              placeholder={t.admin.searchUsersPlaceholder}
            />
          </FilterField>

          <FilterField label={t.admin.filterRole}>
            <SelectNative
              value={role}
              onChange={(event) => {
                setUsers({ status: "loading" })
                setRole(event.target.value as RoleFilter)
                setPageNumber(1)
              }}
            >
              <option value="all">{t.admin.allRoles}</option>
              <option value="Customer">{t.admin.userRole.Customer}</option>
              <option value="Seller">{t.admin.userRole.Seller}</option>
              <option value="Admin">{t.admin.userRole.Admin}</option>
            </SelectNative>
          </FilterField>

          <FilterField label={t.admin.filterUserStatus}>
            <SelectNative
              value={status}
              onChange={(event) => {
                setUsers({ status: "loading" })
                setStatus(event.target.value as StatusFilter)
                setPageNumber(1)
              }}
            >
              <option value="all">{t.admin.allUserStatuses}</option>
              <option value="1">{userStatusLabel(1, t.status)}</option>
              <option value="2">{userStatusLabel(2, t.status)}</option>
            </SelectNative>
          </FilterField>

          <Button type="submit" className="self-end">
            <SearchIcon />
            {t.admin.searchAction}
          </Button>
        </FilterBar>
      </form>

      <DataTable
        columns={columns}
        page={users.status === "ready" ? users.data : null}
        rowKey={(row) => row.id}
        isLoading={users.status === "loading"}
        error={users.status === "unavailable"}
        onRetry={() => {
          setUsers({ status: "loading" })
          setReloadToken((token) => token + 1)
        }}
        onPageChange={(value) => {
          setUsers({ status: "loading" })
          setPageNumber(value)
        }}
        emptyTitle={t.admin.noMatchingUsers}
        minWidthClassName="min-w-[48rem]"
      />

      <p className="flex items-center gap-2 text-xs text-muted-foreground">
        <UserRoundCheckIcon className="size-4" />
        {t.admin.usersReadOnlyNote}
      </p>
    </section>
  )
}
