"use client"

import Link from "next/link"
import { useEffect, useState } from "react"
import { ClipboardListIcon, Loader2Icon, LogOutIcon, PackageIcon, ShieldCheckIcon, StoreIcon } from "lucide-react"

import { MetricCard } from "@/components/patterns/metric-card"
import { EmptyState, ErrorState } from "@/components/patterns/states"
import { SellerShell } from "@/components/seller/seller-shell"
import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { getMyCatalogStores } from "@/lib/api/catalog"
import { logoutFromAuth0 } from "@/lib/auth/auth0"
import { useI18n } from "@/lib/i18n/provider"
import type { CatalogStore, UserProfile } from "@/types"

interface SellerDashboardProps {
  profile: UserProfile
  onOpenAdminWorkspace?: () => void
}

type StoreState =
  | { status: "loading" }
  | { status: "ready"; stores: CatalogStore[] }
  | { status: "unavailable" }

export function SellerDashboard({ profile, onOpenAdminWorkspace }: SellerDashboardProps) {
  const { t } = useI18n()
  const [stores, setStores] = useState<StoreState>({ status: "loading" })
  const [signingOut, setSigningOut] = useState(false)

  useEffect(() => {
    const controller = new AbortController()
    getMyCatalogStores(controller.signal)
      .then((data) => setStores({ status: "ready", stores: data }))
      .catch((error: unknown) => {
        if (!isAbortError(error)) setStores({ status: "unavailable" })
      })

    return () => controller.abort()
  }, [])

  async function handleSignOut() {
    setSigningOut(true)
    await logoutFromAuth0("/login")
  }

  const sections = [
    {
      href: "/seller/stores",
      label: t.seller.stores,
      description: t.seller.manageStoresDescription,
      icon: <StoreIcon className="size-4" />,
    },
    {
      href: "/seller/products",
      label: t.seller.products,
      description: t.seller.manageProductsDescription,
      icon: <PackageIcon className="size-4" />,
    },
    {
      href: "/seller/orders",
      label: t.seller.openOrders,
      description: t.seller.manageOrdersDescription,
      icon: <ClipboardListIcon className="size-4" />,
    },
  ]

  return (
    <SellerShell
      section="overview"
      title={t.seller.welcome.replace("{name}", profile.displayName || profile.email)}
      description={t.seller.description}
      headerActions={
        <>
          {onOpenAdminWorkspace ? (
            <Button type="button" variant="outline" size="sm" onClick={onOpenAdminWorkspace}>
              <ShieldCheckIcon />
              <span className="hidden sm:inline">{t.seller.openAdminWorkspace}</span>
            </Button>
          ) : null}
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={handleSignOut}
            disabled={signingOut}
          >
            {signingOut ? <Loader2Icon className="animate-spin" /> : <LogOutIcon />}
            <span className="hidden sm:inline">{t.home.signOut}</span>
          </Button>
        </>
      }
    >
      <section className="mt-5" aria-labelledby="seller-overview-stores-title">
        <h2 id="seller-overview-stores-title" className="font-heading text-lg font-semibold">
          {t.seller.stores}
        </h2>

        {stores.status === "loading" ? (
          <div className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 3 }, (_, index) => (
              <Skeleton key={index} className="h-24 rounded-sm" />
            ))}
          </div>
        ) : stores.status === "unavailable" ? (
          <ErrorState className="mt-3" title={t.seller.storeLoadFailed} />
        ) : stores.stores.length === 0 ? (
          <EmptyState
            className="mt-3"
            title={t.seller.noStores}
            description={t.seller.noStoresDescription}
          />
        ) : (
          <div className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <MetricCard
              label={t.seller.stores}
              value={stores.stores.length}
              icon={<StoreIcon className="size-4" />}
              tone="success"
            />
            {stores.stores.map((store) => (
              <MetricCard
                key={store.id}
                label={store.name}
                value={`/${store.slug}`}
                hint={t.seller.roleLabel}
              />
            ))}
          </div>
        )}
      </section>

      <section className="mt-7" aria-labelledby="seller-overview-sections-title">
        <h2 id="seller-overview-sections-title" className="font-heading text-lg font-semibold">
          {t.seller.workspace}
        </h2>
        <div className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {sections.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className="flex flex-col gap-1.5 rounded-sm border border-border bg-card p-4 transition-colors hover:bg-muted"
            >
              <span className="flex items-center gap-2 text-sm font-medium">
                {item.icon}
                {item.label}
              </span>
              <span className="text-sm text-muted-foreground">{item.description}</span>
            </Link>
          ))}
        </div>
      </section>
    </SellerShell>
  )
}

function isAbortError(error: unknown) {
  return error instanceof DOMException && error.name === "AbortError"
}
