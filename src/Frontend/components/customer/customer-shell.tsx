"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { useEffect, useState } from "react"
import type { ReactNode } from "react"
import {
  ClipboardListIcon,
  Loader2Icon,
  LogOutIcon,
  ShoppingCartIcon,
  StoreIcon,
} from "lucide-react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { CustomerNotificationLink } from "@/components/customer/customer-notification-link"
import { Button, buttonVariants } from "@/components/ui/button"
import { getProfile } from "@/lib/api/auth"
import { logoutFromAuth0 } from "@/lib/auth/auth0"
import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"

interface CustomerShellProps {
  customerId?: string | null
  children: ReactNode
}

export function CustomerShell({ customerId, children }: CustomerShellProps) {
  const { t } = useI18n()
  const pathname = usePathname()
  const [fetchedId, setFetchedId] = useState<string | null>(null)
  const [signingOut, setSigningOut] = useState(false)
  const resolvedId = customerId === undefined ? fetchedId : customerId

  useEffect(() => {
    if (customerId !== undefined) {
      return
    }

    let active = true
    getProfile()
      .then((profile) => {
        if (active && profile.roles.includes("Customer")) setFetchedId(profile.id)
      })
      .catch(() => undefined)

    return () => {
      active = false
    }
  }, [customerId])

  async function handleSignOut() {
    setSigningOut(true)
    await logoutFromAuth0("/login")
  }

  const links: Array<{ href: string; label: string; icon: ReactNode }> = [
    { href: "/", label: t.customer.catalog, icon: <StoreIcon /> },
    { href: "/orders", label: t.orders.openOrders, icon: <ClipboardListIcon /> },
    { href: "/basket", label: t.basket.openBasket, icon: <ShoppingCartIcon /> },
  ]

  return (
    <div className="min-h-svh bg-background">
      <header className="sticky top-0 z-20 flex h-14 items-center justify-between gap-3 border-b border-border bg-card px-4 sm:px-6">
        <Link href="/" aria-label={t.customer.catalog}>
          <Logo />
        </Link>

        <div className="flex items-center gap-2">
          {resolvedId ? (
            <nav className="flex items-center gap-2" aria-label={t.customer.navigation}>
              {links.map((link) => (
                <Link
                  key={link.href}
                  href={link.href}
                  aria-current={pathname === link.href ? "page" : undefined}
                  className={cn(
                    buttonVariants({ variant: "outline", size: "sm" }),
                    "gap-1.5",
                    pathname === link.href && "border-primary/40 text-primary",
                  )}
                >
                  {link.icon}
                  <span className="hidden sm:inline">{link.label}</span>
                </Link>
              ))}
              <CustomerNotificationLink customerId={resolvedId} />
            </nav>
          ) : null}

          <LanguageSwitcher />
          <ThemeToggle />

          {resolvedId ? (
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
          ) : (
            <Link href="/login" className={buttonVariants({ variant: "outline", size: "sm" })}>
              {t.login.signInCta}
            </Link>
          )}
        </div>
      </header>

      {children}
    </div>
  )
}
