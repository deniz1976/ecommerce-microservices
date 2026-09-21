"use client"

import Link from "next/link"
import {
  CheckCircle2Icon,
  CreditCardIcon,
  Loader2Icon,
  MinusIcon,
  PlusIcon,
  ShoppingBagIcon,
  Trash2Icon,
} from "lucide-react"
import { useRouter } from "next/navigation"
import { useEffect, useRef, useState } from "react"
import type { FormEvent } from "react"

import { CustomerShell } from "@/components/customer/customer-shell"
import { EmptyState, ErrorState } from "@/components/patterns/states"
import { Button, buttonVariants } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Skeleton } from "@/components/ui/skeleton"
import {
  addBasketItem,
  checkoutBasket,
  clearBasket,
  getBasket,
  removeBasketItem,
} from "@/lib/api/basket"
import { getProfile } from "@/lib/api/auth"
import { getInventoryItems } from "@/lib/api/inventory"
import { ApiError } from "@/lib/api/client"
import { useLocalizedProductNames } from "@/lib/hooks/use-localized-product-names"
import { formatMoney } from "@/lib/i18n/format"
import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"
import type {
  Basket,
  CheckoutBasketPayload,
  CheckoutBasketResult,
  UserProfile,
} from "@/types"

type BasketState =
  | { status: "loading" }
  | { status: "ready"; profile: UserProfile; basket: Basket | null }
  | { status: "unavailable" }
  | { status: "storeUnavailable" }

type BasketOperationError = "generic" | "storeUnavailable"

export function BasketView() {
  const { locale, t } = useI18n()
  const router = useRouter()
  const [state, setState] = useState<BasketState>({ status: "loading" })
  const [busyItemId, setBusyItemId] = useState<string | null>(null)
  const [clearing, setClearing] = useState(false)
  const [checkingOut, setCheckingOut] = useState(false)
  const [operationError, setOperationError] = useState<BasketOperationError | null>(null)
  const [stockByProductId, setStockByProductId] = useState<Record<string, number>>({})
  const [checkoutResult, setCheckoutResult] = useState<CheckoutBasketResult | null>(null)
  const [checkoutAddress, setCheckoutAddress] = useState<CheckoutBasketPayload>({
    checkoutId: "",
    recipientName: "",
    addressLine: "",
    city: "",
    countryCode: "",
    postalCode: "",
  })
  const checkoutId = useRef<string | null>(null)
  const nameByProductId = useLocalizedProductNames(
    state.status === "ready" && state.basket
      ? state.basket.items.map((item) => item.productId)
      : [],
  )

  useEffect(() => {
    let active = true

    getProfile()
      .then(async (profile) => {
        if (!profile.isOnboardingComplete) {
          router.replace("/onboarding/role")
          return
        }

        if (!profile.roles.includes("Customer")) {
          router.replace("/")
          return
        }

        try {
          const basket = await getBasket(profile.id)
          if (active) setState({ status: "ready", profile, basket })
        } catch (error) {
          if (!active) return
          if (error instanceof ApiError && error.status === 404) {
            setState({ status: "ready", profile, basket: null })
          } else if (error instanceof ApiError && error.status === 503) {
            setState({ status: "storeUnavailable" })
          } else {
            setState({ status: "unavailable" })
          }
        }
      })
      .catch(() => {
        if (active) router.replace("/login")
      })

    return () => {
      active = false
    }
  }, [router])

  useEffect(() => {
    if (state.status !== "ready" || !state.basket || state.basket.items.length === 0) {
      return
    }

    const controller = new AbortController()
    getInventoryItems(
      state.basket.items.map((item) => item.productId),
      controller.signal,
    )
      .then((items) => {
        setStockByProductId(
          Object.fromEntries(items.map((item) => [item.productId, item.availableQuantity])),
        )
      })
      .catch(() => undefined)

    return () => controller.abort()
  }, [state])


  async function updateQuantity(productId: string, quantity: number) {
    if (state.status !== "ready" || quantity < 1) return
    setBusyItemId(productId)
    setOperationError(null)

    try {
      const basket = await addBasketItem(state.profile.id, { productId, quantity })
      setState({ ...state, basket })
    } catch (error) {
      setOperationError(resolveOperationError(error))
    } finally {
      setBusyItemId(null)
    }
  }

  async function remove(productId: string) {
    if (state.status !== "ready") return
    setBusyItemId(productId)
    setOperationError(null)

    try {
      const basket = await removeBasketItem(state.profile.id, productId)
      setState({ ...state, basket: basket.items.length > 0 ? basket : null })
    } catch (error) {
      setOperationError(resolveOperationError(error))
    } finally {
      setBusyItemId(null)
    }
  }

  async function clear() {
    if (state.status !== "ready") return
    setClearing(true)
    setOperationError(null)

    try {
      await clearBasket(state.profile.id)
      setState({ ...state, basket: null })
    } catch (error) {
      setOperationError(resolveOperationError(error))
    } finally {
      setClearing(false)
    }
  }

  const stockExceeded =
    state.status === "ready" &&
    state.basket !== null &&
    state.basket.items.some((item) => {
      const available = stockByProductId[item.productId]
      return available !== undefined && item.quantity > available
    })

  async function checkout(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (state.status !== "ready" || !state.basket) return
    setCheckingOut(true)
    setOperationError(null)

    try {
      checkoutId.current ??= crypto.randomUUID()
      const result = await checkoutBasket(state.profile.id, {
        ...checkoutAddress,
        checkoutId: checkoutId.current,
      })
      setCheckoutResult(result)
      setState({ ...state, basket: null })
    } catch (error) {
      setOperationError(resolveOperationError(error))
    } finally {
      setCheckingOut(false)
    }
  }

  return (
    <CustomerShell>
      <main className="mx-auto w-full max-w-5xl px-4 py-8 sm:px-6 lg:px-8">
        <Link href="/" className={cn(buttonVariants({ variant: "ghost" }), "-ml-2")}>
          {t.basket.continueShopping}
        </Link>
        <div className="mt-6 flex items-start gap-3">
          <span className="flex size-11 items-center justify-center rounded-sm bg-primary text-primary-foreground">
            <ShoppingBagIcon className="size-5" />
          </span>
          <div>
            <h1 className="font-heading text-3xl font-semibold tracking-tight">{t.basket.title}</h1>
            <p className="mt-1 text-sm text-muted-foreground">{t.basket.description}</p>
          </div>
        </div>

        {state.status === "loading" ? (
          <div className="mt-8 grid gap-6 lg:grid-cols-[1fr_20rem]">
            <Skeleton className="h-72" />
            <Skeleton className="h-96" />
          </div>
        ) : state.status === "storeUnavailable" ? (
          <ErrorState className="mt-8" title={t.basket.temporarilyUnavailable} />
        ) : state.status === "unavailable" ? (
          <ErrorState className="mt-8" title={t.basket.loadFailed} />
        ) : checkoutResult ? (
          <div className="mt-8 border border-primary/30 bg-primary/5 p-8 text-center">
            <CheckCircle2Icon className="mx-auto size-10 text-primary" />
            <h2 className="mt-4 font-heading text-2xl font-semibold">{t.basket.checkoutRecorded}</h2>
            <p className="mx-auto mt-2 max-w-xl text-sm leading-6 text-muted-foreground">{t.basket.checkoutNote}</p>
            <p className="mt-4 text-sm font-medium">{formatMoney(checkoutResult.totalAmount, checkoutResult.currency, locale)}</p>
            <Link href={`/orders/${checkoutResult.snapshotId}`} className={cn(buttonVariants(), "mt-5")}>
              {t.basket.viewOrder}
            </Link>
          </div>
        ) : !state.basket || state.basket.items.length === 0 ? (
          <div className="mt-8 flex flex-col items-center gap-4">
            <EmptyState className="w-full" title={t.basket.empty} description="" />
            <Link href="/" className={buttonVariants({ variant: "outline" })}>
              {t.basket.continueShopping}
            </Link>
          </div>
        ) : (
          <div className="mt-8 grid gap-6 lg:grid-cols-[1fr_20rem]">
            <section className="divide-y divide-border overflow-hidden border border-border bg-card">
              {state.basket.items.map((item) => {
                const busy = busyItemId === item.productId
                const available = stockByProductId[item.productId]
                const exceedsStock = available !== undefined && item.quantity > available
                return (
                  <article key={item.productId} className="p-5">
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <h2 className="font-heading text-lg font-semibold">
                          {nameByProductId[item.productId] ?? item.productName}
                        </h2>
                        <p className="mt-1 text-sm text-muted-foreground">
                          {t.basket.unitPrice}: {formatMoney(item.unitPrice, item.currency, locale)}
                        </p>
                      </div>
                      <p className="font-heading text-lg font-semibold">{formatMoney(item.totalPrice, item.currency, locale)}</p>
                    </div>
                    <div className="mt-5 flex flex-wrap items-center justify-between gap-3">
                      <div className="flex items-center gap-2" aria-label={t.basket.quantity}>
                        <Button type="button" variant="outline" size="icon" disabled={busy || item.quantity <= 1} onClick={() => updateQuantity(item.productId, item.quantity - 1)} aria-label={`${t.basket.quantity} -`}><MinusIcon /></Button>
                        <span className="min-w-8 text-center text-sm font-medium">{busy ? <Loader2Icon className="mx-auto size-4 animate-spin" /> : item.quantity}</span>
                        <Button type="button" variant="outline" size="icon" disabled={busy || (available !== undefined && item.quantity >= available)} onClick={() => updateQuantity(item.productId, item.quantity + 1)} aria-label={`${t.basket.quantity} +`}><PlusIcon /></Button>
                      </div>
                      <Button type="button" variant="destructive" disabled={busy} onClick={() => remove(item.productId)}><Trash2Icon />{t.basket.remove}</Button>
                    </div>
                    {exceedsStock ? (
                      <p className="mt-3 text-sm text-destructive" role="status">
                        {available === 0
                          ? t.basket.outOfStock
                          : t.basket.stockLimited.replace("{count}", String(available))}
                      </p>
                    ) : null}
                  </article>
                )
              })}
            </section>

            <form onSubmit={checkout} className="h-fit border border-border bg-card p-5 lg:sticky lg:top-6">
              <div className="flex items-center justify-between gap-4">
                <span className="text-sm text-muted-foreground">{t.basket.total}</span>
                <strong className="font-heading text-2xl">{formatMoney(state.basket.totalAmount, state.basket.currency, locale)}</strong>
              </div>
              <div className="mt-6 grid gap-3">
                <CheckoutField
                  label={t.basket.recipientName}
                  value={checkoutAddress.recipientName}
                  onChange={(recipientName) => setCheckoutAddress((current) => ({ ...current, recipientName }))}
                />
                <CheckoutField
                  label={t.basket.addressLine}
                  value={checkoutAddress.addressLine}
                  onChange={(addressLine) => setCheckoutAddress((current) => ({ ...current, addressLine }))}
                />
                <div className="grid grid-cols-2 gap-3">
                  <CheckoutField
                    label={t.basket.city}
                    value={checkoutAddress.city}
                    onChange={(city) => setCheckoutAddress((current) => ({ ...current, city }))}
                  />
                  <CheckoutField
                    label={t.basket.postalCode}
                    value={checkoutAddress.postalCode}
                    onChange={(postalCode) => setCheckoutAddress((current) => ({ ...current, postalCode }))}
                  />
                </div>
                <CheckoutField
                  label={t.basket.countryCode}
                  value={checkoutAddress.countryCode}
                  maxLength={2}
                  onChange={(countryCode) => setCheckoutAddress((current) => ({
                    ...current,
                    countryCode: countryCode.toUpperCase(),
                  }))}
                />
              </div>
              <div className="mt-5 flex gap-3 rounded-lg border border-primary/20 bg-primary/5 p-3">
                <CreditCardIcon className="mt-0.5 size-4 shrink-0 text-primary" />
                <div>
                  <p className="text-sm font-medium">{t.basket.demoPayment}</p>
                  <p className="mt-1 text-xs leading-5 text-muted-foreground">{t.basket.demoPaymentNote}</p>
                </div>
              </div>
              {operationError ? (
                <p className="mt-4 text-sm text-destructive">
                  {operationError === "storeUnavailable" ? t.basket.temporarilyUnavailable : t.basket.updateFailed}
                </p>
              ) : null}
              <Button type="submit" size="lg" className="mt-6 w-full" disabled={checkingOut || clearing || stockExceeded}>
                {checkingOut ? <Loader2Icon className="animate-spin" /> : null}
                {checkingOut ? t.basket.checkingOut : t.basket.checkout}
              </Button>
              <Button type="button" variant="ghost" className="mt-2 w-full" disabled={checkingOut || clearing} onClick={clear}>
                {clearing ? <Loader2Icon className="animate-spin" /> : <Trash2Icon />}
                {t.basket.clear}
              </Button>
            </form>
          </div>
        )}
      </main>
    </CustomerShell>
  )
}

function CheckoutField({
  label,
  value,
  onChange,
  maxLength,
}: {
  label: string
  value: string
  onChange: (value: string) => void
  maxLength?: number
}) {
  return (
    <label className="grid gap-1.5 text-xs font-medium text-muted-foreground">
      {label}
      <Input
        value={value}
        onChange={(event) => onChange(event.target.value)}
        maxLength={maxLength}
        required
      />
    </label>
  )
}

function resolveOperationError(error: unknown): BasketOperationError {
  return error instanceof ApiError && error.status === 503 ? "storeUnavailable" : "generic"
}
