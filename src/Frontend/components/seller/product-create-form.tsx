"use client"

import { type FormEvent, useCallback, useEffect, useState } from "react"
import { Loader2, PackagePlus, RefreshCcw } from "lucide-react"

import { Button } from "@/components/ui/button"
import {
  createCatalogProduct,
  getCatalogBrands,
  getCatalogCategories,
} from "@/lib/api/catalog"
import { useI18n } from "@/lib/i18n/provider"
import type {
  CatalogBrandReference,
  CatalogCategoryReference,
  CatalogProduct,
  CatalogStore,
  ProductStatus,
} from "@/types"

interface ProductCreateFormProps {
  store: CatalogStore
  onCreated: (product: CatalogProduct) => void
}

type ReferenceState =
  | { status: "loading" }
  | { status: "ready"; categories: CatalogCategoryReference[]; brands: CatalogBrandReference[] }
  | { status: "unavailable" }

export function ProductCreateForm({ store, onCreated }: ProductCreateFormProps) {
  const { locale, t } = useI18n()
  const [references, setReferences] = useState<ReferenceState>({ status: "loading" })
  const [sku, setSku] = useState("")
  const [name, setName] = useState("")
  const [description, setDescription] = useState("")
  const [categoryId, setCategoryId] = useState("")
  const [brandId, setBrandId] = useState("")
  const [price, setPrice] = useState("")
  const [currency, setCurrency] = useState("USD")
  const [status, setStatus] = useState<ProductStatus>(0)
  const [saving, setSaving] = useState(false)
  const [refreshingReferences, setRefreshingReferences] = useState(false)
  const [message, setMessage] = useState<string | null>(null)

  const loadReferences = useCallback(async (showProgress = false) => {
    if (showProgress) setRefreshingReferences(true)
    try {
      const [categories, brands] = await Promise.all([
        getCatalogCategories(locale),
        getCatalogBrands(),
      ])
      setReferences({ status: "ready", categories, brands })
      setCategoryId(categories[0]?.id ?? "")
      setBrandId(brands[0]?.id ?? "")
    } catch {
      setReferences({ status: "unavailable" })
    } finally {
      if (showProgress) setRefreshingReferences(false)
    }
  }, [locale])

  useEffect(() => {
    let active = true
    Promise.all([getCatalogCategories(locale), getCatalogBrands()])
      .then(([categories, brands]) => {
        if (!active) return
        setReferences({ status: "ready", categories, brands })
        setCategoryId(categories[0]?.id ?? "")
        setBrandId(brands[0]?.id ?? "")
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
    if (references.status !== "ready" || !categoryId || !brandId) return

    setSaving(true)
    setMessage(null)
    try {
      const product = await createCatalogProduct({
        sku: sku.trim(),
        categoryId,
        brandId,
        storeId: store.id,
        price: Number(price),
        currency: currency.trim().toUpperCase(),
        status,
        translations: [{ languageCode: locale, name: name.trim(), description: description.trim() }],
      })
      setSku("")
      setName("")
      setDescription("")
      setPrice("")
      setStatus(0)
      setMessage(t.seller.productCreated)
      onCreated(product)
    } catch {
      setMessage(t.seller.productCreateFailed)
    } finally {
      setSaving(false)
    }
  }

  const hasReferences = references.status === "ready" && references.categories.length > 0 && references.brands.length > 0

  return (
    <form onSubmit={handleSubmit} className="rounded-lg border border-border bg-background p-5">
      <div className="flex items-center gap-2">
        <PackagePlus className="size-4 text-primary" />
        <div className="min-w-0">
          <h2 className="font-heading text-lg font-semibold text-foreground">{t.seller.createProduct}</h2>
          <p className="truncate text-xs text-muted-foreground">{store.name}</p>
        </div>
      </div>

      {references.status === "loading" ? (
        <div className="flex min-h-24 items-center justify-center"><Loader2 className="size-5 animate-spin text-muted-foreground" /></div>
      ) : references.status === "unavailable" ? (
        <ReferenceUnavailable
          message={t.seller.referencesUnavailable}
          action={t.seller.refreshReferences}
          progress={t.seller.refreshingReferences}
          refreshing={refreshingReferences}
          onRefresh={() => loadReferences(true)}
        />
      ) : !hasReferences ? (
        <ReferenceUnavailable
          message={t.seller.referencesEmpty}
          action={t.seller.refreshReferences}
          progress={t.seller.refreshingReferences}
          refreshing={refreshingReferences}
          onRefresh={() => loadReferences(true)}
        />
      ) : (
        <div className="mt-5 grid gap-4">
          <Field label={t.seller.sku} value={sku} onChange={setSku} required />
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
            </select>
          </label>
        </div>
      )}

      {message ? <p className="mt-4 text-sm text-muted-foreground" role="status">{message}</p> : null}
      {hasReferences ? (
        <Button type="submit" disabled={saving} className="mt-5 w-full">
          {saving ? <Loader2 className="animate-spin" /> : <PackagePlus />}
          {saving ? t.seller.savingProduct : t.seller.saveProduct}
        </Button>
      ) : null}
    </form>
  )
}

function ReferenceUnavailable({
  message,
  action,
  progress,
  refreshing,
  onRefresh,
}: {
  message: string
  action: string
  progress: string
  refreshing: boolean
  onRefresh: () => void
}) {
  return (
    <div className="mt-5 grid gap-3">
      <p className="text-sm text-muted-foreground">{message}</p>
      <Button type="button" variant="outline" onClick={onRefresh} disabled={refreshing}>
        {refreshing ? <Loader2 className="animate-spin" /> : <RefreshCcw />}
        {refreshing ? progress : action}
      </Button>
    </div>
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
