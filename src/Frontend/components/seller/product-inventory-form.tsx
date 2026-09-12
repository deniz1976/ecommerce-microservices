"use client"

import { useEffect, useState } from "react"
import { Loader2Icon, XIcon } from "lucide-react"

import { FormField } from "@/components/patterns/form-field"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Skeleton } from "@/components/ui/skeleton"
import { ApiError } from "@/lib/api/client"
import { getInventoryItem, updateInventoryItem } from "@/lib/api/inventory"
import { useI18n } from "@/lib/i18n/provider"
import type { CatalogProduct, InventoryItem } from "@/types"

interface ProductInventoryFormProps {
  product: CatalogProduct
  onCancel: () => void
}

export function ProductInventoryForm({
  product,
  onCancel,
}: ProductInventoryFormProps) {
  const { t } = useI18n()
  const [inventory, setInventory] = useState<InventoryItem | null>(null)
  const [quantity, setQuantity] = useState("0")
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [isNew, setIsNew] = useState(false)
  const [message, setMessage] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    getInventoryItem(product.id)
      .then((item) => {
        if (!active) return
        setInventory(item)
        setQuantity(String(item.quantityOnHand))
      })
      .catch((error: unknown) => {
        if (!active) return
        if (error instanceof ApiError && error.status === 404) {
          setIsNew(true)
          setQuantity("0")
          return
        }

        setMessage(t.seller.stockLoadFailed)
      })
      .finally(() => {
        if (active) setLoading(false)
      })

    return () => {
      active = false
    }
  }, [product.id, t.seller.stockLoadFailed])

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const quantityOnHand = Number(quantity)
    if (!Number.isInteger(quantityOnHand) || quantityOnHand < 0) {
      setMessage(t.seller.stockInvalid)
      return
    }

    setSaving(true)
    setMessage(null)
    try {
      const updated = await updateInventoryItem(product.id, quantityOnHand)
      setInventory(updated)
      setQuantity(String(updated.quantityOnHand))
      setIsNew(false)
      setMessage(t.seller.stockUpdated)
    } catch {
      setMessage(t.seller.stockUpdateFailed)
    } finally {
      setSaving(false)
    }
  }

  return (
    <section className="rounded-xl border border-border bg-card p-5">
      <div className="flex items-start justify-between gap-3">
        <div>
          <h2 className="font-heading text-lg font-semibold text-foreground">{t.seller.stockTitle}</h2>
          <p className="mt-1 text-sm text-muted-foreground">{product.name}</p>
        </div>
        <Button type="button" variant="ghost" size="icon" onClick={onCancel} aria-label={t.seller.closeStockEditor}>
          <XIcon />
        </Button>
      </div>

      {loading ? (
        <div className="mt-5 flex flex-col gap-4">
          <Skeleton className="h-14 w-full" />
          <Skeleton className="h-16 w-full" />
          <Skeleton className="h-9 w-full" />
        </div>
      ) : (
        <form className="mt-5 space-y-4" onSubmit={handleSubmit}>
          {isNew ? <p className="text-sm text-muted-foreground">{t.seller.stockNotCreated}</p> : null}
          <FormField label={t.seller.quantityOnHand}>
            <Input
              type="number"
              min="0"
              step="1"
              required
              value={quantity}
              onChange={(event) => setQuantity(event.target.value)}
            />
          </FormField>
          {inventory ? (
            <dl className="grid grid-cols-2 gap-3 rounded-lg bg-muted/50 p-3 text-sm">
              <div className="flex flex-col gap-1">
                <dt className="text-muted-foreground">{t.seller.reservedQuantity}</dt>
                <dd className="font-medium tabular-nums">{inventory.reservedQuantity}</dd>
              </div>
              <div className="flex flex-col gap-1">
                <dt className="text-muted-foreground">{t.seller.availableQuantity}</dt>
                <dd className="font-medium tabular-nums">{inventory.availableQuantity}</dd>
              </div>
            </dl>
          ) : null}
          {message ? <p className="text-sm text-muted-foreground">{message}</p> : null}
          <Button className="w-full" type="submit" disabled={saving}>
            {saving ? <Loader2Icon className="animate-spin" /> : null}
            {saving ? t.seller.savingStock : t.seller.saveStock}
          </Button>
        </form>
      )}
    </section>
  )
}
