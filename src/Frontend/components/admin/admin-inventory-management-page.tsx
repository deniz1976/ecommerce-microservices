"use client"

import { SearchIcon } from "lucide-react"
import { useEffect, useState } from "react"

import { AdminShell } from "@/components/admin/admin-shell"
import { StockMovementPanel } from "@/components/admin/stock-movement-panel"
import { DataTable, type DataTableColumn } from "@/components/patterns/data-table"
import { FilterBar, FilterField } from "@/components/patterns/filter-bar"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { SelectNative } from "@/components/ui/select-native"
import { isOptionalGuid } from "@/lib/admin/filters"
import { getManagedInventory, type ManagedInventoryQuery } from "@/lib/api/inventory"
import { formatDateTime } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import type { InventoryItem, PagedResult } from "@/types"

type InventoryState =
  | { status: "loading" }
  | { status: "ready"; data: PagedResult<InventoryItem> }
  | { status: "unavailable" }

export function AdminInventoryManagementPage() {
  const { locale, t } = useI18n()
  const [inventory, setInventory] = useState<InventoryState>({ status: "loading" })
  const [productInput, setProductInput] = useState("")
  const [maximumInput, setMaximumInput] = useState("")
  const [productId, setProductId] = useState("")
  const [maximumAvailableQuantity, setMaximumAvailableQuantity] = useState<number | undefined>()
  const [sortBy, setSortBy] = useState<NonNullable<ManagedInventoryQuery["sortBy"]>>("updatedAt")
  const [sortDescending, setSortDescending] = useState(true)
  const [pageSize, setPageSize] = useState(20)
  const [page, setPage] = useState(1)
  const [reloadToken, setReloadToken] = useState(0)
  const [movementProductId, setMovementProductId] = useState<string | null>(null)

  const normalizedProduct = productInput.trim()
  const productValid = isOptionalGuid(normalizedProduct)
  const normalizedMaximum = maximumInput.trim()
  const maximumValid = normalizedMaximum === "" || /^\d+$/.test(normalizedMaximum)

  useEffect(() => {
    const controller = new AbortController()
    getManagedInventory(
      {
        productId: productId || undefined,
        maximumAvailableQuantity,
        pageNumber: page,
        pageSize,
        sortBy,
        sortDescending,
      },
      controller.signal,
    )
      .then((data) => setInventory({ status: "ready", data }))
      .catch((error: unknown) => {
        if (!(error instanceof DOMException && error.name === "AbortError")) {
          setInventory({ status: "unavailable" })
        }
      })
    return () => controller.abort()
  }, [maximumAvailableQuantity, page, pageSize, productId, reloadToken, sortBy, sortDescending])

  const updateQuery = (update: () => void) => {
    setInventory({ status: "loading" })
    update()
    setPage(1)
  }

  const applyFilters = () => {
    if (!productValid || !maximumValid) return
    updateQuery(() => {
      setProductId(normalizedProduct)
      setMaximumAvailableQuantity(normalizedMaximum === "" ? undefined : Number(normalizedMaximum))
    })
  }

  const clearFilters = () => {
    setProductInput("")
    setMaximumInput("")
    updateQuery(() => {
      setProductId("")
      setMaximumAvailableQuantity(undefined)
    })
  }

  const columns: DataTableColumn<InventoryItem>[] = [
    {
      id: "productId",
      header: t.admin.inventoryProductId,
      cell: (row) => <span className="font-mono text-xs">{row.productId}</span>,
    },
    {
      id: "quantityOnHand",
      header: t.admin.quantityOnHand,
      headerClassName: "text-right",
      className: "text-right tabular-nums",
      cell: (row) => row.quantityOnHand,
    },
    {
      id: "reservedQuantity",
      header: t.admin.reservedQuantity,
      headerClassName: "text-right",
      className: "text-right text-muted-foreground tabular-nums",
      cell: (row) => row.reservedQuantity,
    },
    {
      id: "availableQuantity",
      header: t.admin.availableQuantity,
      headerClassName: "text-right",
      className: "text-right font-medium tabular-nums",
      cell: (row) => row.availableQuantity,
    },
    {
      id: "updatedAt",
      header: t.admin.inventoryUpdatedAt,
      className: "text-muted-foreground tabular-nums",
      cell: (row) => formatDateTime(row.updatedAt, locale),
    },
    {
      id: "actions",
      header: "",
      className: "text-right",
      cell: (row) => (
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => setMovementProductId(row.productId)}
        >
          {t.admin.viewStockMovements}
        </Button>
      ),
    },
  ]

  return (
    <AdminShell section="inventory" title={t.admin.inventory} description={t.admin.manageInventoryDescription}>
      <section className="mt-6 flex flex-col gap-4" aria-labelledby="admin-inventory-list-title">
        <h2 id="admin-inventory-list-title" className="font-heading text-xl font-semibold">
          {t.admin.inventoryList}
        </h2>

        <FilterBar
          onClear={clearFilters}
          hasActiveFilters={productId !== "" || maximumAvailableQuantity !== undefined}
        >
          <FilterField label={t.admin.inventoryProductId} className="min-w-72 flex-1">
            <Input
              value={productInput}
              onChange={(event) => setProductInput(event.target.value)}
              placeholder={t.admin.inventoryProductIdPlaceholder}
              aria-invalid={!productValid}
              className="font-mono text-xs"
            />
            {!productValid ? (
              <span className="text-xs text-destructive">{t.admin.invalidProductId}</span>
            ) : null}
          </FilterField>

          <FilterField label={t.admin.maximumAvailable}>
            <Input
              inputMode="numeric"
              value={maximumInput}
              onChange={(event) => setMaximumInput(event.target.value)}
              placeholder={t.admin.maximumAvailablePlaceholder}
              aria-invalid={!maximumValid}
            />
            {!maximumValid ? (
              <span className="text-xs text-destructive">{t.admin.invalidMaximumAvailable}</span>
            ) : null}
          </FilterField>

          <FilterField label={t.admin.inventorySort}>
            <SelectNative
              value={sortBy}
              onChange={(event) =>
                updateQuery(() =>
                  setSortBy(event.target.value as NonNullable<ManagedInventoryQuery["sortBy"]>),
                )
              }
            >
              <option value="updatedAt">{t.admin.inventoryUpdatedAt}</option>
              <option value="availableQuantity">{t.admin.availableQuantity}</option>
              <option value="quantityOnHand">{t.admin.quantityOnHand}</option>
              <option value="reservedQuantity">{t.admin.reservedQuantity}</option>
            </SelectNative>
          </FilterField>

          <FilterField label={t.admin.inventoryDirection}>
            <SelectNative
              value={sortDescending ? "descending" : "ascending"}
              onChange={(event) =>
                updateQuery(() => setSortDescending(event.target.value === "descending"))
              }
            >
              <option value="descending">{t.admin.descending}</option>
              <option value="ascending">{t.admin.ascending}</option>
            </SelectNative>
          </FilterField>

          <FilterField label={t.table.rowsPerPage} className="min-w-24">
            <SelectNative
              value={pageSize}
              onChange={(event) => updateQuery(() => setPageSize(Number(event.target.value)))}
            >
              {[10, 20, 50].map((value) => (
                <option key={value} value={value}>
                  {value}
                </option>
              ))}
            </SelectNative>
          </FilterField>

          <Button
            type="button"
            className="self-end"
            onClick={applyFilters}
            disabled={!productValid || !maximumValid}
            aria-label={t.admin.applyInventoryFilters}
          >
            <SearchIcon />
            {t.admin.applyInventoryFilters}
          </Button>
        </FilterBar>

        <DataTable
          columns={columns}
          page={inventory.status === "ready" ? inventory.data : null}
          rowKey={(row) => row.productId}
          isLoading={inventory.status === "loading"}
          error={inventory.status === "unavailable"}
          onRetry={() => {
            setInventory({ status: "loading" })
            setReloadToken((token) => token + 1)
          }}
          onPageChange={(value) => {
            setInventory({ status: "loading" })
            setPage(value)
          }}
          emptyTitle={t.admin.noMatchingInventory}
          minWidthClassName="min-w-[55rem]"
        />

        {movementProductId ? (
          <StockMovementPanel
            productId={movementProductId}
            onClose={() => setMovementProductId(null)}
          />
        ) : null}
      </section>
    </AdminShell>
  )
}
