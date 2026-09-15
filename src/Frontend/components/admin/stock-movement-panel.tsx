"use client"

import { XIcon } from "lucide-react"
import { useEffect, useState } from "react"

import { DataTable, type DataTableColumn } from "@/components/patterns/data-table"
import { StatusBadge } from "@/components/patterns/status-badge"
import { Button } from "@/components/ui/button"
import { getStockMovements } from "@/lib/api/inventory"
import { formatDateTime } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import { stockMovementLabel, stockMovementTone } from "@/lib/i18n/status"
import type { PagedResult, StockMovement } from "@/types"

type State =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<StockMovement> }
  | { status: "unavailable" }

export function StockMovementPanel({
  productId,
  onClose,
}: {
  productId: string
  onClose: () => void
}) {
  const { locale, t } = useI18n()
  const [state, setState] = useState<State>({ status: "loading" })
  const [page, setPage] = useState(1)
  const [reloadToken, setReloadToken] = useState(0)

  useEffect(() => {
    const controller = new AbortController()
    getStockMovements(productId, { pageNumber: page, pageSize: 20 }, controller.signal)
      .then((data) => setState({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) {
          setState({ status: "unavailable" })
        }
      })
    return () => controller.abort()
  }, [page, productId, reloadToken])

  const columns: DataTableColumn<StockMovement>[] = [
    {
      id: "type",
      header: t.admin.movementType,
      cell: (row) => (
        <StatusBadge
          label={stockMovementLabel(row.type, t.status)}
          tone={stockMovementTone(row.type)}
        />
      ),
    },
    {
      id: "quantity",
      header: t.admin.movementQuantity,
      headerClassName: "text-right",
      className: "text-right tabular-nums",
      cell: (row) => row.quantity,
    },
    {
      id: "onHand",
      header: t.admin.movementOnHand,
      className: "text-muted-foreground tabular-nums",
      cell: (row) => `${row.quantityOnHandBefore} → ${row.quantityOnHandAfter}`,
    },
    {
      id: "reserved",
      header: t.admin.movementReserved,
      className: "text-muted-foreground tabular-nums",
      cell: (row) => `${row.reservedQuantityBefore} → ${row.reservedQuantityAfter}`,
    },
    {
      id: "orderId",
      header: t.admin.movementOrder,
      cell: (row) => (
        <span className="font-mono text-xs text-muted-foreground">{row.orderId ?? "-"}</span>
      ),
    },
    {
      id: "occurredAt",
      header: t.admin.movementOccurredAt,
      className: "text-muted-foreground tabular-nums",
      cell: (row) => formatDateTime(row.occurredAt, locale),
    },
  ]

  return (
    <section className="flex flex-col gap-4" aria-labelledby="stock-movements-title">
      <div className="flex items-start justify-between gap-4 border border-border bg-card p-4">
        <div>
          <h2 id="stock-movements-title" className="font-heading text-lg font-semibold">
            {t.admin.stockMovements}
          </h2>
          <p className="mt-1 text-sm break-all text-muted-foreground">
            {t.admin.stockMovementsDescription.replace("{productId}", productId)}
          </p>
        </div>
        <Button
          type="button"
          variant="ghost"
          size="icon-sm"
          onClick={onClose}
          aria-label={t.admin.close}
        >
          <XIcon />
        </Button>
      </div>

      <DataTable
        columns={columns}
        page={state.status === "ready" ? state.data : null}
        rowKey={(row) => row.id}
        isLoading={state.status === "loading"}
        error={state.status === "unavailable"}
        onRetry={() => {
          setState({ status: "loading" })
          setReloadToken((token) => token + 1)
        }}
        onPageChange={(value) => {
          setState({ status: "loading" })
          setPage(value)
        }}
        emptyTitle={t.admin.noStockMovements}
        minWidthClassName="min-w-[72rem]"
      />
    </section>
  )
}
