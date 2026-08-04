"use client"

import { type FormEvent, useState } from "react"
import { Loader2, Save, Tags, X } from "lucide-react"

import {
  ReferenceActiveField,
  ReferenceTextField,
  resolveReferenceApiMessage,
} from "@/components/admin/admin-reference-fields"
import { Button } from "@/components/ui/button"
import { createCatalogBrand, updateCatalogBrand } from "@/lib/api/catalog"
import { useI18n } from "@/lib/i18n/provider"
import type { ManagedCatalogBrandReference } from "@/types"

export function AdminBrandForm({
  brand,
  onSaved,
  onCancel,
}: {
  brand?: ManagedCatalogBrandReference
  onSaved: (brand: ManagedCatalogBrandReference) => void
  onCancel: () => void
}) {
  const { t } = useI18n()
  const [name, setName] = useState(brand?.name ?? "")
  const [slug, setSlug] = useState(brand?.slug ?? "")
  const [isActive, setIsActive] = useState(brand?.isActive ?? true)
  const [saving, setSaving] = useState(false)
  const [message, setMessage] = useState<string | null>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSaving(true)
    setMessage(null)

    const payload = {
      name: name.trim(),
      slug: slug.trim(),
    }

    try {
      if (brand) {
        const updated = await updateCatalogBrand(brand.id, {
          ...payload,
          isActive,
        })
        onSaved(updated)
        setMessage(t.admin.brandUpdated)
      } else {
        const created = await createCatalogBrand(payload)
        onSaved({ ...created, isActive: true })
        setName("")
        setSlug("")
        setMessage(t.admin.brandCreated)
      }
    } catch (error) {
      setMessage(resolveReferenceApiMessage(
        error,
        brand ? t.admin.referenceUpdateFailed : t.admin.brandCreateFailed,
      ))
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="grid gap-4">
      <ReferenceTextField
        label={t.admin.brandName}
        value={name}
        onChange={setName}
      />
      <ReferenceTextField
        label={t.admin.referenceSlug}
        value={slug}
        onChange={setSlug}
        hint={t.admin.referenceSlugHint}
        slug
      />
      {brand ? (
        <ReferenceActiveField
          checked={isActive}
          onChange={setIsActive}
          label={t.admin.activeReference}
        />
      ) : null}
      <div className="flex flex-wrap gap-2">
        <Button type="submit" disabled={saving}>
          {saving ? <Loader2 className="animate-spin" /> : brand ? <Save /> : <Tags />}
          {saving
            ? brand ? t.admin.updatingReference : t.admin.creatingBrand
            : brand ? t.admin.updateBrand : t.admin.createBrand}
        </Button>
        <Button type="button" variant="outline" onClick={onCancel}>
          <X />
          {t.admin.cancelReferenceForm}
        </Button>
      </div>
      {message ? (
        <p className="text-sm text-muted-foreground" role="status">{message}</p>
      ) : null}
    </form>
  )
}
