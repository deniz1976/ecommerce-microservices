"use client"

import Link from "next/link"
import { useEffect, type ReactNode } from "react"
import {
  ActivityIcon,
  ClipboardListIcon,
  LayoutDashboardIcon,
  PackageIcon,
  PackageSearchIcon,
  ReceiptTextIcon,
  ShieldCheckIcon,
  StoreIcon,
  TagIcon,
  TagsIcon,
  TruckIcon,
  UsersIcon,
} from "lucide-react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { SignOutButton } from "@/components/auth/sign-out-button"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"
import { storeWorkspace } from "@/lib/workspace/preference"

export type AdminSection =
  | "overview"
  | "catalog"
  | "categories"
  | "brands"
  | "stores"
  | "users"
  | "orders"
  | "inventory"
  | "payments"
  | "shipments"
  | "workflows"

interface AdminShellProps {
  section: AdminSection
  title: string
  description: string
  headerActions?: ReactNode
  children: ReactNode
}

export function AdminShell({
  section,
  title,
  description,
  headerActions,
  children,
}: AdminShellProps) {
  const { t } = useI18n()

  useEffect(() => {
    storeWorkspace("Admin")
  }, [])

  const items: Array<{ id: AdminSection; href: string; label: string; icon: ReactNode }> = [
    { id: "overview", href: "/", label: t.admin.overview, icon: <LayoutDashboardIcon className="size-4" /> },
    { id: "catalog", href: "/#admin-catalog", label: t.admin.catalog, icon: <PackageIcon className="size-4" /> },
    { id: "categories", href: "/admin/categories", label: t.admin.categories, icon: <TagsIcon className="size-4" /> },
    { id: "brands", href: "/admin/brands", label: t.admin.brands, icon: <TagIcon className="size-4" /> },
    { id: "stores", href: "/admin/stores", label: t.admin.stores, icon: <StoreIcon className="size-4" /> },
    { id: "users", href: "/#admin-users", label: t.admin.users, icon: <UsersIcon className="size-4" /> },
    { id: "orders", href: "/admin/orders", label: t.admin.orders, icon: <ClipboardListIcon className="size-4" /> },
    { id: "inventory", href: "/admin/inventory", label: t.admin.inventory, icon: <PackageSearchIcon className="size-4" /> },
    { id: "payments", href: "/admin/payments", label: t.admin.payments, icon: <ReceiptTextIcon className="size-4" /> },
    { id: "shipments", href: "/admin/shipments", label: t.admin.shipments, icon: <TruckIcon className="size-4" /> },
    { id: "workflows", href: "/admin/workflows", label: t.admin.workflows, icon: <ActivityIcon className="size-4" /> },
  ]

  return (
    <div className="min-h-svh bg-background">
      <header className="sticky top-0 z-20 flex h-14 items-center justify-between gap-3 border-b border-border bg-card px-4 sm:px-6">
        <div className="flex items-center gap-3">
          <Logo />
          <span className="hidden items-center gap-1.5 text-xs font-medium text-muted-foreground sm:flex">
            <ShieldCheckIcon className="size-3.5" />
            {t.admin.roleLabel}
          </span>
        </div>
        <div className="flex items-center gap-2">
          {headerActions}
          <SignOutButton />
          <LanguageSwitcher />
          <ThemeToggle />
        </div>
      </header>

      <div className="mx-auto grid w-full max-w-[104rem] lg:grid-cols-[13rem_minmax(0,1fr)]">
        <aside className="hidden border-r border-border bg-card lg:block">
          <nav
            className="sticky top-14 flex flex-col gap-0.5 p-2"
            aria-label={t.admin.navigation}
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
            aria-label={t.admin.navigation}
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
