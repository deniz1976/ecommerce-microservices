import Image from "next/image"
import Link from "next/link"
import { CheckIcon, Loader2Icon, PackageIcon, ShoppingCartIcon } from "lucide-react"

import { Button } from "@/components/ui/button"
import type { Locale } from "@/lib/i18n/dictionaries"
import { formatMoney } from "@/lib/i18n/format"
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
  const price = formatMoney(product.price, product.currency, locale)

  return (
    <article className="group flex flex-col overflow-hidden rounded-xl border border-border bg-card transition-colors hover:border-foreground/20">
      <Link
        href={`/products/${product.id}`}
        aria-label={`${product.name} — ${viewLabel}`}
        className="relative flex aspect-square items-center justify-center overflow-hidden bg-muted outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
      >
        {image ? (
          <Image
            src={image.secureUrl || image.url}
            alt={product.name}
            fill
            unoptimized
            sizes="(min-width: 1280px) 20rem, (min-width: 640px) 45vw, 90vw"
            className="object-cover transition-transform duration-300 group-hover:scale-[1.02]"
          />
        ) : (
          <PackageIcon className="size-8 text-muted-foreground/40" aria-hidden="true" />
        )}
      </Link>

      <div className="flex flex-1 flex-col gap-3 p-4">
        <div className="flex flex-wrap items-center gap-1.5">
          {product.brandName ? (
            <span className="rounded-4xl border border-border/60 bg-muted/50 px-2 py-0.5 text-xs font-medium">
              {product.brandName}
            </span>
          ) : null}
          {product.categoryName ? (
            <span className="text-xs text-muted-foreground">{product.categoryName}</span>
          ) : null}
        </div>

        <div className="flex flex-1 flex-col gap-1">
          <h2 className="line-clamp-2 font-heading text-base leading-snug font-semibold">
            <Link
              href={`/products/${product.id}`}
              className="outline-none hover:underline focus-visible:underline"
            >
              {product.name}
            </Link>
          </h2>
          <p className="line-clamp-2 text-sm leading-5 text-muted-foreground">
            {product.description}
          </p>
        </div>

        <p className="font-heading text-xl font-semibold tabular-nums">{price}</p>

        {onAdd ? (
          <Button
            type="button"
            size="lg"
            className="w-full"
            disabled={adding}
            onClick={() => onAdd(product)}
          >
            {adding ? (
              <Loader2Icon className="animate-spin" />
            ) : added ? (
              <CheckIcon />
            ) : (
              <ShoppingCartIcon />
            )}
            {adding ? addingLabel : added ? addedLabel : addLabel}
          </Button>
        ) : null}
      </div>
    </article>
  )
}
