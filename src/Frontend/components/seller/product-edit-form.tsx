"use client"

import Image from "next/image"
import { type ChangeEvent, type FormEvent, useEffect, useState } from "react"
import { ImagePlus, Images, Loader2, PackageCheck, Star, Trash2, X } from "lucide-react"

import { Button } from "@/components/ui/button"
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

export function ProductEditForm({ product, onCancel, onUpdated }: ProductEditFormProps) {
  const { locale, t } = useI18n()
  const [references, setReferences] = useState<ReferenceState>({ status: "loading" })
  const [name, setName] = useState(product.name)
  const [description, setDescription] = useState(product.description)
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
    Promise.all([getCatalogCategories(locale), getCatalogBrands()])
      .then(([categories, brands]) => {
        if (active) setReferences({ status: "ready", categories, brands })
      })
      .catch(() => {
        if (active) setReferences({ status: "unavailable" })
      })

    return () => {
      active = false
    }
  }, [locale])

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
        translations: [{
          languageCode: locale,
          name: name.trim(),
          description: description.trim(),
        }],
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
    <form onSubmit={handleSubmit} className="rounded-lg border border-primary/30 bg-background p-5 shadow-sm">
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
          <Field label={t.seller.productName} value={name} onChange={setName} required />
          <Field label={t.seller.productDescription} value={description} onChange={setDescription} required />
          <SelectField label={t.seller.category} value={categoryId} onChange={setCategoryId} options={references.categories} />
          <SelectField label={t.seller.brand} value={brandId} onChange={setBrandId} options={references.brands} />
          <div className="grid min-w-0 grid-cols-2 gap-3">
            <Field label={t.seller.price} value={price} onChange={setPrice} type="number" min="0.01" step="0.01" required />
            <Field label={t.seller.currency} value={currency} onChange={setCurrency} minLength={3} maxLength={3} required />
          </div>
          <label className="grid gap-2 text-sm font-medium text-foreground">
            {t.seller.statusLabel}
            <select value={status} onChange={(event) => setStatus(Number(event.target.value) as ProductStatus)} className="h-10 w-full min-w-0 rounded-md border border-input bg-background px-3 font-normal">
              <option value={0}>{t.seller.draft}</option>
              <option value={1}>{t.seller.active}</option>
              <option value={2}>{t.seller.inactive}</option>
              <option value={3}>{t.seller.archived}</option>
            </select>
          </label>

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

interface FieldProps {
  label: string
  value: string
  onChange: (value: string) => void
  type?: string
  required?: boolean
  min?: string
  step?: string
  minLength?: number
  maxLength?: number
}

function Field({ label, value, onChange, type = "text", ...inputProps }: FieldProps) {
  return (
    <label className="grid min-w-0 gap-2 text-sm font-medium text-foreground">
      {label}
      <input type={type} value={value} onChange={(event) => onChange(event.target.value)} {...inputProps} className="h-10 w-full min-w-0 rounded-md border border-input bg-background px-3 font-normal" />
    </label>
  )
}

interface SelectFieldProps {
  label: string
  value: string
  onChange: (value: string) => void
  options: Array<{ id: string; name: string }>
}

function SelectField({ label, value, onChange, options }: SelectFieldProps) {
  return (
    <label className="grid min-w-0 gap-2 text-sm font-medium text-foreground">
      {label}
      <select value={value} onChange={(event) => onChange(event.target.value)} className="h-10 w-full min-w-0 rounded-md border border-input bg-background px-3 font-normal">
        {options.map((option) => <option key={option.id} value={option.id}>{option.name}</option>)}
      </select>
    </label>
  )
}
