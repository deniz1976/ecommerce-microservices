"use client"

import { ArrowLeft, ShieldCheck } from "lucide-react"
import Link from "next/link"
import type { ReactNode } from "react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { buttonVariants } from "@/components/ui/button"
import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"

export function AdminPageLayout({
  title,
  description,
  children,
}: {
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
          <Link href="/" className={cn(buttonVariants({ variant: "outline", size: "sm" }))}>
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
        <div className="mt-2 border-b border-border pb-6">
          <h1 className="font-heading text-2xl font-semibold text-foreground sm:text-3xl">{title}</h1>
          <p className="mt-2 max-w-3xl text-sm leading-6 text-muted-foreground">{description}</p>
        </div>
        {children}
      </main>
    </div>
  )
}
