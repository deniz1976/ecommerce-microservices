"use client"

import { useEffect, useState } from "react"
import { Loader2, X } from "lucide-react"

import { Button } from "@/components/ui/button"
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
    <section className="rounded-lg border border-border bg-background p-5">
      <div className="flex items-start justify-between gap-3">
        <div>
          <h2 className="font-heading text-lg font-semibold text-foreground">{t.seller.stockTitle}</h2>
          <p className="mt-1 text-sm text-muted-foreground">{product.name}</p>
        </div>
        <Button type="button" variant="ghost" size="icon" onClick={onCancel} aria-label={t.seller.closeStockEditor}>
          <X />
        </Button>
      </div>

      {loading ? (
        <div className="flex min-h-32 items-center justify-center">
          <Loader2 className="size-5 animate-spin text-muted-foreground" />
          <span className="sr-only">{t.common.loading}</span>
        </div>
      ) : (
        <form className="mt-5 space-y-4" onSubmit={handleSubmit}>
          {isNew ? <p className="text-sm text-muted-foreground">{t.seller.stockNotCreated}</p> : null}
          <label className="block text-sm font-medium text-foreground">
            {t.seller.quantityOnHand}
            <input
              className="mt-2 h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
              type="number"
              min="0"
              step="1"
              required
              value={quantity}
              onChange={(event) => setQuantity(event.target.value)}
            />
          </label>
          {inventory ? (
            <dl className="grid grid-cols-2 gap-3 rounded-md bg-muted/50 p-3 text-sm">
              <div>
                <dt className="text-muted-foreground">{t.seller.reservedQuantity}</dt>
                <dd className="mt-1 font-medium text-foreground">{inventory.reservedQuantity}</dd>
              </div>
              <div>
                <dt className="text-muted-foreground">{t.seller.availableQuantity}</dt>
                <dd className="mt-1 font-medium text-foreground">{inventory.availableQuantity}</dd>
              </div>
            </dl>
          ) : null}
          {message ? <p className="text-sm text-muted-foreground">{message}</p> : null}
          <Button className="w-full" type="submit" disabled={saving}>
            {saving ? <Loader2 className="animate-spin" /> : null}
            {saving ? t.seller.savingStock : t.seller.saveStock}
          </Button>
        </form>
      )}
    </section>
  )
}
