"use client"

import Image from "next/image"
import Link from "next/link"
import { ArrowLeft, Check, Loader2, Package, ShoppingCart } from "lucide-react"
import { useParams } from "next/navigation"
import { useEffect, useState } from "react"
import type { ReactNode } from "react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { Button, buttonVariants } from "@/components/ui/button"
import { incrementBasketItem } from "@/lib/api/basket"
import { getProfile } from "@/lib/api/auth"
import { getCatalogProduct } from "@/lib/api/catalog"
import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"
import type { CatalogProduct } from "@/types"

type ProductState =
  | { status: "loading" }
  | { status: "ready"; product: CatalogProduct }
  | { status: "unavailable" }

export function ProductDetail() {
  const { locale, t } = useI18n()
  const params = useParams<{ id: string }>()
  const [state, setState] = useState<ProductState>({ status: "loading" })
  const [customerId, setCustomerId] = useState<string | null>(null)
  const [adding, setAdding] = useState(false)
  const [added, setAdded] = useState(false)
  const [addFailed, setAddFailed] = useState(false)

  useEffect(() => {
    let active = true
    getCatalogProduct(params.id)
      .then((product) => {
        if (!active) return
        setState(product.status === 1 ? { status: "ready", product } : { status: "unavailable" })
      })
      .catch(() => {
        if (active) setState({ status: "unavailable" })
      })
    return () => {
      active = false
    }
  }, [params.id])

  useEffect(() => {
    let active = true
    getProfile()
      .then((profile) => {
        if (active && profile.roles.includes("Customer")) setCustomerId(profile.id)
      })
      .catch(() => undefined)
    return () => {
      active = false
    }
  }, [])

  async function addToBasket(product: CatalogProduct) {
    if (!customerId) return
    setAdding(true)
    setAdded(false)
    setAddFailed(false)

    try {
      await incrementBasketItem(customerId, product.id)
      setAdded(true)
    } catch {
      setAddFailed(true)
    } finally {
      setAdding(false)
    }
  }

  return (
    <div className="min-h-svh bg-muted/30">
      <header className="flex h-16 items-center justify-between border-b border-border bg-background px-4 sm:px-6">
        <Logo />
        <div className="flex items-center gap-2">
          <LanguageSwitcher />
          <ThemeToggle />
          {customerId ? (
            <Link href="/basket" className={cn(buttonVariants({ variant: "outline", size: "sm" }), "gap-1.5")}>
              <ShoppingCart />
              {t.basket.openBasket}
            </Link>
          ) : null}
        </div>
      </header>
      <main className="mx-auto w-full max-w-6xl px-4 py-8 sm:px-6 lg:px-8">
        <Link href="/" className="inline-flex items-center gap-2 text-sm font-medium text-primary hover:underline">
          <ArrowLeft className="size-4" />
          {t.customer.backToCatalog}
        </Link>
        {state.status === "loading" ? (
          <div className="flex min-h-[28rem] items-center justify-center">
            <Loader2 className="size-6 animate-spin text-muted-foreground" />
          </div>
        ) : state.status === "unavailable" ? (
          <div className="mt-8 flex min-h-80 flex-col items-center justify-center rounded-xl border border-dashed border-border bg-card px-6 text-center">
            <Package className="size-9 text-muted-foreground" />
            <p className="mt-3 text-sm text-muted-foreground">{t.customer.productUnavailable}</p>
          </div>
        ) : (
          <ProductContent
            product={state.product}
            locale={locale}
            labels={{
              detail: t.customer.productDetails,
              sku: t.seller.sku,
              category: t.customer.category,
              brand: t.customer.brand,
            }}
            basketAction={customerId ? (
              <div className="mt-6">
                <Button type="button" size="lg" className="w-full sm:w-auto" disabled={adding} onClick={() => addToBasket(state.product)}>
                  {adding ? <Loader2 className="animate-spin" /> : added ? <Check /> : <ShoppingCart />}
                  {adding ? t.basket.adding : added ? t.basket.added : t.basket.addToBasket}
                </Button>
                {addFailed ? <p className="mt-2 text-sm text-destructive">{t.basket.addFailed}</p> : null}
              </div>
            ) : (
              <Link href="/login" className={cn(buttonVariants({ size: "lg" }), "mt-6 w-full sm:w-auto")}>
                {t.basket.signInToAdd}
              </Link>
            )}
          />
        )}
      </main>
    </div>
  )
}

interface ProductContentProps {
  product: CatalogProduct
  locale: "en" | "tr"
  labels: {
    detail: string
    sku: string
    category: string
    brand: string
  }
  basketAction: ReactNode
}

function ProductContent({ product, locale, labels, basketAction }: ProductContentProps) {
  const image = product.images[0]
  const price = new Intl.NumberFormat(locale === "tr" ? "tr-TR" : "en-US", {
    style: "currency",
    currency: product.currency,
  }).format(product.price)

  return (
    <article className="mt-8 grid overflow-hidden rounded-2xl border border-border bg-card shadow-sm lg:grid-cols-2">
      <div className="relative flex min-h-80 items-center justify-center bg-muted lg:min-h-[34rem]">
        {image ? (
          <Image src={image.secureUrl || image.url} alt={product.name} fill unoptimized className="object-cover" />
        ) : (
          <Package className="size-16 text-muted-foreground/40" />
        )}
      </div>
      <div className="flex flex-col justify-center p-6 sm:p-10">
        <p className="text-sm font-medium text-primary">{labels.detail}</p>
        <p className="mt-4 text-xs uppercase tracking-[0.16em] text-muted-foreground">{product.brandName || product.sku}</p>
        <h1 className="mt-3 font-heading text-3xl font-semibold tracking-tight text-foreground sm:text-4xl">{product.name}</h1>
        <p className="mt-5 text-base leading-7 text-muted-foreground">{product.description}</p>
        <dl className="mt-8 grid gap-4 border-t border-border pt-6 text-sm sm:grid-cols-2">
          <div><dt className="text-muted-foreground">{labels.sku}</dt><dd className="mt-1 font-medium text-foreground">{product.sku}</dd></div>
          <div><dt className="text-muted-foreground">{labels.category}</dt><dd className="mt-1 font-medium text-foreground">{product.categoryName || "-"}</dd></div>
          <div><dt className="text-muted-foreground">{labels.brand}</dt><dd className="mt-1 font-medium text-foreground">{product.brandName || "-"}</dd></div>
        </dl>
        <p className="mt-8 font-heading text-3xl font-semibold text-foreground">{price}</p>
        {basketAction}
      </div>
    </article>
  )
}
