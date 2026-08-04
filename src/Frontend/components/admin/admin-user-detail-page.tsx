"use client"

import { Loader2, UserRound, Users } from "lucide-react"
import Link from "next/link"
import { useParams } from "next/navigation"
import { useEffect, useState } from "react"

import { AdminPageLayout } from "@/components/admin/admin-page-layout"
import { buttonVariants } from "@/components/ui/button"
import { getAdminUser } from "@/lib/api/admin-users"
import { ApiError } from "@/lib/api/client"
import { useI18n } from "@/lib/i18n/provider"
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

  useEffect(() => {
    const controller = new AbortController()
    getAdminUser(id, controller.signal)
      .then((data) => setUser({ status: "ready", data }))
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === "AbortError") return
        setUser({ status: error instanceof ApiError && error.status === 404 ? "not-found" : "unavailable" })
      })
    return () => controller.abort()
  }, [id])

  return (
    <AdminPageLayout title={t.admin.userDetails} description={t.admin.userDetailsDescription}>
      <div className="mt-6">
        {user.status === "loading" ? <DetailMessage loading message={t.common.loading} />
          : user.status === "not-found" ? <DetailMessage message={t.admin.userNotFound} />
            : user.status === "unavailable" ? <DetailMessage message={t.admin.userDetailsUnavailable} />
              : <article className="overflow-hidden rounded-lg border border-border bg-background">
                <div className="flex items-center gap-3 border-b border-border px-5 py-4">
                  <span className="flex size-10 items-center justify-center rounded-full bg-primary/10 text-primary"><UserRound /></span>
                  <div className="min-w-0">
                    <h2 className="truncate font-heading text-xl font-semibold text-foreground">{user.data.displayName || user.data.email}</h2>
                    <p className="truncate text-sm text-muted-foreground">{user.data.email}</p>
                  </div>
                </div>
                <dl className="grid gap-px bg-border sm:grid-cols-2">
                  <DetailField label={t.admin.userId} value={user.data.id} mono />
                  <DetailField label={t.admin.filterUserStatus} value={user.data.status === 1 ? t.admin.userActive : t.admin.userDisabled} />
                  <DetailField label={t.admin.userRoles} value={user.data.roles.length === 0 ? t.admin.onboardingPending : user.data.roles.map((role) => t.admin.userRole[role]).join(", ")} />
                  <DetailField label={t.admin.onboardingStatus} value={user.data.isOnboardingComplete ? t.admin.onboardingComplete : t.admin.onboardingPending} />
                  <DetailField label={t.admin.joinedAt} value={formatDate(user.data.createdAt, locale)} />
                  <DetailField label={t.admin.paymentUpdatedAt} value={formatDate(user.data.updatedAt, locale)} />
                </dl>
                <div className="border-t border-border px-5 py-4">
                  <Link href="/#admin-users" className={cn(buttonVariants({ variant: "outline", size: "sm" }))}>{t.admin.backToUsers}</Link>
                </div>
              </article>}
      </div>
    </AdminPageLayout>
  )
}

function DetailField({ label, value, mono = false }: { label: string; value: string; mono?: boolean }) {
  return <div className="bg-background px-5 py-4"><dt className="text-xs font-medium text-muted-foreground">{label}</dt><dd className={`mt-1 break-words text-sm text-foreground ${mono ? "font-mono text-xs" : ""}`}>{value}</dd></div>
}

function DetailMessage({ message, loading = false }: { message: string; loading?: boolean }) {
  return <div className="flex min-h-52 items-center justify-center gap-3 rounded-lg border border-border bg-background px-5 text-sm text-muted-foreground">{loading ? <Loader2 className="size-5 animate-spin" /> : <Users className="size-5" />}{message}</div>
}

function formatDate(value: string, locale: "en" | "tr") {
  return new Intl.DateTimeFormat(locale === "tr" ? "tr-TR" : "en-US", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value))
}
