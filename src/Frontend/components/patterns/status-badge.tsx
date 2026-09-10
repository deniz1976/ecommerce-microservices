import { cva, type VariantProps } from "class-variance-authority"

import type { StatusTone } from "@/lib/i18n/status"
import { cn } from "@/lib/utils"

/**
 * Colour is carried by the dot rather than the label so contrast stays safe in
 * both themes regardless of which token a tone maps to.
 */
const dotVariants = cva("size-1.5 shrink-0 rounded-full", {
  variants: {
    tone: {
      neutral: "bg-muted-foreground",
      progress: "bg-chart-2",
      success: "bg-chart-1",
      warning: "bg-highlight",
      danger: "bg-destructive",
    },
  },
  defaultVariants: {
    tone: "neutral",
  },
})

interface StatusBadgeProps extends VariantProps<typeof dotVariants> {
  label: string
  tone: StatusTone
  className?: string
}

export function StatusBadge({ label, tone, className }: StatusBadgeProps) {
  return (
    <span
      data-slot="status-badge"
      className={cn(
        "inline-flex h-6 w-fit items-center gap-1.5 rounded-4xl border border-border/60 bg-muted/50 px-2 text-xs font-medium whitespace-nowrap text-foreground",
        className,
      )}
    >
      <span className={cn(dotVariants({ tone }))} aria-hidden="true" />
      {label}
    </span>
  )
}
