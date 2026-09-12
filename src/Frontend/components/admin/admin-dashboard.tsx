"use client"

import { useState } from "react"
import { Loader2, LogOut, Store } from "lucide-react"

import { AdminCatalogWorkspace } from "@/components/admin/admin-catalog-workspace"
import { AdminCatalogReferenceWorkspace } from "@/components/admin/admin-catalog-reference-workspace"
import { AdminOverviewMetrics } from "@/components/admin/admin-overview-metrics"
import { AdminShell } from "@/components/admin/admin-shell"
import { AdminUserWorkspace } from "@/components/admin/admin-user-workspace"
import { Button } from "@/components/ui/button"
import { logoutFromAuth0 } from "@/lib/auth/auth0"
import { useI18n } from "@/lib/i18n/provider"
import type { UserProfile } from "@/types"

interface AdminDashboardProps {
  profile: UserProfile
  onOpenSellerWorkspace?: () => void
}

export function AdminDashboard({ profile, onOpenSellerWorkspace }: AdminDashboardProps) {
  const { t } = useI18n()
  const [signingOut, setSigningOut] = useState(false)

  async function handleSignOut() {
    setSigningOut(true)
    await logoutFromAuth0("/login")
  }

  return (
    <AdminShell
      section="overview"
      title={t.admin.welcome.replace("{name}", profile.displayName || profile.email)}
      description={t.admin.description}
      headerActions={
        <>
          {onOpenSellerWorkspace ? (
            <Button type="button" variant="outline" size="sm" onClick={onOpenSellerWorkspace}>
              <Store />
              <span className="hidden sm:inline">{t.admin.openSellerWorkspace}</span>
            </Button>
          ) : null}
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={handleSignOut}
            disabled={signingOut}
          >
            {signingOut ? <Loader2 className="animate-spin" /> : <LogOut />}
            <span className="hidden sm:inline">{t.home.signOut}</span>
          </Button>
        </>
      }
    >
      <AdminOverviewMetrics />
      <AdminCatalogReferenceWorkspace />
      <AdminCatalogWorkspace />
      <AdminUserWorkspace />
    </AdminShell>
  )
}
