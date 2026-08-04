"use client"

import { type FormEvent, useState } from "react"
import { Loader2, Save, Tags, X } from "lucide-react"

import {
  ReferenceActiveField,
  ReferenceTextField,
  resolveReferenceApiMessage,
} from "@/components/admin/admin-reference-fields"
import { Button } from "@/components/ui/button"
import { createCatalogCategory, updateCatalogCategory } from "@/lib/api/catalog"
import { useI18n } from "@/lib/i18n/provider"
import type { ManagedCatalogCategoryReference } from "@/types"

export function AdminCategoryForm({
  category,
  onSaved,
  onCancel,
}: {
  category?: ManagedCatalogCategoryReference
  onSaved: (category: ManagedCatalogCategoryReference) => void
  onCancel: () => void
}) {
  const { t } = useI18n()
  const [englishName, setEnglishName] = useState(category?.englishName ?? "")
  const [turkishName, setTurkishName] = useState(category?.turkishName ?? "")
  const [slug, setSlug] = useState(category?.slug ?? "")
  const [isActive, setIsActive] = useState(category?.isActive ?? true)
  const [saving, setSaving] = useState(false)
  const [message, setMessage] = useState<string | null>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSaving(true)
    setMessage(null)

    const payload = {
      englishName: englishName.trim(),
      turkishName: turkishName.trim(),
      slug: slug.trim(),
    }

    try {
      if (category) {
        const updated = await updateCatalogCategory(category.id, {
          ...payload,
          isActive,
        })
        onSaved(updated)
        setMessage(t.admin.categoryUpdated)
      } else {
        const created = await createCatalogCategory(payload)
        onSaved({
          id: created.id,
          englishName: payload.englishName,
          turkishName: payload.turkishName,
          slug: created.slug,
          isActive: true,
        })
        setEnglishName("")
        setTurkishName("")
        setSlug("")
        setMessage(t.admin.categoryCreated)
      }
    } catch (error) {
      setMessage(resolveReferenceApiMessage(
        error,
        category ? t.admin.referenceUpdateFailed : t.admin.categoryCreateFailed,
      ))
    } finally {
      setSaving(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="grid gap-4">
      <div className="grid gap-4 md:grid-cols-2">
        <ReferenceTextField
          label={t.admin.categoryEnglishName}
          value={englishName}
          onChange={setEnglishName}
        />
        <ReferenceTextField
          label={t.admin.categoryTurkishName}
          value={turkishName}
          onChange={setTurkishName}
        />
      </div>
      <ReferenceTextField
        label={t.admin.referenceSlug}
        value={slug}
        onChange={setSlug}
        hint={t.admin.referenceSlugHint}
        slug
      />
      {category ? (
        <ReferenceActiveField
          checked={isActive}
          onChange={setIsActive}
          label={t.admin.activeReference}
        />
      ) : null}
      <div className="flex flex-wrap gap-2">
        <Button type="submit" disabled={saving}>
          {saving ? <Loader2 className="animate-spin" /> : category ? <Save /> : <Tags />}
          {saving
            ? category ? t.admin.updatingReference : t.admin.creatingCategory
            : category ? t.admin.updateCategory : t.admin.createCategory}
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
