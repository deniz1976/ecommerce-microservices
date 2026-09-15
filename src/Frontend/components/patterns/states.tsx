"use client"

import { AlertTriangleIcon, InboxIcon } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { TableCell, TableRow } from "@/components/ui/table"
import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"

interface StateProps {
  title?: string
  description?: string
  className?: string
}

export function EmptyState({ title, description, className }: StateProps) {
  const { t } = useI18n()
  return (
    <div
      className={cn(
        "flex flex-col items-center justify-center gap-2 border border-dashed border-border bg-muted/20 px-6 py-12 text-center",
        className,
      )}
    >
      <InboxIcon className="size-6 text-muted-foreground" aria-hidden="true" />
      <p className="text-sm font-medium">{title ?? t.table.empty}</p>
      <p className="max-w-sm text-sm text-muted-foreground">
        {description ?? t.table.emptyDescription}
      </p>
    </div>
  )
}

interface ErrorStateProps extends StateProps {
  onRetry?: () => void
}

export function ErrorState({ title, description, onRetry, className }: ErrorStateProps) {
  const { t } = useI18n()
  return (
    <div
      role="alert"
      className={cn(
        "flex flex-col items-center justify-center gap-2 border border-destructive/30 bg-destructive/5 px-6 py-12 text-center",
        className,
      )}
    >
      <AlertTriangleIcon className="size-6 text-destructive" aria-hidden="true" />
      <p className="text-sm font-medium">{title ?? t.table.loadFailed}</p>
      <p className="max-w-sm text-sm text-muted-foreground">
        {description ?? t.table.loadFailedDescription}
      </p>
      {onRetry ? (
        <Button variant="outline" size="sm" onClick={onRetry} className="mt-2">
          {t.table.retry}
        </Button>
      ) : null}
    </div>
  )
}

export function TableSkeleton({ rows = 5, columns = 4 }: { rows?: number; columns?: number }) {
  return (
    <>
      {Array.from({ length: rows }, (_, rowIndex) => (
        <TableRow key={rowIndex}>
          {Array.from({ length: columns }, (_, columnIndex) => (
            <TableCell key={columnIndex}>
              <Skeleton className="h-4 w-full" />
            </TableCell>
          ))}
        </TableRow>
      ))}
    </>
  )
}
