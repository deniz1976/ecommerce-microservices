"use client"

import { useEffect, useState } from "react"

import { getCatalogProduct } from "@/lib/api/catalog"
import { useI18n } from "@/lib/i18n/provider"

export function useLocalizedProductNames(productIds: string[]): Record<string, string> {
  const { locale } = useI18n()
  const [nameByProductId, setNameByProductId] = useState<Record<string, string>>({})
  const productIdKey = productIds.join(",")

  useEffect(() => {
    if (productIdKey.length === 0) {
      return
    }

    const ids = productIdKey.split(",")
    let active = true
    Promise.all(
      ids.map((productId) =>
        getCatalogProduct(productId)
          .then((product) => [productId, product.name] as const)
          .catch(() => null),
      ),
    ).then((entries) => {
      if (!active) {
        return
      }

      setNameByProductId(Object.fromEntries(entries.filter((entry) => entry !== null)))
    })

    return () => {
      active = false
    }
  }, [locale, productIdKey])

  return nameByProductId
}
