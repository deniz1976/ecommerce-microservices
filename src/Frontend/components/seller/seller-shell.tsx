"use client"

import Link from "next/link"
import type { ReactNode } from "react"
import { ClipboardListIcon, LayoutDashboardIcon, PackageIcon, StoreIcon } from "lucide-react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"

export type SellerSection = "overview" | "stores" | "products" | "orders"

interface SellerShellProps {
  section: SellerSection
  title: string
  description: string
  headerActions?: ReactNode
  children: ReactNode
}

export function SellerShell({
  section,
  title,
  description,
  headerActions,
  children,
}: SellerShellProps) {
  const { t } = useI18n()

  const items: Array<{ id: SellerSection; href: string; label: string; icon: ReactNode }> = [
    { id: "overview", href: "/", label: t.seller.overview, icon: <LayoutDashboardIcon className="size-4" /> },
    { id: "stores", href: "/seller/stores", label: t.seller.stores, icon: <StoreIcon className="size-4" /> },
    { id: "products", href: "/seller/products", label: t.seller.products, icon: <PackageIcon className="size-4" /> },
    { id: "orders", href: "/seller/orders", label: t.seller.openOrders, icon: <ClipboardListIcon className="size-4" /> },
  ]

  return (
    <div className="min-h-svh bg-background">
      <header className="sticky top-0 z-20 flex h-14 items-center justify-between gap-3 border-b border-border bg-card px-4 sm:px-6">
        <div className="flex items-center gap-3">
          <Logo />
          <span className="hidden items-center gap-1.5 text-xs font-medium text-muted-foreground sm:flex">
            <StoreIcon className="size-3.5" />
            {t.seller.roleLabel}
          </span>
        </div>
        <div className="flex items-center gap-2">
          {headerActions}
          <LanguageSwitcher />
          <ThemeToggle />
        </div>
      </header>

      <div className="mx-auto grid w-full max-w-[104rem] lg:grid-cols-[13rem_minmax(0,1fr)]">
        <aside className="hidden border-r border-border bg-card lg:block">
          <nav
            className="sticky top-14 flex flex-col gap-0.5 p-2"
            aria-label={t.seller.navigation}
          >
            {items.map((item) => (
              <Link
                key={item.id}
                href={item.href}
                aria-current={item.id === section ? "page" : undefined}
                className={cn(
                  "flex h-8 items-center gap-2.5 rounded-sm px-2.5 text-sm transition-colors",
                  item.id === section
                    ? "bg-accent font-medium text-accent-foreground"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground",
                )}
              >
                {item.icon}
                {item.label}
              </Link>
            ))}
          </nav>
        </aside>

        <main className="min-w-0 px-4 py-5 sm:px-6">
          <div className="border-b border-border pb-4">
            <h1 className="font-heading text-xl font-bold tracking-tight sm:text-2xl">{title}</h1>
            <p className="mt-1 max-w-3xl text-sm text-muted-foreground">{description}</p>
          </div>

          <nav
            className="-mx-4 mt-3 flex gap-1 overflow-x-auto px-4 pb-1 lg:hidden"
            aria-label={t.seller.navigation}
          >
            {items.map((item) => (
              <Link
                key={item.id}
                href={item.href}
                aria-current={item.id === section ? "page" : undefined}
                className={cn(
                  "flex h-8 shrink-0 items-center rounded-sm border px-2.5 text-sm whitespace-nowrap",
                  item.id === section
                    ? "border-transparent bg-accent font-medium text-accent-foreground"
                    : "border-border text-muted-foreground",
                )}
              >
                {item.label}
              </Link>
            ))}
          </nav>

          {children}
        </main>
      </div>
    </div>
  )
}
