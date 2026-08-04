"use client"

import type { ReactNode } from "react"
import Link from "next/link"
import { ArrowLeft, ShieldCheck, Tags } from "lucide-react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { buttonVariants } from "@/components/ui/button"
import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"

export function AdminReferencePageLayout({
  activePage,
  title,
  description,
  children,
}: {
  activePage: "categories" | "brands"
  title: string
  description: string
  children: ReactNode
}) {
  const { t } = useI18n()

  return (
    <div className="min-h-svh bg-muted/35">
      <header className="sticky top-0 z-10 flex h-16 items-center justify-between border-b border-border bg-background px-4 sm:px-6">
        <Logo />
        <div className="flex items-center gap-2">
          <Link
            href="/"
            className={cn(buttonVariants({ variant: "outline", size: "sm" }))}
          >
            <ArrowLeft />
            <span className="hidden sm:inline">{t.admin.backToOverview}</span>
          </Link>
          <LanguageSwitcher />
          <ThemeToggle />
        </div>
      </header>

      <main className="mx-auto w-full max-w-7xl px-4 py-7 sm:px-6 lg:px-8">
        <div className="flex items-center gap-2 text-sm text-primary">
          <ShieldCheck className="size-4" />
          <span className="font-medium">{t.admin.roleLabel}</span>
        </div>
        <div className="mt-2 flex flex-col justify-between gap-4 border-b border-border pb-6 lg:flex-row lg:items-end">
          <div>
            <h1 className="font-heading text-2xl font-semibold text-foreground sm:text-3xl">
              {title}
            </h1>
            <p className="mt-2 max-w-3xl text-sm leading-6 text-muted-foreground">
              {description}
            </p>
          </div>
          <nav className="flex gap-2" aria-label={t.admin.referenceNavigation}>
            <Link
              href="/admin/categories"
              className={cn(
                buttonVariants({
                  variant: activePage === "categories" ? "secondary" : "outline",
                }),
              )}
              aria-current={activePage === "categories" ? "page" : undefined}
            >
              <Tags />
              {t.admin.categories}
            </Link>
            <Link
              href="/admin/brands"
              className={cn(
                buttonVariants({
                  variant: activePage === "brands" ? "secondary" : "outline",
                }),
              )}
              aria-current={activePage === "brands" ? "page" : undefined}
            >
              <Tags />
              {t.admin.brands}
            </Link>
          </nav>
        </div>

        {children}
      </main>
    </div>
  )
}
