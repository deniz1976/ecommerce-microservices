"use client"

import { type FormEvent, useState } from "react"
import { Loader2Icon, PencilIcon, XIcon } from "lucide-react"

import { FormField } from "@/components/patterns/form-field"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
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
    <form onSubmit={handleSubmit} className="rounded-xl border border-border bg-card p-5">
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <PencilIcon className="size-4 text-primary" />
          <h2 className="font-heading text-lg font-semibold text-foreground">
            {t.seller.editStore}
          </h2>
        </div>
        <Button type="button" variant="ghost" size="icon-sm" onClick={onCancel}>
          <XIcon />
          <span className="sr-only">{t.common.back}</span>
        </Button>
      </div>
      <div className="mt-5 grid gap-4">
        <FormField label={t.seller.storeName}>
          <Input
            value={name}
            onChange={(event) => setName(event.target.value)}
            minLength={2}
            maxLength={160}
            required
          />
        </FormField>
        <FormField label={t.seller.storeSlug} hint={t.seller.slugHint}>
          <Input
            value={slug}
            onChange={(event) => setSlug(event.target.value.toLowerCase())}
            pattern="[a-z0-9]+(?:-[a-z0-9]+)*"
            minLength={2}
            maxLength={160}
            required
          />
        </FormField>
      </div>
      {error ? <p className="mt-4 text-sm text-destructive" role="alert">{error}</p> : null}
      <Button type="submit" disabled={saving} className="mt-5">
        {saving ? <Loader2Icon className="animate-spin" /> : <PencilIcon />}
        {saving ? t.seller.updatingStore : t.seller.updateStore}
      </Button>
    </form>
  )
}
