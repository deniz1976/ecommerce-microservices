"use client"

import { ChevronLeftIcon, ChevronRightIcon } from "lucide-react"
import type { ReactNode } from "react"

import { EmptyState, ErrorState, TableSkeleton } from "@/components/patterns/states"
import { Button } from "@/components/ui/button"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { useI18n } from "@/lib/i18n/provider"
import type { PagedResult } from "@/types"
import { cn } from "@/lib/utils"

export interface DataTableColumn<TRow> {
  /** Stable key, also used as the React key for the cell. */
  id: string
  header: string
  cell: (row: TRow) => ReactNode
  className?: string
  headerClassName?: string
}

interface DataTableProps<TRow> {
  columns: DataTableColumn<TRow>[]
  page: PagedResult<TRow> | null
  rowKey: (row: TRow) => string
  isLoading?: boolean
  error?: boolean
  onRetry?: () => void
  onPageChange: (pageNumber: number) => void
  rowClassName?: (row: TRow) => string | undefined
  emptyTitle?: string
  emptyDescription?: string
  minWidthClassName?: string
}

/**
 * Server-paged table bound to the `PagedResult<T>` contract every list endpoint
 * returns. Loading, empty and error states are handled here so pages do not
 * re-implement them.
 */
export function DataTable<TRow>({
  columns,
  page,
  rowKey,
  isLoading = false,
  error = false,
  onRetry,
  onPageChange,
  rowClassName,
  emptyTitle,
  emptyDescription,
  minWidthClassName = "min-w-[52rem]",
}: DataTableProps<TRow>) {
  const { t } = useI18n()

  if (error) {
    return <ErrorState onRetry={onRetry} />
  }

  if (!isLoading && page && page.items.length === 0) {
    return <EmptyState title={emptyTitle} description={emptyDescription} />
  }

  const pageNumber = page?.pageNumber ?? 1
  const totalPages = page?.totalPages ?? 1

  return (
    <div className="flex flex-col gap-3">
      <div className="overflow-x-auto rounded-xl border border-border bg-card">
        <Table className={minWidthClassName}>
          <TableHeader>
            <TableRow>
              {columns.map((column) => (
                <TableHead key={column.id} className={column.headerClassName}>
                  {column.header}
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              <TableSkeleton columns={columns.length} />
            ) : (
              page?.items.map((row) => (
                <TableRow key={rowKey(row)} className={rowClassName?.(row)}>
                  {columns.map((column) => (
                    <TableCell key={column.id} className={column.className}>
                      {column.cell(row)}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      <div className="flex flex-wrap items-center justify-between gap-3 text-sm text-muted-foreground">
        <span>
          {t.table.totalItems.replace("{count}", String(page?.totalCount ?? 0))}
        </span>
        <div className="flex items-center gap-2">
          <span className={cn(totalPages <= 1 && "text-muted-foreground/60")}>
            {t.table.pageStatus
              .replace("{page}", String(pageNumber))
              .replace("{totalPages}", String(totalPages))}
          </span>
          <Button
            variant="outline"
            size="icon-sm"
            aria-label={t.table.previousPage}
            disabled={isLoading || pageNumber <= 1}
            onClick={() => onPageChange(pageNumber - 1)}
          >
            <ChevronLeftIcon />
          </Button>
          <Button
            variant="outline"
            size="icon-sm"
            aria-label={t.table.nextPage}
            disabled={isLoading || pageNumber >= totalPages}
            onClick={() => onPageChange(pageNumber + 1)}
          >
            <ChevronRightIcon />
          </Button>
        </div>
      </div>
    </div>
  )
}
