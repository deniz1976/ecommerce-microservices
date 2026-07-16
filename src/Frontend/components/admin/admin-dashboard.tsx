"use client"

import { useEffect, useMemo, useState } from "react"
import {
  Boxes,
  CheckCircle2,
  ClipboardList,
  LayoutDashboard,
  Loader2,
  LogOut,
  Package,
  ShieldCheck,
  Store,
  Users,
} from "lucide-react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { Button } from "@/components/ui/button"
import { getCatalogProducts } from "@/lib/api/catalog"
import { logoutFromAuth0 } from "@/lib/auth/auth0"
import { useI18n } from "@/lib/i18n/provider"
import { productStatusNames, type CatalogProduct, type PagedResult, type UserProfile } from "@/types"

interface AdminDashboardProps {
  profile: UserProfile
}

type CatalogState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<CatalogProduct> }
  | { status: "unavailable" }

export function AdminDashboard({ profile }: AdminDashboardProps) {
  const { t } = useI18n()
  const [catalog, setCatalog] = useState<CatalogState>({ status: "loading" })
  const [signingOut, setSigningOut] = useState(false)

  useEffect(() => {
    let active = true

    getCatalogProducts()
      .then((data) => {
        if (active) {
          setCatalog({ status: "ready", data })
        }
      })
      .catch(() => {
        if (active) {
          setCatalog({ status: "unavailable" })
        }
      })

    return () => {
      active = false
    }
  }, [])

  const summary = useMemo(() => {
    if (catalog.status !== "ready") {
      return { total: "-", active: "-", drafts: "-" }
    }

    return {
      total: String(catalog.data.totalCount),
      active: String(catalog.data.items.filter((product) => product.status === 1).length),
      drafts: String(catalog.data.items.filter((product) => product.status === 0).length),
    }
  }, [catalog])

  async function handleSignOut() {
    setSigningOut(true)
    await logoutFromAuth0("/login")
  }

  return (
    <div className="min-h-svh bg-muted/35">
      <header className="sticky top-0 z-10 flex h-16 items-center justify-between border-b border-border bg-background px-4 sm:px-6">
        <Logo />
        <div className="flex items-center gap-2">
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
            <span className="flex h-9 items-center gap-3 px-3 text-sm text-muted-foreground">
              <Package className="size-4" />
              {t.admin.catalog}
            </span>
            <span className="flex h-9 items-center gap-3 px-3 text-sm text-muted-foreground">
              <Users className="size-4" />
              {t.admin.users}
            </span>
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

          <section className="grid gap-px overflow-hidden rounded-lg border border-border bg-border sm:grid-cols-3" aria-label={t.admin.catalogSummary}>
            <Metric icon={Boxes} label={t.admin.totalProducts} value={summary.total} />
            <Metric icon={CheckCircle2} label={t.admin.activeProducts} value={summary.active} />
            <Metric icon={ClipboardList} label={t.admin.draftProducts} value={summary.drafts} />
          </section>

          <section className="mt-8" aria-labelledby="recent-products-title">
            <div className="flex items-center justify-between gap-3">
              <div>
                <h2 id="recent-products-title" className="font-heading text-lg font-semibold text-foreground">
                  {t.admin.recentProducts}
                </h2>
                <p className="mt-1 text-sm text-muted-foreground">{t.admin.recentProductsDescription}</p>
              </div>
            </div>

            <div className="mt-4 overflow-hidden rounded-lg border border-border bg-background">
              {catalog.status === "loading" ? (
                <div className="flex min-h-48 items-center justify-center">
                  <Loader2 className="size-5 animate-spin text-muted-foreground" />
                  <span className="sr-only">{t.common.loading}</span>
                </div>
              ) : catalog.status === "unavailable" ? (
                <div className="flex min-h-48 items-center px-5 text-sm text-muted-foreground">
                  {t.admin.catalogUnavailable}
                </div>
              ) : catalog.data.items.length === 0 ? (
                <div className="flex min-h-48 items-center px-5 text-sm text-muted-foreground">
                  {t.admin.noProducts}
                </div>
              ) : (
                <div className="divide-y divide-border">
                  {catalog.data.items.map((product) => (
                    <article key={product.id} className="grid gap-2 px-5 py-4 sm:grid-cols-[minmax(0,1fr)_auto_auto] sm:items-center sm:gap-6">
                      <div className="min-w-0">
                        <p className="truncate text-sm font-medium text-foreground">{product.name}</p>
                        <p className="mt-1 truncate text-xs text-muted-foreground">
                          {product.sku}{product.categoryName ? ` - ${product.categoryName}` : ""}
                        </p>
                      </div>
                      <p className="text-sm font-medium text-foreground">{formatPrice(product.price, product.currency)}</p>
                      <StatusBadge status={product.status} label={t.admin.status[productStatusNames[product.status]]} />
                    </article>
                  ))}
                </div>
              )}
            </div>
          </section>
        </main>
      </div>
    </div>
  )
}

function Metric({ icon: Icon, label, value }: { icon: typeof Boxes; label: string; value: string }) {
  return (
    <div className="bg-background p-5">
      <Icon className="size-4 text-primary" />
      <p className="mt-5 text-2xl font-semibold text-foreground">{value}</p>
      <p className="mt-1 text-sm text-muted-foreground">{label}</p>
    </div>
  )
}

function StatusBadge({ status, label }: { status: CatalogProduct["status"]; label: string }) {
  const classes = {
    0: "bg-highlight/15 text-highlight-foreground",
    1: "bg-primary/10 text-primary",
    2: "bg-muted text-muted-foreground",
    3: "bg-muted text-muted-foreground",
  } satisfies Record<CatalogProduct["status"], string>

  return <span className={`w-fit rounded-md px-2 py-1 text-xs font-medium ${classes[status]}`}>{label}</span>
}

function formatPrice(price: number, currency: string) {
  return new Intl.NumberFormat(undefined, {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(price)
}
