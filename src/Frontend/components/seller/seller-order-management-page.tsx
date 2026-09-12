"use client"

import {
  ChevronDownIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ChevronUpIcon,
  ClipboardListIcon,
  Loader2Icon,
  StoreIcon,
} from "lucide-react"
import Link from "next/link"
import { useEffect, useRef, useState } from "react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { FilterBar, FilterField } from "@/components/patterns/filter-bar"
import { EmptyState, ErrorState } from "@/components/patterns/states"
import { StatusBadge } from "@/components/patterns/status-badge"
import { Button, buttonVariants } from "@/components/ui/button"
import { SelectNative } from "@/components/ui/select-native"
import { Skeleton } from "@/components/ui/skeleton"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { getMyCatalogStores } from "@/lib/api/catalog"
import { getSellerOrder, getSellerOrders } from "@/lib/api/orders"
import { formatDateTime, formatMoney } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import { orderStatusLabel, orderStatusTone } from "@/lib/i18n/status"
import { cn } from "@/lib/utils"
import type {
  CatalogStore,
  OrderStatus,
  PagedResult,
  SellerOrderDetail,
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
  const [reloadToken, setReloadToken] = useState(0)

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
  }, [page, pageSize, reloadToken, selectedStoreId, sortDescending, status])

  const selectedStore =
    stores.status === "ready"
      ? stores.stores.find((store) => store.id === selectedStoreId)
      : undefined

  const retry = () => {
    setOrders({ status: "loading" })
    setReloadToken((token) => token + 1)
  }

  return (
    <div className="min-h-svh bg-muted/35">
      <header className="sticky top-0 z-10 flex h-16 items-center justify-between border-b border-border bg-background px-4 sm:px-6">
        <Logo />
        <div className="flex items-center gap-2">
          <Link href="/" className={cn(buttonVariants({ variant: "outline", size: "sm" }))}>
            <StoreIcon />
            <span className="hidden sm:inline">{t.seller.backToWorkspace}</span>
          </Link>
          <LanguageSwitcher />
          <ThemeToggle />
        </div>
      </header>

      <main className="mx-auto w-full max-w-7xl px-4 py-7 sm:px-6 lg:px-8">
        <div className="flex items-center gap-2 text-sm text-primary">
          <ClipboardListIcon className="size-4" />
          <span className="font-medium">{t.seller.roleLabel}</span>
        </div>
        <div className="mt-2 border-b border-border pb-6">
          <h1 className="font-heading text-2xl font-semibold sm:text-3xl">
            {t.seller.orderPageTitle}
          </h1>
          <p className="mt-2 max-w-3xl text-sm leading-6 text-muted-foreground">
            {t.seller.orderPageDescription}
          </p>
        </div>

        <section className="mt-6 flex flex-col gap-4" aria-labelledby="seller-order-list-title">
          <FilterBar>
            <FilterField label={t.seller.orderStore} className="min-w-56 flex-1">
              <SelectNative
                value={selectedStoreId}
                disabled={stores.status !== "ready" || stores.stores.length === 0}
                onChange={(event) => {
                  setOrders({ status: "loading" })
                  setSelectedStoreId(event.target.value)
                  setPage(1)
                }}
              >
                {stores.status === "ready" && stores.stores.length > 0 ? (
                  stores.stores.map((store) => (
                    <option key={store.id} value={store.id}>
                      {store.name}
                    </option>
                  ))
                ) : (
                  <option value="">{t.seller.selectStore}</option>
                )}
              </SelectNative>
            </FilterField>

            <FilterField label={t.seller.orderStatus}>
              <SelectNative
                value={String(status)}
                onChange={(event) => {
                  setOrders({ status: "loading" })
                  setStatus(
                    event.target.value === "all"
                      ? "all"
                      : (Number(event.target.value) as OrderStatus),
                  )
                  setPage(1)
                }}
              >
                <option value="all">{t.seller.allOrderStatuses}</option>
                {orderStatuses.map((value) => (
                  <option key={value} value={value}>
                    {orderStatusLabel(value, t.orders.status)}
                  </option>
                ))}
              </SelectNative>
            </FilterField>

            <FilterField label={t.seller.orderSort}>
              <SelectNative
                value={sortDescending ? "newest" : "oldest"}
                onChange={(event) => {
                  setOrders({ status: "loading" })
                  setSortDescending(event.target.value === "newest")
                  setPage(1)
                }}
              >
                <option value="newest">{t.seller.newestOrders}</option>
                <option value="oldest">{t.seller.oldestOrders}</option>
              </SelectNative>
            </FilterField>

            <FilterField label={t.seller.orderRowsPerPage} className="min-w-24">
              <SelectNative
                value={String(pageSize)}
                onChange={(event) => {
                  setOrders({ status: "loading" })
                  setPageSize(Number(event.target.value))
                  setPage(1)
                }}
              >
                {[10, 20, 50].map((value) => (
                  <option key={value} value={value}>
                    {value}
                  </option>
                ))}
              </SelectNative>
            </FilterField>
          </FilterBar>

          <div className="flex flex-col justify-between gap-2 sm:flex-row sm:items-end">
            <div>
              <h2 id="seller-order-list-title" className="font-heading text-xl font-semibold">
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
            <div className="flex flex-col gap-4">
              {Array.from({ length: 3 }, (_, index) => (
                <Skeleton key={index} className="h-32 rounded-xl" />
              ))}
            </div>
          ) : stores.status === "unavailable" ? (
            <ErrorState title={t.seller.storeLoadFailed} />
          ) : stores.status === "ready" && stores.stores.length === 0 ? (
            <EmptyState title={t.seller.noStores} description={t.seller.noStoresDescription} />
          ) : orders.status === "unavailable" ? (
            <ErrorState title={t.seller.ordersLoadFailed} onRetry={retry} />
          ) : orders.status === "ready" && orders.data.items.length > 0 ? (
            <div className="flex flex-col gap-4">
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
            <EmptyState title={t.seller.noOrders} />
          )}
        </section>
      </main>
    </div>
  )
}

function SellerOrderCard({ order }: { order: SellerOrderSummary }) {
  const { locale, t } = useI18n()
  const [expanded, setExpanded] = useState(false)
  const detailAbortController = useRef<AbortController | null>(null)
  const [detail, setDetail] = useState<
    | { status: "idle" }
    | { status: "loading" }
    | { status: "ready"; data: SellerOrderDetail }
    | { status: "unavailable" }
  >({ status: "idle" })

  useEffect(() => () => detailAbortController.current?.abort(), [])

  const toggleDetails = () => {
    if (expanded) {
      detailAbortController.current?.abort()
      setExpanded(false)
      return
    }

    setExpanded(true)
    if (detail.status === "ready") return

    detailAbortController.current?.abort()
    const controller = new AbortController()
    detailAbortController.current = controller
    setDetail({ status: "loading" })
    getSellerOrder(order.storeId, order.orderId, controller.signal)
      .then((data) => setDetail({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!isAbortError(error)) setDetail({ status: "unavailable" })
      })
  }

  return (
    <article className="overflow-hidden rounded-xl border border-border bg-card">
      <div className="grid gap-4 border-b border-border bg-muted/25 px-5 py-4 sm:grid-cols-2 lg:grid-cols-4">
        <OrderFact
          label={t.seller.orderNumber}
          value={order.orderId.slice(0, 8).toUpperCase()}
          mono
        />
        <OrderFact label={t.seller.placedAt} value={formatDateTime(order.createdAt, locale)} />
        <OrderFact label={t.seller.lastUpdated} value={formatDateTime(order.updatedAt, locale)} />
        <div className="flex flex-col gap-2 lg:items-end">
          <div className="flex flex-col gap-1 lg:items-end">
            <span className="text-xs text-muted-foreground">{t.seller.storeTotal}</span>
            <span className="font-heading text-lg font-semibold tabular-nums">
              {formatMoney(order.storeTotalAmount, order.currency, locale)}
            </span>
          </div>
          <StatusBadge
            label={orderStatusLabel(order.status, t.orders.status)}
            tone={orderStatusTone(order.status)}
          />
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={toggleDetails}
            aria-expanded={expanded}
          >
            {expanded ? <ChevronUpIcon /> : <ChevronDownIcon />}
            {expanded
              ? t.seller.hideOrderItems
              : t.seller.showOrderItems.replace("{count}", String(order.itemCount))}
          </Button>
        </div>
      </div>
      {expanded && detail.status === "loading" ? (
        <div className="flex min-h-28 items-center justify-center">
          <Loader2Icon className="size-5 animate-spin text-muted-foreground" />
          <span className="sr-only">{t.common.loading}</span>
        </div>
      ) : expanded && detail.status === "unavailable" ? (
        <p className="px-5 py-6 text-sm text-destructive">{t.seller.orderItemsLoadFailed}</p>
      ) : expanded && detail.status === "ready" ? (
        <SellerOrderItemsTable order={detail.data} />
      ) : null}
    </article>
  )
}

function SellerOrderItemsTable({ order }: { order: SellerOrderDetail }) {
  const { locale, t } = useI18n()
  return (
    <div className="overflow-x-auto">
      <Table className="min-w-[42rem]">
        <TableHeader>
          <TableRow>
            <TableHead>{t.seller.orderProduct}</TableHead>
            <TableHead className="text-right">{t.seller.orderQuantity}</TableHead>
            <TableHead className="text-right">{t.seller.orderUnitPrice}</TableHead>
            <TableHead className="text-right">{t.seller.orderLineTotal}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {order.items.map((item) => (
            <TableRow key={item.id}>
              <TableCell className="font-medium">{item.productName}</TableCell>
              <TableCell className="text-right text-muted-foreground tabular-nums">
                {item.quantity}
              </TableCell>
              <TableCell className="text-right text-muted-foreground tabular-nums">
                {formatMoney(item.unitPrice, item.currency, locale)}
              </TableCell>
              <TableCell className="text-right font-medium tabular-nums">
                {formatMoney(item.totalPrice, item.currency, locale)}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
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
          <ChevronLeftIcon />
        </Button>
        <Button
          type="button"
          variant="outline"
          size="icon-sm"
          disabled={data.pageNumber >= totalPages}
          onClick={() => onPageChange(data.pageNumber + 1)}
          aria-label={t.seller.nextOrderPage}
        >
          <ChevronRightIcon />
        </Button>
      </div>
    </div>
  )
}

function OrderFact({ label, value, mono = false }: { label: string; value: string; mono?: boolean }) {
  return (
    <div className="flex flex-col gap-1">
      <span className="text-xs text-muted-foreground">{label}</span>
      <span className={cn("text-sm font-medium", mono && "font-mono")}>{value}</span>
    </div>
  )
}

function isAbortError(error: unknown): boolean {
  return error instanceof DOMException && error.name === "AbortError"
}
