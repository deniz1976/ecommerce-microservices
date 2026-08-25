"use client"

import { Loader2, X } from "lucide-react"
import { useEffect, useState } from "react"

import { ReferencePagination } from "@/components/admin/admin-reference-list-controls"
import { Button } from "@/components/ui/button"
import { getStockMovements } from "@/lib/api/inventory"
import { useI18n } from "@/lib/i18n/provider"
import type { PagedResult, StockMovement, StockMovementKind } from "@/types"

type State = { status: "loading" } | { status: "ready"; data: PagedResult<StockMovement> } | { status: "unavailable" }
const movementNames: Record<StockMovementKind, string> = { 1: "StockInitialized", 2: "StockIncreased", 3: "StockDecreased", 4: "StockReserved", 5: "StockReleased", 6: "AuditBaseline" }

export function StockMovementPanel({ productId, onClose }: { productId: string; onClose: () => void }) {
  const { locale, t } = useI18n()
  const [state, setState] = useState<State>({ status: "loading" })
  const [page, setPage] = useState(1)

  useEffect(() => {
    const controller = new AbortController()
    getStockMovements(productId, { pageNumber: page, pageSize: 20 }, controller.signal)
      .then((data) => setState({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) setState({ status: "unavailable" })
      })
    return () => controller.abort()
  }, [page, productId])

  return <section className="mt-6 overflow-hidden rounded-lg border bg-background" aria-labelledby="stock-movements-title">
    <div className="flex items-start justify-between gap-4 border-b p-4"><div><h2 id="stock-movements-title" className="font-heading text-lg font-semibold">{t.admin.stockMovements}</h2><p className="mt-1 break-all text-sm text-muted-foreground">{t.admin.stockMovementsDescription.replace("{productId}", productId)}</p></div><Button type="button" variant="ghost" size="icon-sm" onClick={onClose} aria-label={t.admin.close}><X /></Button></div>
    {state.status === "loading" ? <div className="flex min-h-36 items-center justify-center"><Loader2 className="size-5 animate-spin" /></div> : state.status === "unavailable" ? <p className="p-6 text-sm text-muted-foreground">{t.admin.stockMovementsUnavailable}</p> : state.data.items.length === 0 ? <p className="p-6 text-sm text-muted-foreground">{t.admin.noStockMovements}</p> : <>
      <div className="overflow-x-auto"><table className="w-full min-w-[72rem] text-left text-sm"><thead className="bg-muted/55 text-xs text-muted-foreground"><tr><th className="px-4 py-3">{t.admin.movementType}</th><th className="px-4 py-3 text-right">{t.admin.movementQuantity}</th><th className="px-4 py-3">{t.admin.movementOnHand}</th><th className="px-4 py-3">{t.admin.movementReserved}</th><th className="px-4 py-3">{t.admin.movementOrder}</th><th className="px-4 py-3">{t.admin.movementOccurredAt}</th></tr></thead><tbody className="divide-y">{state.data.items.map((item) => <tr key={item.id}><td className="px-4 py-3 font-medium">{movementNames[item.type]}</td><td className="px-4 py-3 text-right">{item.quantity}</td><td className="px-4 py-3">{item.quantityOnHandBefore} → {item.quantityOnHandAfter}</td><td className="px-4 py-3">{item.reservedQuantityBefore} → {item.reservedQuantityAfter}</td><td className="px-4 py-3 font-mono text-xs">{item.orderId ?? "-"}</td><td className="px-4 py-3">{formatDate(item.occurredAt, locale)}</td></tr>)}</tbody></table></div>
      <ReferencePagination page={state.data.pageNumber} totalPages={Math.max(1, state.data.totalPages)} totalCount={state.data.totalCount} pageLabel={t.admin.movementPageStatus} previousLabel={t.admin.previousPage} nextLabel={t.admin.nextPage} onPageChange={(value) => { setState({ status: "loading" }); setPage(value) }} />
    </>}
  </section>
}

function formatDate(value: string, locale: "en" | "tr") {
  return new Intl.DateTimeFormat(locale === "tr" ? "tr-TR" : "en-US", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value))
}
