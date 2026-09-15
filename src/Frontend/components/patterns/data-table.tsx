"use client"

import {
  ChevronDownIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ChevronUpIcon,
  ChevronsUpDownIcon,
} from "lucide-react"
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

export type DataTableSortDirection = "ascending" | "descending"

export interface DataTableSort<TSortKey extends string = string> {
  key: TSortKey
  direction: DataTableSortDirection
  onChange: (key: TSortKey) => void
}

export interface DataTableColumn<TRow, TSortKey extends string = string> {
  id: string
  header: string
  cell: (row: TRow) => ReactNode
  className?: string
  headerClassName?: string
  sortKey?: TSortKey
}

interface DataTableProps<TRow, TSortKey extends string = string> {
  columns: DataTableColumn<TRow, TSortKey>[]
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
  sort?: DataTableSort<TSortKey>
}

export function DataTable<TRow, TSortKey extends string = string>({
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
  sort,
}: DataTableProps<TRow, TSortKey>) {
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
      <div className="overflow-x-auto border border-border bg-card">
        <Table className={minWidthClassName}>
          <TableHeader>
            <TableRow>
              {columns.map((column) => {
                const sortable = sort !== undefined && column.sortKey !== undefined
                const isSorted = sortable && sort.key === column.sortKey
                const SortIcon = !isSorted
                  ? ChevronsUpDownIcon
                  : sort.direction === "ascending"
                    ? ChevronUpIcon
                    : ChevronDownIcon

                return (
                  <TableHead
                    key={column.id}
                    className={column.headerClassName}
                    aria-sort={isSorted ? sort.direction : undefined}
                  >
                    {sortable ? (
                      <button
                        type="button"
                        onClick={() => sort.onChange(column.sortKey as TSortKey)}
                        className="inline-flex items-center gap-1.5 font-medium hover:text-foreground"
                      >
                        {column.header}
                        <SortIcon className="size-3.5" />
                      </button>
                    ) : (
                      column.header
                    )}
                  </TableHead>
                )
              })}
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
