"use client"

import { ChevronLeft, ChevronRight } from "lucide-react"

import { Button } from "@/components/ui/button"

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
