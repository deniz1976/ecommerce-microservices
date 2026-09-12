"use client"

import { CheckIcon, XIcon } from "lucide-react"

import { formatDateTime } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import {
  ORDER_TIMELINE_STEPS,
  orderStatusLabel,
  orderStatusTone,
} from "@/lib/i18n/status"
import type {
  OrderCancellationReasonCode,
  OrderStatus,
  OrderStatusHistory,
} from "@/types"
import { cn } from "@/lib/utils"
import { StatusBadge } from "@/components/patterns/status-badge"

interface OrderTimelineProps {
  status: OrderStatus
  history: OrderStatusHistory[]
  cancellationReasonLabel?: string
  cancellationReasons?: Record<OrderCancellationReasonCode, string>
  className?: string
}

export function OrderTimeline({
  status,
  history,
  cancellationReasonLabel,
  cancellationReasons,
  className,
}: OrderTimelineProps) {
  const { t, locale } = useI18n()

  const occurredAtByStatus = new Map<OrderStatus, string>()
  for (const entry of history) {
    if (!occurredAtByStatus.has(entry.status)) {
      occurredAtByStatus.set(entry.status, entry.occurredAt)
    }
  }

  const isCancelled = status === 5
  const cancellationRequested = status === 6
  const reachedIndex = ORDER_TIMELINE_STEPS.indexOf(status)
  const cancellationReason = history.find((entry) => entry.reasonCode !== null)?.reasonCode ?? null

  return (
    <div className={cn("flex flex-col gap-4", className)}>
      {isCancelled || cancellationRequested ? (
        <StatusBadge label={orderStatusLabel(status, t.orders.status)} tone={orderStatusTone(status)} />
      ) : null}

      <ol className="grid gap-4 sm:grid-cols-5">
        {ORDER_TIMELINE_STEPS.map((step, index) => {
          const occurredAt = occurredAtByStatus.get(step)
          const isDone = occurredAt !== undefined || (reachedIndex >= 0 && index <= reachedIndex)
          const isCurrent = reachedIndex === index
          const isBlocked = isCancelled && !isDone

          return (
            <li key={step} className="flex flex-col gap-2">
              <div className="flex items-center gap-2">
                <span
                  className={cn(
                    "flex size-6 shrink-0 items-center justify-center rounded-full border text-xs font-medium",
                    isBlocked && "border-destructive/40 text-destructive",
                    !isBlocked && isDone && "border-transparent bg-chart-1 text-background",
                    !isBlocked && !isDone && "border-border text-muted-foreground",
                  )}
                  aria-hidden="true"
                >
                  {isBlocked ? (
                    <XIcon className="size-3" />
                  ) : isDone ? (
                    <CheckIcon className="size-3" />
                  ) : (
                    index + 1
                  )}
                </span>
                <span
                  className={cn(
                    "h-px flex-1",
                    index === ORDER_TIMELINE_STEPS.length - 1 && "hidden sm:block sm:opacity-0",
                    isDone ? "bg-chart-1/40" : "bg-border",
                  )}
                  aria-hidden="true"
                />
              </div>
              <div className="flex flex-col gap-0.5">
                <span
                  className={cn(
                    "text-sm font-medium",
                    isCurrent ? "text-foreground" : "text-muted-foreground",
                  )}
                >
                  {orderStatusLabel(step, t.orders.status)}
                </span>
                <span className="text-xs text-muted-foreground tabular-nums">
                  {formatDateTime(occurredAt ?? null, locale)}
                </span>
              </div>
            </li>
          )
        })}
      </ol>

      {cancellationReason && cancellationReasonLabel && cancellationReasons ? (
        <p className="rounded-lg bg-destructive/10 px-3 py-2 text-sm text-destructive">
          <span className="font-medium">{cancellationReasonLabel}:</span>{" "}
          {cancellationReasons[cancellationReason]}
        </p>
      ) : null}
    </div>
  )
}
