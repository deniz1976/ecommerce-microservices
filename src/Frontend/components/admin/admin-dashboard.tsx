"use client"

import { Store } from "lucide-react"

import { AdminCatalogWorkspace } from "@/components/admin/admin-catalog-workspace"
import { AdminCatalogReferenceWorkspace } from "@/components/admin/admin-catalog-reference-workspace"
import { AdminOverviewMetrics } from "@/components/admin/admin-overview-metrics"
import { AdminShell } from "@/components/admin/admin-shell"
import { AdminUserWorkspace } from "@/components/admin/admin-user-workspace"
import { Button } from "@/components/ui/button"
import { useI18n } from "@/lib/i18n/provider"
import type { UserProfile } from "@/types"

interface AdminDashboardProps {
  profile: UserProfile
  onOpenSellerWorkspace?: () => void
}

export function AdminDashboard({ profile, onOpenSellerWorkspace }: AdminDashboardProps) {
  const { t } = useI18n()
  return (
    <AdminShell
      section="overview"
      title={t.admin.welcome.replace("{name}", profile.displayName || profile.email)}
      description={t.admin.description}
      headerActions={onOpenSellerWorkspace ? (
        <Button type="button" variant="outline" size="sm" onClick={onOpenSellerWorkspace}>
          <Store />
          <span className="hidden sm:inline">{t.admin.openSellerWorkspace}</span>
        </Button>
      ) : null}
    >
      <AdminOverviewMetrics />
      <AdminCatalogReferenceWorkspace />
      <AdminCatalogWorkspace />
      <AdminUserWorkspace />
    </AdminShell>
  )
}
