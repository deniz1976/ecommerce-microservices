"use client"

import { type FormEvent, useState } from "react"
import { Loader2, Pencil, X } from "lucide-react"

import { Button } from "@/components/ui/button"
import { ApiError } from "@/lib/api/client"
import { updateCatalogStore } from "@/lib/api/catalog"
import { useI18n } from "@/lib/i18n/provider"
import type { CatalogStore } from "@/types"

interface StoreEditFormProps {
  store: CatalogStore
  onCancel: () => void
  onUpdated: (store: CatalogStore) => void
}

export function StoreEditForm({ store, onCancel, onUpdated }: StoreEditFormProps) {
  const { t } = useI18n()
  const [name, setName] = useState(store.name)
  const [slug, setSlug] = useState(store.slug)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSaving(true)
    setError(null)

    try {
      const updated = await updateCatalogStore(store.id, {
        name: name.trim(),
        slug: slug.trim(),
      })
      onUpdated(updated)
    } catch (requestError) {
      setError(
        requestError instanceof ApiError && requestError.status === 409
          ? t.seller.slugConflict
          : t.seller.storeUpdateFailed,
      )
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-border bg-background p-5">
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <Pencil className="size-4 text-primary" />
          <h2 className="font-heading text-lg font-semibold text-foreground">
            {t.seller.editStore}
          </h2>
        </div>
        <Button type="button" variant="ghost" size="icon-sm" onClick={onCancel}>
          <X />
          <span className="sr-only">{t.common.back}</span>
        </Button>
      </div>
      <div className="mt-5 grid gap-4">
        <label className="grid min-w-0 gap-2 text-sm font-medium text-foreground">
          {t.seller.storeName}
          <input
            value={name}
            onChange={(event) => setName(event.target.value)}
            minLength={2}
            maxLength={160}
            required
            className="h-10 w-full min-w-0 rounded-md border border-input bg-background px-3 font-normal outline-none focus:ring-2 focus:ring-ring/40"
          />
        </label>
        <label className="grid min-w-0 gap-2 text-sm font-medium text-foreground">
          {t.seller.storeSlug}
          <input
            value={slug}
            onChange={(event) => setSlug(event.target.value.toLowerCase())}
            pattern="[a-z0-9]+(?:-[a-z0-9]+)*"
            minLength={2}
            maxLength={160}
            required
            className="h-10 w-full min-w-0 rounded-md border border-input bg-background px-3 font-normal outline-none focus:ring-2 focus:ring-ring/40"
          />
          <span className="text-xs font-normal text-muted-foreground">{t.seller.slugHint}</span>
        </label>
      </div>
      {error ? <p className="mt-4 text-sm text-destructive" role="alert">{error}</p> : null}
      <Button type="submit" disabled={saving} className="mt-5">
        {saving ? <Loader2 className="animate-spin" /> : <Pencil />}
        {saving ? t.seller.updatingStore : t.seller.updateStore}
      </Button>
    </form>
  )
}
