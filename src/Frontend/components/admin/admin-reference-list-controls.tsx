"use client"

import { ArrowDown, ArrowUp, ArrowUpDown, ChevronLeft, ChevronRight, Search } from "lucide-react"

import { Button } from "@/components/ui/button"

export type ReferenceStatusFilter = "all" | "active" | "inactive"
export type ReferenceSortDirection = "ascending" | "descending"

export function ReferenceListToolbar({
  search,
  onSearchChange,
  searchLabel,
  searchPlaceholder,
  status,
  onStatusChange,
  statusLabel,
  allStatusesLabel,
  activeLabel,
  inactiveLabel,
  pageSize,
  onPageSizeChange,
  rowsPerPageLabel,
}: {
  search: string
  onSearchChange: (value: string) => void
  searchLabel: string
  searchPlaceholder: string
  status: ReferenceStatusFilter
  onStatusChange: (value: ReferenceStatusFilter) => void
  statusLabel: string
  allStatusesLabel: string
  activeLabel: string
  inactiveLabel: string
  pageSize: number
  onPageSizeChange: (value: number) => void
  rowsPerPageLabel: string
}) {
  return (
    <div className="grid gap-3 rounded-lg border border-border bg-background p-4 lg:grid-cols-[minmax(0,1fr)_12rem_10rem]">
      <label className="grid gap-2 text-sm font-medium text-foreground">
        {searchLabel}
        <span className="relative">
          <Search className="absolute top-1/2 left-3 size-4 -translate-y-1/2 text-muted-foreground" />
          <input
            value={search}
            onChange={(event) => onSearchChange(event.target.value)}
            placeholder={searchPlaceholder}
            className="h-10 w-full rounded-md border border-input bg-background pr-3 pl-9 font-normal"
          />
        </span>
      </label>
      <label className="grid gap-2 text-sm font-medium text-foreground">
        {statusLabel}
        <select
          value={status}
          onChange={(event) => onStatusChange(event.target.value as ReferenceStatusFilter)}
          className="h-10 rounded-md border border-input bg-background px-3 font-normal"
        >
          <option value="all">{allStatusesLabel}</option>
          <option value="active">{activeLabel}</option>
          <option value="inactive">{inactiveLabel}</option>
        </select>
      </label>
      <label className="grid gap-2 text-sm font-medium text-foreground">
        {rowsPerPageLabel}
        <select
          value={pageSize}
          onChange={(event) => onPageSizeChange(Number(event.target.value))}
          className="h-10 rounded-md border border-input bg-background px-3 font-normal"
        >
          <option value={10}>10</option>
          <option value={25}>25</option>
          <option value={50}>50</option>
        </select>
      </label>
    </div>
  )
}

export function ReferenceSortButton({
  label,
  active,
  direction,
  onClick,
}: {
  label: string
  active: boolean
  direction: ReferenceSortDirection
  onClick: () => void
}) {
  const Icon = !active
    ? ArrowUpDown
    : direction === "ascending"
      ? ArrowUp
      : ArrowDown

  return (
    <button
      type="button"
      onClick={onClick}
      className="inline-flex items-center gap-1.5 font-medium text-foreground hover:text-primary"
    >
      {label}
      <Icon className="size-3.5" />
    </button>
  )
}

export function ReferenceStatusBadge({
  isActive,
  activeLabel,
  inactiveLabel,
}: {
  isActive: boolean
  activeLabel: string
  inactiveLabel: string
}) {
  return (
    <span
      className={isActive
        ? "inline-flex rounded-md bg-primary/10 px-2 py-1 text-xs font-medium text-primary"
        : "inline-flex rounded-md bg-muted px-2 py-1 text-xs font-medium text-muted-foreground"}
    >
      {isActive ? activeLabel : inactiveLabel}
    </span>
  )
}

export function ReferencePagination({
  page,
  totalPages,
  totalCount,
  pageLabel,
  previousLabel,
  nextLabel,
  onPageChange,
}: {
  page: number
  totalPages: number
  totalCount: number
  pageLabel: string
  previousLabel: string
  nextLabel: string
  onPageChange: (page: number) => void
}) {
  return (
    <div className="flex items-center justify-between gap-3 border-t border-border px-4 py-3">
      <p className="text-xs text-muted-foreground">
        {pageLabel
          .replace("{page}", String(page))
          .replace("{total}", String(totalPages))
          .replace("{count}", String(totalCount))}
      </p>
      <div className="flex gap-2">
        <Button
          type="button"
          variant="outline"
          size="icon-sm"
          disabled={page <= 1}
          onClick={() => onPageChange(Math.max(1, page - 1))}
          aria-label={previousLabel}
        >
          <ChevronLeft />
        </Button>
        <Button
          type="button"
          variant="outline"
          size="icon-sm"
          disabled={page >= totalPages}
          onClick={() => onPageChange(Math.min(totalPages, page + 1))}
          aria-label={nextLabel}
        >
          <ChevronRight />
        </Button>
      </div>
    </div>
  )
}
