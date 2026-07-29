"use client"

import { useEffect, useState } from "react"
import {
  Archive,
  Badge,
  Boxes,
  CheckCircle2,
  ClipboardList,
  Loader2,
  PauseCircle,
  Store,
  Tags,
} from "lucide-react"

import { getCatalogMetrics } from "@/lib/api/catalog"
import { useI18n } from "@/lib/i18n/provider"
import type { CatalogMetrics } from "@/types"

type MetricsState =
  | { status: "loading" }
  | { status: "ready"; data: CatalogMetrics }
  | { status: "unavailable" }

export function AdminOverviewMetrics() {
  const { t } = useI18n()
  const [metrics, setMetrics] = useState<MetricsState>({ status: "loading" })

  useEffect(() => {
    let active = true

    getCatalogMetrics()
      .then((data) => {
        if (active) setMetrics({ status: "ready", data })
      })
      .catch(() => {
        if (active) setMetrics({ status: "unavailable" })
      })

    return () => {
      active = false
    }
  }, [])

  if (metrics.status === "loading") {
    return (
      <section className="mt-7 flex min-h-32 items-center justify-center rounded-lg border border-border bg-background">
        <Loader2 className="size-5 animate-spin text-muted-foreground" />
        <span className="sr-only">{t.common.loading}</span>
      </section>
    )
  }

  if (metrics.status === "unavailable") {
    return (
      <section className="mt-7 rounded-lg border border-border bg-background p-5 text-sm text-muted-foreground">
        {t.admin.metricsUnavailable}
      </section>
    )
  }

  const data = metrics.data

  return (
    <section className="mt-7" aria-labelledby="admin-overview-title">
      <div>
        <h2 id="admin-overview-title" className="font-heading text-xl font-semibold text-foreground">
          {t.admin.catalogSummary}
        </h2>
        <p className="mt-1 text-sm text-muted-foreground">{t.admin.catalogSummaryDescription}</p>
      </div>
      <div className="mt-5 grid gap-px overflow-hidden rounded-lg border border-border bg-border sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard icon={Boxes} label={t.admin.totalProducts} value={data.totalProducts} />
        <MetricCard icon={CheckCircle2} label={t.admin.activeProducts} value={data.activeProducts} />
        <MetricCard icon={ClipboardList} label={t.admin.draftProducts} value={data.draftProducts} />
        <MetricCard icon={PauseCircle} label={t.admin.inactiveProducts} value={data.inactiveProducts} />
        <MetricCard icon={Archive} label={t.admin.archivedProducts} value={data.archivedProducts} />
        <MetricCard icon={Store} label={t.admin.totalStores} value={data.totalStores} />
        <MetricCard icon={Tags} label={t.admin.totalCategories} value={data.totalCategories} />
        <MetricCard icon={Badge} label={t.admin.totalBrands} value={data.totalBrands} />
      </div>
    </section>
  )
}

function MetricCard({
  icon: Icon,
  label,
  value,
}: {
  icon: typeof Boxes
  label: string
  value: number
}) {
  return (
    <div className="bg-background p-5">
      <Icon className="size-4 text-primary" />
      <p className="mt-5 text-2xl font-semibold text-foreground">{value.toLocaleString()}</p>
      <p className="mt-1 text-sm text-muted-foreground">{label}</p>
    </div>
  )
}
