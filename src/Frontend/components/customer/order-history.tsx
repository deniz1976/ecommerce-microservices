"use client"

import { ChevronLeft, ChevronRight, ClipboardList, Loader2, PackageOpen } from "lucide-react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { useEffect, useState } from "react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { Button, buttonVariants } from "@/components/ui/button"
import { getProfile } from "@/lib/api/auth"
import { getCustomerOrders } from "@/lib/api/orders"
import { useI18n } from "@/lib/i18n/provider"
import { getOrderStatusName } from "@/lib/orders/status"
import { cn } from "@/lib/utils"
import type { OrderSummary, PagedResult } from "@/types"

type OrderState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<OrderSummary> }
  | { status: "unavailable" }

export function OrderHistory() {
  const { locale, t } = useI18n()
  const router = useRouter()
  const [state, setState] = useState<OrderState>({ status: "loading" })
  const [customerId, setCustomerId] = useState<string | null>(null)
  const [page, setPage] = useState(1)

  useEffect(() => {
    let active = true
    getProfile()
      .then(async (profile) => {
        if (!profile.roles.includes("Customer")) {
          router.replace("/")
          return
        }

        if (active) setCustomerId(profile.id)
      })
      .catch(() => {
        if (active) setState({ status: "unavailable" })
      })

    return () => {
      active = false
    }
  }, [router])

  useEffect(() => {
    if (!customerId) return

    let active = true
    getCustomerOrders(customerId, page)
      .then((data) => {
        if (active) setState({ status: "ready", data })
      })
      .catch(() => {
        if (active) setState({ status: "unavailable" })
      })

    return () => {
      active = false
    }
  }, [customerId, page])

  return (
    <div className="min-h-svh bg-muted/30">
      <header className="flex h-16 items-center justify-between border-b border-border bg-background px-4 sm:px-6">
        <Logo />
        <div className="flex items-center gap-2">
          <LanguageSwitcher />
          <ThemeToggle />
          <Link href="/basket" className={buttonVariants({ variant: "outline", size: "sm" })}>
            {t.basket.openBasket}
          </Link>
        </div>
      </header>
      <main className="mx-auto w-full max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
        <Link href="/" className={cn(buttonVariants({ variant: "ghost" }), "-ml-2")}>
          {t.basket.continueShopping}
        </Link>
        <div className="mt-6 flex items-start gap-3">
          <span className="flex size-11 items-center justify-center rounded-xl bg-primary text-primary-foreground">
            <ClipboardList className="size-5" />
          </span>
          <div>
            <h1 className="font-heading text-3xl font-semibold tracking-tight">{t.orders.title}</h1>
            <p className="mt-1 text-sm text-muted-foreground">{t.orders.description}</p>
          </div>
        </div>

        {state.status === "loading" ? (
          <div className="flex min-h-72 items-center justify-center"><Loader2 className="size-6 animate-spin" /></div>
        ) : state.status === "unavailable" ? (
          <OrderMessage message={t.orders.loadFailed} />
        ) : state.data.items.length === 0 ? (
          <OrderMessage message={t.orders.empty} />
        ) : (
          <div className="mt-8 grid gap-4">
            {state.data.items.map((order) => (
              <Link
                key={order.id}
                href={`/orders/${order.id}`}
                className="grid gap-4 rounded-xl border border-border bg-card p-5 transition-colors hover:border-primary/40 sm:grid-cols-[1fr_auto_auto] sm:items-center"
              >
                <div>
                  <p className="text-xs text-muted-foreground">{t.orders.orderNumber}</p>
                  <p className="mt-1 font-mono text-sm font-medium">{order.id.slice(0, 8).toUpperCase()}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">{t.orders.placedAt}</p>
                  <p className="mt-1 text-sm font-medium">{formatDate(order.createdAt, locale)}</p>
                </div>
                <div className="sm:text-right">
                  <p className="text-sm font-semibold">{formatMoney(order.totalAmount, order.currency, locale)}</p>
                  <p className="mt-1 text-xs text-primary">{t.orders.status[getOrderStatusName(order.status)]}</p>
                </div>
              </Link>
            ))}
            <div className="flex items-center justify-between gap-3 border-t border-border pt-4">
              <p className="text-sm text-muted-foreground">
                {t.orders.pageStatus
                  .replace("{page}", String(state.data.pageNumber))
                  .replace("{total}", String(Math.max(1, state.data.totalPages)))}
              </p>
              <div className="flex gap-2">
                <Button
                  type="button"
                  variant="outline"
                  size="icon-sm"
                  disabled={state.data.pageNumber <= 1}
                  onClick={() => {
                    setState({ status: "loading" })
                    setPage((current) => Math.max(1, current - 1))
                  }}
                  aria-label={t.orders.previousPage}
                >
                  <ChevronLeft />
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="icon-sm"
                  disabled={state.data.pageNumber >= state.data.totalPages}
                  onClick={() => {
                    setState({ status: "loading" })
                    setPage((current) => current + 1)
                  }}
                  aria-label={t.orders.nextPage}
                >
                  <ChevronRight />
                </Button>
              </div>
            </div>
          </div>
        )}
      </main>
    </div>
  )
}

function OrderMessage({ message }: { message: string }) {
  return (
    <div className="mt-8 flex min-h-64 flex-col items-center justify-center rounded-xl border border-dashed border-border bg-card px-6 text-center">
      <PackageOpen className="size-9 text-muted-foreground/60" />
      <p className="mt-3 text-sm text-muted-foreground">{message}</p>
    </div>
  )
}

function formatMoney(amount: number, currency: string, locale: "en" | "tr") {
  return new Intl.NumberFormat(locale === "tr" ? "tr-TR" : "en-US", {
    style: "currency",
    currency,
  }).format(amount)
}

function formatDate(value: string, locale: "en" | "tr") {
  return new Intl.DateTimeFormat(locale === "tr" ? "tr-TR" : "en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value))
}
