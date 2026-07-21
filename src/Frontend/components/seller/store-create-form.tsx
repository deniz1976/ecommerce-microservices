"use client"

import { type FormEvent, useState } from "react"
import { Loader2, Plus } from "lucide-react"

import { Button } from "@/components/ui/button"
import { ApiError } from "@/lib/api/client"
import { createCatalogStore } from "@/lib/api/catalog"
import { useI18n } from "@/lib/i18n/provider"
import type { CatalogStore } from "@/types"

interface StoreCreateFormProps {
  onCreated: (store: CatalogStore) => void
}

export function StoreCreateForm({ onCreated }: StoreCreateFormProps) {
  const { t } = useI18n()
  const [name, setName] = useState("")
  const [slug, setSlug] = useState("")
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSaving(true)
    setError(null)

    try {
      const store = await createCatalogStore({ name: name.trim(), slug: slug.trim() })
      setName("")
      setSlug("")
      onCreated(store)
    } catch (requestError) {
      setError(
        requestError instanceof ApiError && requestError.status === 409
          ? t.seller.slugConflict
          : t.seller.storeCreateFailed,
      )
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-border bg-background p-5">
      <div className="flex items-center gap-2">
        <Plus className="size-4 text-primary" />
        <h2 className="font-heading text-lg font-semibold text-foreground">{t.seller.createStore}</h2>
      </div>
      <div className="mt-5 grid gap-4 sm:grid-cols-2">
        <label className="grid gap-2 text-sm font-medium text-foreground">
          {t.seller.storeName}
          <input
            value={name}
            onChange={(event) => setName(event.target.value)}
            placeholder={t.seller.storeNamePlaceholder}
            minLength={2}
            maxLength={160}
            required
            className="h-10 rounded-md border border-input bg-background px-3 font-normal outline-none focus:ring-2 focus:ring-ring/40"
          />
        </label>
        <label className="grid gap-2 text-sm font-medium text-foreground">
          {t.seller.storeSlug}
          <input
            value={slug}
            onChange={(event) => setSlug(event.target.value.toLowerCase())}
            placeholder={t.seller.storeSlugPlaceholder}
            pattern="[a-z0-9]+(?:-[a-z0-9]+)*"
            minLength={2}
            maxLength={160}
            required
            className="h-10 rounded-md border border-input bg-background px-3 font-normal outline-none focus:ring-2 focus:ring-ring/40"
          />
          <span className="text-xs font-normal text-muted-foreground">{t.seller.slugHint}</span>
        </label>
      </div>
      {error ? <p className="mt-4 text-sm text-destructive" role="alert">{error}</p> : null}
      <Button type="submit" disabled={saving} className="mt-5">
        {saving ? <Loader2 className="animate-spin" /> : <Plus />}
        {saving ? t.seller.savingStore : t.seller.saveStore}
      </Button>
    </form>
  )
}
