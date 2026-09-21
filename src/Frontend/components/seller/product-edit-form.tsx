"use client"

import Image from "next/image"
import { type ChangeEvent, type FormEvent, useEffect, useState } from "react"
import { ImagePlus, Images, Loader2, PackageCheck, Star, Trash2, X } from "lucide-react"

import { FormField, SelectField, TextField } from "@/components/patterns/form-field"
import { Button } from "@/components/ui/button"
import { SelectNative } from "@/components/ui/select-native"
import {
  deleteCatalogProductImage,
  getCatalogBrands,
  getCatalogCategories,
  getManagedCatalogProduct,
  setMainCatalogProductImage,
  updateCatalogProduct,
  uploadCatalogProductImage,
} from "@/lib/api/catalog"
import { useI18n } from "@/lib/i18n/provider"
import type {
  CatalogBrandReference,
  CatalogCategoryReference,
  CatalogProduct,
  CatalogProductTranslation,
  ProductStatus,
} from "@/types"

interface ProductEditFormProps {
  product: CatalogProduct
  onCancel: () => void
  onUpdated: (product: CatalogProduct) => void
}

type ReferenceState =
  | { status: "loading" }
  | { status: "ready"; categories: CatalogCategoryReference[]; brands: CatalogBrandReference[] }
  | { status: "unavailable" }

interface LocalizedText {
  name: string
  description: string
}

export function ProductEditForm({ product, onCancel, onUpdated }: ProductEditFormProps) {
  const { locale, t } = useI18n()
  const [references, setReferences] = useState<ReferenceState>({ status: "loading" })
  const [englishText, setEnglishText] = useState<LocalizedText>({ name: "", description: "" })
  const [turkishText, setTurkishText] = useState<LocalizedText>({ name: "", description: "" })
  const [categoryId, setCategoryId] = useState(product.categoryId)
  const [brandId, setBrandId] = useState(product.brandId)
  const [price, setPrice] = useState(String(product.price))
  const [currency, setCurrency] = useState(product.currency)
  const [status, setStatus] = useState<ProductStatus>(product.status)
  const [images, setImages] = useState(product.images)
  const [saving, setSaving] = useState(false)
  const [imageBusy, setImageBusy] = useState(false)
  const [message, setMessage] = useState<string | null>(null)
  const [imageMessage, setImageMessage] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    Promise.all([
      getCatalogCategories(locale),
      getCatalogBrands(),
      getManagedCatalogProduct(product.id),
    ])
      .then(([categories, brands, managed]) => {
        if (!active) return
        setEnglishText(selectTranslation(managed.translations, "en"))
        setTurkishText(selectTranslation(managed.translations, "tr"))
        setReferences({ status: "ready", categories, brands })
      })
      .catch(() => {
        if (active) setReferences({ status: "unavailable" })
      })

    return () => {
      active = false
    }
  }, [locale, product.id])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (references.status !== "ready") return

    setSaving(true)
    setMessage(null)
    try {
      const updated = await updateCatalogProduct(product.id, {
        categoryId,
        brandId,
        price: Number(price),
        currency: currency.trim().toUpperCase(),
        status,
        translations: buildTranslations(englishText, turkishText),
      })
      setImages(updated.images)
      setMessage(t.seller.productUpdated)
      onUpdated(updated)
    } catch {
      setMessage(t.seller.productUpdateFailed)
    } finally {
      setSaving(false)
    }
  }

  async function refreshProduct() {
    const refreshed = await getManagedCatalogProduct(product.id)
    setImages(refreshed.images)
    onUpdated(refreshed)
  }

  async function handleImageUpload(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    event.target.value = ""
    if (!file) return

    if (file.size > 5 * 1024 * 1024) {
      setImageMessage(t.seller.imageInvalid)
      return
    }

    setImageBusy(true)
    setImageMessage(null)
    try {
      await uploadCatalogProductImage(product.id, file)
      await refreshProduct()
      setImageMessage(t.seller.imageUploaded)
    } catch {
      setImageMessage(t.seller.imageUploadFailed)
    } finally {
      setImageBusy(false)
    }
  }

  async function handleSetMain(imageId: string) {
    setImageBusy(true)
    setImageMessage(null)
    try {
      await setMainCatalogProductImage(product.id, imageId)
      await refreshProduct()
      setImageMessage(t.seller.mainImageUpdated)
    } catch {
      setImageMessage(t.seller.imageUpdateFailed)
    } finally {
      setImageBusy(false)
    }
  }

  async function handleDeleteImage(imageId: string) {
    setImageBusy(true)
    setImageMessage(null)
    try {
      await deleteCatalogProductImage(product.id, imageId)
      await refreshProduct()
      setImageMessage(t.seller.imageDeleted)
    } catch {
      setImageMessage(t.seller.imageDeleteFailed)
    } finally {
      setImageBusy(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-primary/30 bg-card p-5">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div className="flex items-center gap-2">
            <PackageCheck className="size-4 text-primary" />
            <h2 className="font-heading text-lg font-semibold text-foreground">{t.seller.editProduct}</h2>
          </div>
          <p className="mt-1 truncate text-xs text-muted-foreground">{product.sku}</p>
        </div>
        <Button type="button" variant="ghost" size="icon" onClick={onCancel} aria-label={t.seller.cancelEditing}>
          <X />
        </Button>
      </div>

      {references.status === "loading" ? (
        <div className="flex min-h-24 items-center justify-center">
          <Loader2 className="size-5 animate-spin text-muted-foreground" />
        </div>
      ) : references.status === "unavailable" ? (
        <p className="mt-5 text-sm text-destructive">{t.seller.referencesUnavailable}</p>
      ) : (
        <div className="mt-5 grid gap-4">
          <TextField
            label={t.seller.productNameEnglish}
            value={englishText.name}
            onChange={(value) => setEnglishText({ ...englishText, name: value })}
            required
          />
          <TextField
            label={t.seller.productDescriptionEnglish}
            value={englishText.description}
            onChange={(value) => setEnglishText({ ...englishText, description: value })}
            required
          />
          <TextField
            label={t.seller.productNameTurkish}
            value={turkishText.name}
            onChange={(value) => setTurkishText({ ...turkishText, name: value })}
          />
          <TextField
            label={t.seller.productDescriptionTurkish}
            value={turkishText.description}
            onChange={(value) => setTurkishText({ ...turkishText, description: value })}
          />
          <SelectField label={t.seller.category} value={categoryId} onChange={setCategoryId} options={references.categories} />
          <SelectField label={t.seller.brand} value={brandId} onChange={setBrandId} options={references.brands} />
          <div className="grid min-w-0 grid-cols-2 gap-3">
            <TextField label={t.seller.price} value={price} onChange={setPrice} type="number" min="0.01" step="0.01" required />
            <TextField label={t.seller.currency} value={currency} onChange={setCurrency} minLength={3} maxLength={3} required />
          </div>
          <FormField label={t.seller.statusLabel}>
            <SelectNative
              value={status}
              onChange={(event) => setStatus(Number(event.target.value) as ProductStatus)}
            >
              <option value={0}>{t.seller.draft}</option>
              <option value={1}>{t.seller.active}</option>
              <option value={2}>{t.seller.inactive}</option>
              <option value={3}>{t.seller.archived}</option>
            </SelectNative>
          </FormField>

          <div>
            <div className="flex items-center gap-2 text-sm font-medium text-foreground">
              <Images className="size-4 text-primary" />
              {t.seller.currentImages}
            </div>
            {images.length > 0 ? (
              <div className="mt-2 grid grid-cols-3 gap-2">
                {images.map((image) => (
                  <div key={image.id} className="group relative aspect-square overflow-hidden rounded-md border border-border bg-muted">
                    <Image src={image.secureUrl || image.url} alt={product.name} fill unoptimized className="object-cover" />
                    {image.isMain ? (
                      <span className="absolute left-1.5 top-1.5 rounded bg-primary px-1.5 py-0.5 text-[10px] font-medium text-primary-foreground">
                        {t.seller.mainImage}
                      </span>
                    ) : null}
                    <div className="absolute inset-x-1.5 bottom-1.5 flex justify-end gap-1">
                      {!image.isMain ? (
                        <Button
                          type="button"
                          size="icon-xs"
                          variant="secondary"
                          disabled={imageBusy}
                          onClick={() => handleSetMain(image.id)}
                          aria-label={t.seller.setMainImage}
                        >
                          <Star />
                        </Button>
                      ) : null}
                      <Button
                        type="button"
                        size="icon-xs"
                        variant="destructive"
                        disabled={imageBusy}
                        onClick={() => handleDeleteImage(image.id)}
                        aria-label={t.seller.deleteImage}
                      >
                        <Trash2 />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="mt-2 text-xs text-muted-foreground">{t.seller.noImages}</p>
            )}
            <label className="mt-3 flex cursor-pointer items-center justify-center gap-2 rounded-md border border-dashed border-border px-3 py-3 text-xs font-medium text-foreground transition hover:bg-muted">
              {imageBusy ? <Loader2 className="size-4 animate-spin" /> : <ImagePlus className="size-4 text-primary" />}
              {imageBusy ? t.seller.uploadingImage : t.seller.uploadImage}
              <input
                type="file"
                accept="image/jpeg,image/png,image/gif,image/webp"
                className="sr-only"
                disabled={imageBusy || images.length >= 8}
                onChange={handleImageUpload}
              />
            </label>
            <p className="mt-2 text-xs leading-5 text-muted-foreground">{t.seller.imageUploadHint}</p>
            {imageMessage ? <p className="mt-2 text-xs text-muted-foreground" role="status">{imageMessage}</p> : null}
          </div>
        </div>
      )}

      {message ? <p className="mt-4 text-sm text-muted-foreground" role="status">{message}</p> : null}
      {references.status === "ready" ? (
        <Button type="submit" disabled={saving} className="mt-5 w-full">
          {saving ? <Loader2 className="animate-spin" /> : <PackageCheck />}
          {saving ? t.seller.savingChanges : t.seller.saveChanges}
        </Button>
      ) : null}
    </form>
  )
}

function selectTranslation(
  translations: CatalogProductTranslation[],
  languageCode: CatalogProductTranslation["languageCode"],
): LocalizedText {
  const translation = translations.find((entry) => entry.languageCode === languageCode)

  return {
    name: translation?.name ?? "",
    description: translation?.description ?? "",
  }
}

function buildTranslations(english: LocalizedText, turkish: LocalizedText) {
  const translations: Array<CatalogProductTranslation> = [
    {
      languageCode: "en",
      name: english.name.trim(),
      description: english.description.trim(),
    },
  ]

  if (turkish.name.trim() !== "" && turkish.description.trim() !== "") {
    translations.push({
      languageCode: "tr",
      name: turkish.name.trim(),
      description: turkish.description.trim(),
    })
  }

  return translations
}
