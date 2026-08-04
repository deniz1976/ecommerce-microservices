"use client"

import { useState } from "react"
import Link from "next/link"
import {
  ClipboardList,
  LayoutDashboard,
  Loader2,
  LogOut,
  Package,
  PackageSearch,
  ReceiptText,
  ShieldCheck,
  Store,
  Tag,
  Tags,
  Truck,
  Users,
} from "lucide-react"

import { AdminCatalogWorkspace } from "@/components/admin/admin-catalog-workspace"
import { AdminCatalogReferenceWorkspace } from "@/components/admin/admin-catalog-reference-workspace"
import { AdminOverviewMetrics } from "@/components/admin/admin-overview-metrics"
import { AdminUserWorkspace } from "@/components/admin/admin-user-workspace"
import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
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
    <div className="min-h-svh bg-muted/35">
      <header className="sticky top-0 z-10 flex h-16 items-center justify-between border-b border-border bg-background px-4 sm:px-6">
        <Logo />
        <div className="flex items-center gap-2">
          {onOpenSellerWorkspace ? (
            <Button type="button" variant="outline" size="sm" onClick={onOpenSellerWorkspace}>
              <Store />
              <span className="hidden sm:inline">{t.admin.openSellerWorkspace}</span>
            </Button>
          ) : null}
          <LanguageSwitcher />
          <ThemeToggle />
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={handleSignOut}
            disabled={signingOut}
          >
            {signingOut ? <Loader2 className="animate-spin" /> : <LogOut />}
            <span className="hidden sm:inline">{t.home.signOut}</span>
            <span className="sr-only sm:hidden">{t.home.signOut}</span>
          </Button>
        </div>
      </header>

      <div className="mx-auto grid w-full max-w-7xl lg:grid-cols-[13.5rem_minmax(0,1fr)]">
        <aside className="hidden border-r border-border bg-background px-3 py-5 lg:block">
          <p className="px-3 text-xs font-semibold tracking-normal text-muted-foreground uppercase">
            {t.admin.workspace}
          </p>
          <nav className="mt-3 space-y-1" aria-label={t.admin.navigation}>
            <span className="flex h-9 items-center gap-3 rounded-md bg-accent px-3 text-sm font-medium text-accent-foreground">
              <LayoutDashboard className="size-4" />
              {t.admin.overview}
            </span>
            <a href="#admin-catalog" className="flex h-9 items-center gap-3 px-3 text-sm text-muted-foreground transition hover:text-foreground">
              <Package className="size-4" />
              {t.admin.catalog}
            </a>
            <Link href="/admin/categories" className="flex h-9 items-center gap-3 px-3 text-sm text-muted-foreground transition hover:text-foreground">
              <Tags className="size-4" />
              {t.admin.categories}
            </Link>
            <Link href="/admin/brands" className="flex h-9 items-center gap-3 px-3 text-sm text-muted-foreground transition hover:text-foreground">
              <Tag className="size-4" />
              {t.admin.brands}
            </Link>
            <Link href="/admin/stores" className="flex h-9 items-center gap-3 px-3 text-sm text-muted-foreground transition hover:text-foreground">
              <Store className="size-4" />
              {t.admin.stores}
            </Link>
            <a href="#admin-users" className="flex h-9 items-center gap-3 px-3 text-sm text-muted-foreground transition hover:text-foreground">
              <Users className="size-4" />
              {t.admin.users}
            </a>
            <Link href="/admin/orders" className="flex h-9 items-center gap-3 px-3 text-sm text-muted-foreground transition hover:text-foreground">
              <ClipboardList className="size-4" />
              {t.admin.orders}
            </Link>
            <Link href="/admin/inventory" className="flex h-9 items-center gap-3 px-3 text-sm text-muted-foreground transition hover:text-foreground">
              <PackageSearch className="size-4" />
              {t.admin.inventory}
            </Link>
            <Link href="/admin/payments" className="flex h-9 items-center gap-3 px-3 text-sm text-muted-foreground transition hover:text-foreground">
              <ReceiptText className="size-4" />
              {t.admin.payments}
            </Link>
            <Link href="/admin/shipments" className="flex h-9 items-center gap-3 px-3 text-sm text-muted-foreground transition hover:text-foreground">
              <Truck className="size-4" />
              {t.admin.shipments}
            </Link>
          </nav>
        </aside>

        <main className="min-w-0 px-4 py-7 sm:px-6 lg:px-8">
          <div className="flex flex-col justify-between gap-4 border-b border-border pb-6 sm:flex-row sm:items-end">
            <div>
              <div className="flex items-center gap-2 text-sm text-primary">
                <ShieldCheck className="size-4" />
                <span className="font-medium">{t.admin.roleLabel}</span>
              </div>
              <h1 className="mt-2 font-heading text-2xl font-semibold text-foreground sm:text-3xl">
                {t.admin.welcome.replace("{name}", profile.displayName || profile.email)}
              </h1>
              <p className="mt-2 max-w-2xl text-sm leading-6 text-muted-foreground">
                {t.admin.description}
              </p>
            </div>
            <div className="flex items-center gap-3 text-sm">
              <span className="flex size-9 items-center justify-center rounded-full bg-primary/10 text-primary">
                <Store className="size-4" />
              </span>
              <div className="min-w-0">
                <p className="truncate font-medium text-foreground">{profile.displayName || profile.email}</p>
                <p className="truncate text-muted-foreground">{profile.email}</p>
              </div>
            </div>
          </div>

          <AdminOverviewMetrics />
          <AdminCatalogReferenceWorkspace />
          <AdminCatalogWorkspace />
          <AdminUserWorkspace />
        </main>
      </div>
    </div>
  )
}
