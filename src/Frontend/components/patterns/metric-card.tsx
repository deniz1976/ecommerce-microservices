import type { ReactNode } from "react"

import { Card, CardContent } from "@/components/ui/card"
import type { StatusTone } from "@/lib/i18n/status"
import { cn } from "@/lib/utils"

const accentByTone: Record<StatusTone, string> = {
  neutral: "bg-muted-foreground",
  progress: "bg-chart-2",
  success: "bg-chart-1",
  warning: "bg-highlight",
  danger: "bg-destructive",
}

interface MetricCardProps {
  label: string
  value: ReactNode
  hint?: string
  tone?: StatusTone
  icon?: ReactNode
  className?: string
}

export function MetricCard({
  label,
  value,
  hint,
  tone = "neutral",
  icon,
  className,
}: MetricCardProps) {
  return (
    <Card className={cn("relative overflow-hidden", className)}>
      <span
        className={cn("absolute inset-y-0 left-0 w-0.5", accentByTone[tone])}
        aria-hidden="true"
      />
      <CardContent className="flex items-start justify-between gap-4">
        <div className="flex flex-col gap-1">
          <span className="text-xs font-medium tracking-wide text-muted-foreground uppercase">
            {label}
          </span>
          <span className="font-heading text-2xl leading-none font-semibold tabular-nums">
            {value}
          </span>
          {hint ? <span className="text-xs text-muted-foreground">{hint}</span> : null}
        </div>
        {icon ? <span className="text-muted-foreground">{icon}</span> : null}
      </CardContent>
    </Card>
  )
}
