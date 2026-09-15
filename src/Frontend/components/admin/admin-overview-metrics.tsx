"use client"

import Link from "next/link"
import { useEffect, useState } from "react"
import {
  ArchiveIcon,
  BookmarkIcon,
  BoxesIcon,
  CheckCircle2Icon,
  ClipboardListIcon,
  PauseCircleIcon,
  StoreIcon,
  TagsIcon,
  TriangleAlertIcon,
} from "lucide-react"

import { MetricCard } from "@/components/patterns/metric-card"
import { ErrorState } from "@/components/patterns/states"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"
import { getCatalogMetrics } from "@/lib/api/catalog"
import { getWorkflowDiagnostics } from "@/lib/api/workflows"
import { formatNumber } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import type { CatalogMetrics } from "@/types"

type MetricsState =
  | { status: "loading" }
  | { status: "ready"; data: CatalogMetrics }
  | { status: "unavailable" }

export function AdminOverviewMetrics() {
  const { locale, t } = useI18n()
  const [metrics, setMetrics] = useState<MetricsState>({ status: "loading" })
  const [overdueCount, setOverdueCount] = useState(0)
  const [reloadToken, setReloadToken] = useState(0)

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
  }, [reloadToken])

  useEffect(() => {
    let active = true

    getWorkflowDiagnostics({ pageNumber: 1, pageSize: 1, overdueOnly: true })
      .then((data) => {
        if (active) setOverdueCount(data.totalCount)
      })
      .catch(() => {
        if (active) setOverdueCount(0)
      })

    return () => {
      active = false
    }
  }, [reloadToken])

  return (
    <section className="mt-7 flex flex-col gap-5" aria-labelledby="admin-overview-title">
      <div>
        <h2 id="admin-overview-title" className="font-heading text-xl font-semibold">
          {t.admin.catalogSummary}
        </h2>
        <p className="mt-1 text-sm text-muted-foreground">{t.admin.catalogSummaryDescription}</p>
      </div>

      {overdueCount > 0 ? (
        <Card className="border-destructive/30 bg-destructive/5">
          <CardContent className="flex flex-wrap items-center justify-between gap-3">
            <div className="flex items-start gap-3">
              <TriangleAlertIcon className="mt-0.5 size-4 shrink-0 text-destructive" aria-hidden="true" />
              <div className="flex flex-col gap-0.5">
                <span className="text-sm font-medium">{t.admin.overdueWorkflowsTitle}</span>
                <span className="text-sm text-muted-foreground">
                  {t.admin.overdueWorkflowsBody.replace("{count}", formatNumber(overdueCount, locale))}
                </span>
              </div>
            </div>
            <Button variant="outline" size="sm" render={<Link href="/admin/workflows" />}>
              {t.admin.reviewOverdueWorkflows}
            </Button>
          </CardContent>
        </Card>
      ) : null}

      {metrics.status === "unavailable" ? (
        <ErrorState
          title={t.admin.metricsUnavailable}
          onRetry={() => {
            setMetrics({ status: "loading" })
            setReloadToken((token) => token + 1)
          }}
        />
      ) : (
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
          {metrics.status === "loading"
            ? Array.from({ length: 8 }, (_, index) => (
                <Skeleton key={index} className="h-[5.75rem]" />
              ))
            : metricItems(metrics.data).map((item) => (
                <MetricCard
                  key={item.id}
                  label={labelFor(item.id, t)}
                  value={formatNumber(item.value, locale)}
                  tone={item.tone}
                  icon={item.icon}
                />
              ))}
        </div>
      )}
    </section>
  )
}

type MetricId =
  | "totalProducts"
  | "activeProducts"
  | "draftProducts"
  | "inactiveProducts"
  | "archivedProducts"
  | "totalStores"
  | "totalCategories"
  | "totalBrands"

function metricItems(data: CatalogMetrics) {
  return [
    { id: "totalProducts" as const, value: data.totalProducts, tone: "neutral" as const, icon: <BoxesIcon className="size-4" /> },
    { id: "activeProducts" as const, value: data.activeProducts, tone: "success" as const, icon: <CheckCircle2Icon className="size-4" /> },
    { id: "draftProducts" as const, value: data.draftProducts, tone: "progress" as const, icon: <ClipboardListIcon className="size-4" /> },
    { id: "inactiveProducts" as const, value: data.inactiveProducts, tone: "warning" as const, icon: <PauseCircleIcon className="size-4" /> },
    { id: "archivedProducts" as const, value: data.archivedProducts, tone: "danger" as const, icon: <ArchiveIcon className="size-4" /> },
    { id: "totalStores" as const, value: data.totalStores, tone: "neutral" as const, icon: <StoreIcon className="size-4" /> },
    { id: "totalCategories" as const, value: data.totalCategories, tone: "neutral" as const, icon: <TagsIcon className="size-4" /> },
    { id: "totalBrands" as const, value: data.totalBrands, tone: "neutral" as const, icon: <BookmarkIcon className="size-4" /> },
  ]
}

function labelFor(id: MetricId, t: ReturnType<typeof useI18n>["t"]): string {
  return t.admin[id]
}
