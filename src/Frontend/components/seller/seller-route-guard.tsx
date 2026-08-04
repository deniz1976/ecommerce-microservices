"use client"

import { Loader2 } from "lucide-react"
import { useRouter } from "next/navigation"
import type { ReactNode } from "react"
import { useEffect, useState } from "react"

import { getProfile } from "@/lib/api/auth"
import { useI18n } from "@/lib/i18n/provider"

type AccessState = "checking" | "allowed" | "redirecting"

export function SellerRouteGuard({ children }: { children: ReactNode }) {
  const { t } = useI18n()
  const router = useRouter()
  const [state, setState] = useState<AccessState>("checking")

  useEffect(() => {
    let active = true

    getProfile()
      .then((profile) => {
        if (!active) return
        if (!profile.roles.some((role) => role === "Seller" || role === "Admin")) {
          setState("redirecting")
          router.replace("/")
          return
        }

        setState("allowed")
      })
      .catch(() => {
        if (!active) return
        setState("redirecting")
        router.replace("/login")
      })

    return () => {
      active = false
    }
  }, [router])

  if (state !== "allowed") {
    return (
      <main className="flex min-h-svh items-center justify-center">
        <Loader2 className="size-6 animate-spin text-muted-foreground" />
        <span className="sr-only">{t.common.loading}</span>
      </main>
    )
  }

  return children
}
