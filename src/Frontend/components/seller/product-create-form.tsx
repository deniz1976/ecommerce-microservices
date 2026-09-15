"use client"

import { type FormEvent, useCallback, useEffect, useState } from "react"
import { Loader2, PackagePlus, RefreshCcw } from "lucide-react"

import { FormField, SelectField, TextField } from "@/components/patterns/form-field"
import { Button } from "@/components/ui/button"
import { SelectNative } from "@/components/ui/select-native"
import {
  createCatalogProduct,
  getCatalogBrands,
  getCatalogCategories,
} from "@/lib/api/catalog"
import type { Locale } from "@/lib/i18n/dictionaries"
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
  const [englishName, setEnglishName] = useState("")
  const [englishDescription, setEnglishDescription] = useState("")
  const [turkishName, setTurkishName] = useState("")
  const [turkishDescription, setTurkishDescription] = useState("")
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
        translations: buildTranslations(
          { name: englishName, description: englishDescription },
          { name: turkishName, description: turkishDescription },
        ),
      })
      setSku("")
      setEnglishName("")
      setEnglishDescription("")
      setTurkishName("")
      setTurkishDescription("")
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
          <TextField label={t.seller.sku} value={sku} onChange={setSku} required />
          <TextField label={t.seller.productNameEnglish} value={englishName} onChange={setEnglishName} required />
          <TextField
            label={t.seller.productDescriptionEnglish}
            value={englishDescription}
            onChange={setEnglishDescription}
            required
          />
          <TextField label={t.seller.productNameTurkish} value={turkishName} onChange={setTurkishName} />
          <TextField
            label={t.seller.productDescriptionTurkish}
            value={turkishDescription}
            onChange={setTurkishDescription}
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
            </SelectNative>
          </FormField>
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

function buildTranslations(
  english: { name: string; description: string },
  turkish: { name: string; description: string },
) {
  const translations: Array<{ languageCode: Locale; name: string; description: string }> = [
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

