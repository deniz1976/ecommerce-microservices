"use client"

import Image from "next/image"
import Link from "next/link"
import { ArrowLeftIcon, CheckIcon, Loader2Icon, PackageIcon, ShoppingCartIcon } from "lucide-react"
import { useParams } from "next/navigation"
import { useEffect, useState } from "react"
import type { ReactNode } from "react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { EmptyState } from "@/components/patterns/states"
import { Button, buttonVariants } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { incrementBasketItem } from "@/lib/api/basket"
import { getProfile } from "@/lib/api/auth"
import { getCatalogProduct } from "@/lib/api/catalog"
import type { Locale } from "@/lib/i18n/dictionaries"
import { formatMoney } from "@/lib/i18n/format"
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
  }, [locale, params.id])

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
            <Link
              href="/basket"
              className={cn(buttonVariants({ variant: "outline", size: "sm" }), "gap-1.5")}
            >
              <ShoppingCartIcon />
              {t.basket.openBasket}
            </Link>
          ) : null}
        </div>
      </header>

      <main className="mx-auto w-full max-w-6xl px-4 py-8 sm:px-6 lg:px-8">
        <Link
          href="/"
          className="inline-flex items-center gap-2 text-sm font-medium text-primary hover:underline"
        >
          <ArrowLeftIcon className="size-4" />
          {t.customer.backToCatalog}
        </Link>

        {state.status === "loading" ? (
          <div className="mt-8 grid gap-8 lg:grid-cols-2">
            <Skeleton className="aspect-square rounded-2xl" />
            <div className="flex flex-col gap-4">
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-10 w-3/4" />
              <Skeleton className="h-20 w-full" />
              <Skeleton className="h-9 w-40" />
            </div>
          </div>
        ) : state.status === "unavailable" ? (
          <EmptyState className="mt-8" title={t.customer.productUnavailable} description="" />
        ) : (
          <ProductContent
            product={state.product}
            locale={locale}
            labels={{
              detail: t.customer.productDetails,
              sku: t.seller.sku,
              category: t.customer.category,
              brand: t.customer.brand,
              store: t.customer.viewStore,
            }}
            basketAction={
              customerId ? (
                <div className="flex flex-col gap-2">
                  <Button
                    type="button"
                    size="lg"
                    className="w-full sm:w-auto"
                    disabled={adding}
                    onClick={() => addToBasket(state.product)}
                  >
                    {adding ? (
                      <Loader2Icon className="animate-spin" />
                    ) : added ? (
                      <CheckIcon />
                    ) : (
                      <ShoppingCartIcon />
                    )}
                    {adding ? t.basket.adding : added ? t.basket.added : t.basket.addToBasket}
                  </Button>
                  {addFailed ? (
                    <p className="text-sm text-destructive">{t.basket.addFailed}</p>
                  ) : null}
                </div>
              ) : (
                <Link
                  href="/login"
                  className={cn(buttonVariants({ size: "lg" }), "w-full sm:w-auto")}
                >
                  {t.basket.signInToAdd}
                </Link>
              )
            }
          />
        )}
      </main>
    </div>
  )
}

interface ProductContentProps {
  product: CatalogProduct
  locale: Locale
  labels: {
    detail: string
    sku: string
    category: string
    brand: string
    store: string
  }
  basketAction: ReactNode
}

function ProductContent({ product, locale, labels, basketAction }: ProductContentProps) {
  const images = product.images
  const [activeImageId, setActiveImageId] = useState<string | null>(null)
  const activeImage = images.find((image) => image.id === activeImageId) ?? images[0]
  const price = formatMoney(product.price, product.currency, locale)

  return (
    <article className="mt-8 grid gap-8 lg:grid-cols-2 lg:gap-12">
      <div className="flex flex-col gap-3">
        <div className="relative flex aspect-square items-center justify-center overflow-hidden rounded-2xl border border-border bg-muted">
          {activeImage ? (
            <Image
              src={activeImage.secureUrl || activeImage.url}
              alt={product.name}
              fill
              unoptimized
              sizes="(min-width: 1024px) 32rem, 90vw"
              className="object-cover"
            />
          ) : (
            <PackageIcon className="size-14 text-muted-foreground/40" aria-hidden="true" />
          )}
        </div>

        {images.length > 1 ? (
          <div className="grid grid-cols-5 gap-2">
            {images.map((image) => (
              <button
                key={image.id}
                type="button"
                onClick={() => setActiveImageId(image.id)}
                aria-current={image.id === activeImage?.id}
                className={cn(
                  "relative aspect-square overflow-hidden rounded-lg border bg-muted outline-none transition-colors focus-visible:ring-3 focus-visible:ring-ring/50",
                  image.id === activeImage?.id
                    ? "border-foreground/40"
                    : "border-border hover:border-foreground/20",
                )}
              >
                <Image
                  src={image.secureUrl || image.url}
                  alt=""
                  fill
                  unoptimized
                  sizes="6rem"
                  className="object-cover"
                />
              </button>
            ))}
          </div>
        ) : null}
      </div>

      <div className="flex flex-col gap-5">
        <div className="flex flex-col gap-2">
          <span className="text-xs font-medium tracking-wide text-muted-foreground uppercase">
            {product.brandName || product.sku}
          </span>
          <h1 className="font-heading text-3xl font-semibold tracking-tight sm:text-4xl">
            {product.name}
          </h1>
          <p className="font-heading text-3xl font-semibold tabular-nums">{price}</p>
        </div>

        {basketAction}

        <p className="text-base leading-7 text-muted-foreground">{product.description}</p>

        <dl className="grid gap-4 border-t border-border pt-6 text-sm sm:grid-cols-3">
          <div className="flex flex-col gap-1">
            <dt className="text-muted-foreground">{labels.sku}</dt>
            <dd className="font-mono text-xs font-medium">{product.sku}</dd>
          </div>
          <div className="flex flex-col gap-1">
            <dt className="text-muted-foreground">{labels.category}</dt>
            <dd className="font-medium">{product.categoryName || "-"}</dd>
          </div>
          <div className="flex flex-col gap-1">
            <dt className="text-muted-foreground">{labels.brand}</dt>
            <dd className="font-medium">{product.brandName || "-"}</dd>
          </div>
        </dl>

        {product.storeId ? (
          <Link
            href={`/stores/${product.storeId}`}
            className="inline-flex w-fit text-sm font-medium text-primary hover:underline"
          >
            {labels.store}
          </Link>
        ) : null}
      </div>
    </article>
  )
}
