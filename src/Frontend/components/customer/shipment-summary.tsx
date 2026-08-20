"use client"

import { Loader2, PackageX, Truck } from "lucide-react"
import { useEffect, useState } from "react"

import { ApiError } from "@/lib/api/client"
import { getOrderShipment } from "@/lib/api/shipping"
import type { Dictionary } from "@/lib/i18n/dictionaries"
import type { ShipmentSummary as Shipment } from "@/types"

type ShipmentState =
  | { status: "loading" }
  | { status: "ready"; shipment: Shipment }
  | { status: "unavailable" }

export function ShipmentSummary({
  orderId,
  locale,
  labels,
  retryWhenMissing,
}: {
  orderId: string
  locale: "en" | "tr"
  labels: Dictionary["orders"]["shipment"]
  retryWhenMissing: boolean
}) {
  const [state, setState] = useState<ShipmentState>({ status: "loading" })

  useEffect(() => {
    let timer: ReturnType<typeof setTimeout> | undefined
    let controller = new AbortController()

    async function loadShipment() {
      try {
        const shipment = await getOrderShipment(orderId, controller.signal)
        setState({ status: "ready", shipment })
      } catch (error) {
        if (error instanceof DOMException && error.name === "AbortError") return
        if (error instanceof ApiError && error.status === 404 && retryWhenMissing) {
          timer = setTimeout(() => {
            controller = new AbortController()
            loadShipment()
          }, 2500)
        } else {
          setState({ status: "unavailable" })
        }
      }
    }

    loadShipment()
    return () => {
      if (timer) clearTimeout(timer)
      controller.abort()
    }
  }, [orderId, retryWhenMissing])

  if (state.status === "loading") {
    return <section className="flex items-center gap-3 border-t border-border p-6 text-sm text-muted-foreground"><Loader2 className="size-4 animate-spin" />{labels.pending}</section>
  }

  if (state.status === "unavailable") {
    return <section className="border-t border-border p-6 text-sm text-muted-foreground">{labels.unavailable}</section>
  }

  const shipment = state.shipment
  const failed = shipment.status === 2
  return (
    <section className="border-t border-border p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex items-center gap-2 text-sm font-medium">
          {failed ? <PackageX className="size-4 text-destructive" /> : <Truck className="size-4 text-primary" />}
          {labels.title}
        </div>
        <span className={`rounded-full px-3 py-1 text-xs font-medium ${failed ? "bg-destructive/10 text-destructive" : "bg-primary/10 text-primary"}`}>
          {shipmentStatusLabel(shipment.status, labels)}
        </span>
      </div>
      <dl className="mt-4 grid gap-3 sm:grid-cols-2">
        <div><dt className="text-xs text-muted-foreground">{labels.trackingNumber}</dt><dd className="mt-1 font-mono text-sm text-foreground">{shipment.trackingNumber ?? labels.notAvailable}</dd></div>
        <div><dt className="text-xs text-muted-foreground">{labels.createdAt}</dt><dd className="mt-1 text-sm text-foreground">{formatDate(shipment.createdAt, locale)}</dd></div>
      </dl>
    </section>
  )
}

function shipmentStatusLabel(status: Shipment["status"], labels: Dictionary["orders"]["shipment"]): string {
  if (status === 2) return labels.failed
  if (status === 3) return labels.inTransit
  if (status === 4) return labels.delivered
  return labels.created
}

function formatDate(value: string, locale: "en" | "tr") {
  return new Intl.DateTimeFormat(locale === "tr" ? "tr-TR" : "en-US", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value))
}
