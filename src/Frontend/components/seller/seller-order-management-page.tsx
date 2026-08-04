"use client"

import {
  ChevronLeft,
  ChevronRight,
  ClipboardList,
  Loader2,
  PackageOpen,
  Store,
} from "lucide-react"
import Link from "next/link"
import type { ReactNode } from "react"
import { useEffect, useState } from "react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { Button, buttonVariants } from "@/components/ui/button"
import { getMyCatalogStores } from "@/lib/api/catalog"
import { getSellerOrders } from "@/lib/api/orders"
import { useI18n } from "@/lib/i18n/provider"
import { getOrderStatusName } from "@/lib/orders/status"
import { cn } from "@/lib/utils"
import type {
  CatalogStore,
  OrderStatus,
  PagedResult,
  SellerOrderSummary,
} from "@/types"

type StoreState =
  | { status: "loading" }
  | { status: "ready"; stores: CatalogStore[] }
  | { status: "unavailable" }

type OrderState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<SellerOrderSummary> }
  | { status: "unavailable" }

const orderStatuses: readonly OrderStatus[] = [0, 1, 2, 3, 4, 5, 6]

export function SellerOrderManagementPage() {
  const { t } = useI18n()
  const [stores, setStores] = useState<StoreState>({ status: "loading" })
  const [selectedStoreId, setSelectedStoreId] = useState("")
  const [orders, setOrders] = useState<OrderState>({ status: "idle" })
  const [status, setStatus] = useState<"all" | OrderStatus>("all")
  const [sortDescending, setSortDescending] = useState(true)
  const [pageSize, setPageSize] = useState(20)
  const [page, setPage] = useState(1)

  useEffect(() => {
    const controller = new AbortController()
    getMyCatalogStores(controller.signal)
      .then((data) => {
        setStores({ status: "ready", stores: data })
        if (data.length > 0) setOrders({ status: "loading" })
        setSelectedStoreId((current) => current || data[0]?.id || "")
      })
      .catch((error: unknown) => {
        if (!isAbortError(error)) setStores({ status: "unavailable" })
      })

    return () => controller.abort()
  }, [])

  useEffect(() => {
    if (!selectedStoreId) {
      return
    }

    const controller = new AbortController()
    getSellerOrders(
      selectedStoreId,
      {
        pageNumber: page,
        pageSize,
        status: status === "all" ? undefined : status,
        sortDescending,
      },
      controller.signal,
    )
      .then((data) => setOrders({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!isAbortError(error)) setOrders({ status: "unavailable" })
      })

    return () => controller.abort()
  }, [page, pageSize, selectedStoreId, sortDescending, status])

  const selectedStore = stores.status === "ready"
    ? stores.stores.find((store) => store.id === selectedStoreId)
    : undefined

  return (
    <div className="min-h-svh bg-muted/35">
      <header className="sticky top-0 z-10 flex h-16 items-center justify-between border-b border-border bg-background px-4 sm:px-6">
        <Logo />
        <div className="flex items-center gap-2">
          <Link href="/" className={cn(buttonVariants({ variant: "outline", size: "sm" }))}>
            <Store />
            <span className="hidden sm:inline">{t.seller.backToWorkspace}</span>
          </Link>
          <LanguageSwitcher />
          <ThemeToggle />
        </div>
      </header>

      <main className="mx-auto w-full max-w-7xl px-4 py-7 sm:px-6 lg:px-8">
        <div className="flex items-center gap-2 text-sm text-primary">
          <ClipboardList className="size-4" />
          <span className="font-medium">{t.seller.roleLabel}</span>
        </div>
        <div className="mt-2 border-b border-border pb-6">
          <h1 className="font-heading text-2xl font-semibold text-foreground sm:text-3xl">
            {t.seller.orderPageTitle}
          </h1>
          <p className="mt-2 max-w-3xl text-sm leading-6 text-muted-foreground">
            {t.seller.orderPageDescription}
          </p>
        </div>

        <section className="mt-6" aria-labelledby="seller-order-list-title">
          <div className="grid gap-4 rounded-xl border border-border bg-background p-4 sm:grid-cols-2 lg:grid-cols-4">
            <SelectControl
              label={t.seller.orderStore}
              value={selectedStoreId}
              disabled={stores.status !== "ready" || stores.stores.length === 0}
              onChange={(value) => {
                setOrders({ status: "loading" })
                setSelectedStoreId(value)
                setPage(1)
              }}
            >
              {stores.status === "ready" && stores.stores.length > 0 ? (
                stores.stores.map((store) => (
                  <option key={store.id} value={store.id}>{store.name}</option>
                ))
              ) : (
                <option value="">{t.seller.selectStore}</option>
              )}
            </SelectControl>
            <SelectControl
              label={t.seller.orderStatus}
              value={String(status)}
              onChange={(value) => {
                setOrders({ status: "loading" })
                setStatus(value === "all" ? "all" : Number(value) as OrderStatus)
                setPage(1)
              }}
            >
              <option value="all">{t.seller.allOrderStatuses}</option>
              {orderStatuses.map((value) => (
                <option key={value} value={value}>
                  {t.orders.status[getOrderStatusName(value)]}
                </option>
              ))}
            </SelectControl>
            <SelectControl
              label={t.seller.orderSort}
              value={sortDescending ? "newest" : "oldest"}
              onChange={(value) => {
                setOrders({ status: "loading" })
                setSortDescending(value === "newest")
                setPage(1)
              }}
            >
              <option value="newest">{t.seller.newestOrders}</option>
              <option value="oldest">{t.seller.oldestOrders}</option>
            </SelectControl>
            <SelectControl
              label={t.seller.orderRowsPerPage}
              value={String(pageSize)}
              onChange={(value) => {
                setOrders({ status: "loading" })
                setPageSize(Number(value))
                setPage(1)
              }}
            >
              {[10, 20, 50].map((value) => (
                <option key={value} value={value}>{value}</option>
              ))}
            </SelectControl>
          </div>

          <div className="mt-6 flex flex-col justify-between gap-2 sm:flex-row sm:items-end">
            <div>
              <h2 id="seller-order-list-title" className="font-heading text-xl font-semibold text-foreground">
                {selectedStore?.name ?? t.seller.orderPageTitle}
              </h2>
              {selectedStore ? (
                <p className="mt-1 text-sm text-muted-foreground">/{selectedStore.slug}</p>
              ) : null}
            </div>
            {orders.status === "ready" ? (
              <p className="text-sm text-muted-foreground">
                {t.seller.orderCount.replace("{count}", String(orders.data.totalCount))}
              </p>
            ) : null}
          </div>

          {stores.status === "loading" || orders.status === "loading" ? (
            <SellerOrderMessage loading message={t.common.loading} />
          ) : stores.status === "unavailable" ? (
            <SellerOrderMessage message={t.seller.storeLoadFailed} />
          ) : stores.status === "ready" && stores.stores.length === 0 ? (
            <SellerOrderMessage message={`${t.seller.noStores} ${t.seller.noStoresDescription}`} />
          ) : orders.status === "unavailable" ? (
            <SellerOrderMessage message={t.seller.ordersLoadFailed} />
          ) : orders.status === "ready" && orders.data.items.length > 0 ? (
            <div className="mt-4 space-y-4">
              {orders.data.items.map((order) => (
                <SellerOrderCard key={order.orderId} order={order} />
              ))}
              <SellerOrderPagination
                data={orders.data}
                onPageChange={(value) => {
                  setOrders({ status: "loading" })
                  setPage(value)
                }}
              />
            </div>
          ) : (
            <SellerOrderMessage message={t.seller.noOrders} />
          )}
        </section>
      </main>
    </div>
  )
}

function SellerOrderCard({ order }: { order: SellerOrderSummary }) {
  const { locale, t } = useI18n()
  return (
    <article className="overflow-hidden rounded-xl border border-border bg-background">
      <div className="grid gap-4 border-b border-border bg-muted/25 px-5 py-4 sm:grid-cols-2 lg:grid-cols-4">
        <OrderFact label={t.seller.orderNumber} value={order.orderId.slice(0, 8).toUpperCase()} mono />
        <OrderFact label={t.seller.placedAt} value={formatDate(order.createdAt, locale)} />
        <OrderFact label={t.seller.lastUpdated} value={formatDate(order.updatedAt, locale)} />
        <div className="lg:text-right">
          <p className="text-xs text-muted-foreground">{t.seller.storeTotal}</p>
          <p className="mt-1 font-heading text-lg font-semibold text-foreground">
            {formatMoney(order.storeTotalAmount, order.currency, locale)}
          </p>
          <p className="mt-1 text-xs font-medium text-primary">
            {t.orders.status[getOrderStatusName(order.status)]}
          </p>
        </div>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full min-w-[42rem] border-collapse text-left text-sm">
          <caption className="sr-only">{t.seller.orderItems}</caption>
          <thead className="text-xs text-muted-foreground">
            <tr>
              <th scope="col" className="px-5 py-3 font-medium">{t.seller.orderProduct}</th>
              <th scope="col" className="px-5 py-3 text-right font-medium">{t.seller.orderQuantity}</th>
              <th scope="col" className="px-5 py-3 text-right font-medium">{t.seller.orderUnitPrice}</th>
              <th scope="col" className="px-5 py-3 text-right font-medium">{t.seller.orderLineTotal}</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-border">
            {order.items.map((item) => (
              <tr key={item.id}>
                <td className="px-5 py-3 font-medium text-foreground">{item.productName}</td>
                <td className="px-5 py-3 text-right text-muted-foreground">{item.quantity}</td>
                <td className="px-5 py-3 text-right text-muted-foreground">
                  {formatMoney(item.unitPrice, item.currency, locale)}
                </td>
                <td className="px-5 py-3 text-right font-medium text-foreground">
                  {formatMoney(item.totalPrice, item.currency, locale)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </article>
  )
}

function SellerOrderPagination({
  data,
  onPageChange,
}: {
  data: PagedResult<SellerOrderSummary>
  onPageChange: (page: number) => void
}) {
  const { t } = useI18n()
  const totalPages = Math.max(1, data.totalPages)
  return (
    <div className="flex items-center justify-between gap-3 border-t border-border pt-4">
      <p className="text-sm text-muted-foreground">
        {t.seller.orderPageStatus
          .replace("{page}", String(data.pageNumber))
          .replace("{total}", String(totalPages))
          .replace("{count}", String(data.totalCount))}
      </p>
      <div className="flex gap-2">
        <Button
          type="button"
          variant="outline"
          size="icon-sm"
          disabled={data.pageNumber <= 1}
          onClick={() => onPageChange(Math.max(1, data.pageNumber - 1))}
          aria-label={t.seller.previousOrderPage}
        >
          <ChevronLeft />
        </Button>
        <Button
          type="button"
          variant="outline"
          size="icon-sm"
          disabled={data.pageNumber >= totalPages}
          onClick={() => onPageChange(data.pageNumber + 1)}
          aria-label={t.seller.nextOrderPage}
        >
          <ChevronRight />
        </Button>
      </div>
    </div>
  )
}

function SelectControl({
  label,
  value,
  onChange,
  disabled = false,
  children,
}: {
  label: string
  value: string
  onChange: (value: string) => void
  disabled?: boolean
  children: ReactNode
}) {
  return (
    <label className="grid gap-2 text-sm font-medium text-foreground">
      {label}
      <select
        value={value}
        disabled={disabled}
        onChange={(event) => onChange(event.target.value)}
        className="h-10 rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
      >
        {children}
      </select>
    </label>
  )
}

function OrderFact({ label, value, mono = false }: { label: string; value: string; mono?: boolean }) {
  return (
    <div>
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className={cn("mt-1 text-sm font-medium text-foreground", mono && "font-mono")}>{value}</p>
    </div>
  )
}

function SellerOrderMessage({ message, loading = false }: { message: string; loading?: boolean }) {
  return (
    <div className="mt-4 flex min-h-56 flex-col items-center justify-center rounded-xl border border-dashed border-border bg-background px-6 text-center">
      {loading ? (
        <Loader2 className="size-7 animate-spin text-muted-foreground" />
      ) : (
        <PackageOpen className="size-8 text-muted-foreground/60" />
      )}
      <p className="mt-3 text-sm text-muted-foreground">{message}</p>
    </div>
  )
}

function isAbortError(error: unknown): boolean {
  return error instanceof DOMException && error.name === "AbortError"
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
