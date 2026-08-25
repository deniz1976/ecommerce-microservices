import Image from "next/image"
import Link from "next/link"
import { ArrowUpRight, Check, Loader2, Package, ShoppingCart } from "lucide-react"

import { Button } from "@/components/ui/button"
import type { Locale } from "@/lib/i18n/dictionaries"
import type { CatalogProduct } from "@/types"

interface CatalogProductCardProps {
  locale: Locale
  product: CatalogProduct
  viewLabel: string
  addLabel?: string
  addingLabel?: string
  addedLabel?: string
  adding?: boolean
  added?: boolean
  onAdd?: (product: CatalogProduct) => void
}

export function CatalogProductCard({
  locale,
  product,
  viewLabel,
  addLabel,
  addingLabel,
  addedLabel,
  adding,
  added,
  onAdd,
}: CatalogProductCardProps) {
  const image = product.images[0]
  const price = new Intl.NumberFormat(locale === "tr" ? "tr-TR" : "en-US", {
    style: "currency",
    currency: product.currency,
  }).format(product.price)

  return (
    <article className="group overflow-hidden rounded-xl border border-border bg-card shadow-sm transition hover:-translate-y-0.5 hover:shadow-md">
      <div className="relative flex aspect-[4/3] items-center justify-center overflow-hidden bg-muted">
        {image ? (
          <Image
            src={image.secureUrl || image.url}
            alt={product.name}
            fill
            unoptimized
            className="object-cover transition duration-300 group-hover:scale-[1.03]"
          />
        ) : (
          <Package className="size-10 text-muted-foreground/50" aria-hidden="true" />
        )}
      </div>
      <div className="p-5">
        <div className="flex items-center justify-between gap-3 text-xs text-muted-foreground">
          <span className="truncate">{product.brandName || product.sku}</span>
          <span className="shrink-0">{product.categoryName}</span>
        </div>
        <h2 className="mt-3 line-clamp-2 font-heading text-lg font-semibold text-foreground">
          {product.name}
        </h2>
        <p className="mt-2 line-clamp-2 min-h-10 text-sm leading-5 text-muted-foreground">
          {product.description}
        </p>
        <div className="mt-5 flex items-center justify-between gap-3 border-t border-border pt-4">
          <p className="font-heading text-lg font-semibold text-foreground">{price}</p>
          <Link
            href={`/products/${product.id}`}
            className="inline-flex items-center gap-1.5 text-sm font-medium text-primary hover:underline"
          >
            {viewLabel}
            <ArrowUpRight className="size-4" aria-hidden="true" />
          </Link>
        </div>
        {onAdd ? <Button
          type="button"
          className="mt-4 w-full"
          disabled={adding}
          onClick={() => onAdd(product)}
        >
          {adding ? <Loader2 className="animate-spin" /> : added ? <Check /> : <ShoppingCart />}
          {adding ? addingLabel : added ? addedLabel : addLabel}
        </Button> : null}
      </div>
    </article>
  )
}
