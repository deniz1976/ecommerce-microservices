"use client"

import { UserRoundIcon } from "lucide-react"
import Link from "next/link"
import { useParams } from "next/navigation"
import { useEffect, useState } from "react"

import { AdminPageLayout } from "@/components/admin/admin-page-layout"
import { EmptyState, ErrorState } from "@/components/patterns/states"
import { StatusBadge } from "@/components/patterns/status-badge"
import { buttonVariants } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { getAdminUser } from "@/lib/api/admin-users"
import { ApiError } from "@/lib/api/client"
import { formatDateTime } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import { userStatusLabel, userStatusTone } from "@/lib/i18n/status"
import { cn } from "@/lib/utils"
import type { AdminUser } from "@/types"

type UserDetailState =
  | { status: "loading" }
  | { status: "ready"; data: AdminUser }
  | { status: "not-found" }
  | { status: "unavailable" }

export function AdminUserDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { locale, t } = useI18n()
  const [user, setUser] = useState<UserDetailState>({ status: "loading" })
  const [reloadToken, setReloadToken] = useState(0)

  useEffect(() => {
    const controller = new AbortController()
    getAdminUser(id, controller.signal)
      .then((data) => setUser({ status: "ready", data }))
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === "AbortError") return
        setUser({
          status: error instanceof ApiError && error.status === 404 ? "not-found" : "unavailable",
        })
      })
    return () => controller.abort()
  }, [id, reloadToken])

  return (
    <AdminPageLayout title={t.admin.userDetails} description={t.admin.userDetailsDescription}>
      <div className="mt-6">
        {user.status === "loading" ? (
          <div className="flex flex-col gap-3">
            <Skeleton className="h-20 rounded-xl" />
            <Skeleton className="h-64 rounded-xl" />
          </div>
        ) : user.status === "not-found" ? (
          <EmptyState title={t.admin.userNotFound} description="" />
        ) : user.status === "unavailable" ? (
          <ErrorState
            title={t.admin.userDetailsUnavailable}
            onRetry={() => {
              setUser({ status: "loading" })
              setReloadToken((token) => token + 1)
            }}
          />
        ) : (
          <article className="overflow-hidden rounded-xl border border-border bg-card">
            <div className="flex items-center gap-3 border-b border-border px-5 py-4">
              <span className="flex size-10 shrink-0 items-center justify-center rounded-full bg-muted">
                <UserRoundIcon className="size-5 text-muted-foreground" aria-hidden="true" />
              </span>
              <div className="flex min-w-0 flex-col gap-0.5">
                <h2 className="truncate font-heading text-xl font-semibold">
                  {user.data.displayName || user.data.email}
                </h2>
                <p className="truncate text-sm text-muted-foreground">{user.data.email}</p>
              </div>
              <StatusBadge
                className="ml-auto"
                label={userStatusLabel(user.data.status, t.status)}
                tone={userStatusTone(user.data.status)}
              />
            </div>

            <dl className="grid gap-px bg-border sm:grid-cols-2">
              <DetailField label={t.admin.userId} value={user.data.id} mono />
              <DetailField
                label={t.admin.userRoles}
                value={
                  user.data.roles.length === 0
                    ? t.admin.onboardingPending
                    : user.data.roles.map((role) => t.admin.userRole[role]).join(", ")
                }
              />
              <DetailField
                label={t.admin.onboardingStatus}
                value={
                  user.data.isOnboardingComplete
                    ? t.admin.onboardingComplete
                    : t.admin.onboardingPending
                }
              />
              <DetailField
                label={t.admin.joinedAt}
                value={formatDateTime(user.data.createdAt, locale)}
              />
              <DetailField
                label={t.admin.paymentUpdatedAt}
                value={formatDateTime(user.data.updatedAt, locale)}
              />
            </dl>

            <div className="border-t border-border px-5 py-4">
              <Link
                href="/#admin-users"
                className={cn(buttonVariants({ variant: "outline", size: "sm" }))}
              >
                {t.admin.backToUsers}
              </Link>
            </div>
          </article>
        )}
      </div>
    </AdminPageLayout>
  )
}

function DetailField({
  label,
  value,
  mono = false,
}: {
  label: string
  value: string
  mono?: boolean
}) {
  return (
    <div className="flex flex-col gap-1 bg-card px-5 py-4">
      <dt className="text-xs font-medium text-muted-foreground">{label}</dt>
      <dd className={cn("text-sm break-words", mono && "font-mono text-xs")}>{value}</dd>
    </div>
  )
}
