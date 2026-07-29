"use client"

import Link from "next/link"
import { useRouter } from "next/navigation"
import { useEffect, useState } from "react"
import { CheckCircle2, Loader2, LogOut } from "lucide-react"

import { AdminDashboard } from "@/components/admin/admin-dashboard"
import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { LoadingButton } from "@/components/auth/loading-button"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { SellerDashboard } from "@/components/seller/seller-dashboard"
import { CustomerDashboard } from "@/components/customer/customer-dashboard"
import { buttonVariants } from "@/components/ui/button"
import { getProfile } from "@/lib/api/auth"
import { logoutFromAuth0 } from "@/lib/auth/auth0"
import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"
import type { UserProfile } from "@/types"

type AuthState =
  | { status: "loading" }
  | { status: "unauthenticated" }
  | { status: "authenticated"; profile: UserProfile }

export function HomeShell() {
  const { t } = useI18n()
  const router = useRouter()
  const [state, setState] = useState<AuthState>({ status: "loading" })
  const [signingOut, setSigningOut] = useState(false)

  useEffect(() => {
    let active = true
    getProfile()
      .then((profile) => {
        if (!active) return

        if (!profile.isOnboardingComplete) {
          router.replace("/onboarding/role")
          return
        }

        setState({ status: "authenticated", profile })
      })
      .catch(() => {
        if (active) setState({ status: "unauthenticated" })
      })
    return () => {
      active = false
    }
  }, [router])

  if (state.status === "loading") {
    return (
      <main className="flex min-h-svh items-center justify-center">
        <Loader2 className="size-6 animate-spin text-muted-foreground" />
        <span className="sr-only">{t.common.loading}</span>
      </main>
    )
  }

  if (state.status === "unauthenticated") {
    return <AuthEntry />
  }

  const { profile } = state

  if (profile.roles.includes("Admin")) {
    return <AdminDashboard profile={profile} />
  }

  if (profile.roles.includes("Seller")) {
    return <SellerDashboard profile={profile} />
  }

  if (profile.roles.includes("Customer")) {
    return <CustomerDashboard profile={profile} />
  }

  async function handleSignOut() {
    setSigningOut(true)
    await logoutFromAuth0("/login")
  }

  return (
    <div className="flex min-h-svh flex-col">
      <header className="flex items-center justify-between gap-3 border-b border-border p-5 sm:px-8">
        <Logo />
        <div className="flex items-center gap-2">
          <LanguageSwitcher />
          <ThemeToggle />
        </div>
      </header>

      <main className="flex flex-1 items-center justify-center px-5 py-12 sm:px-6">
        <div className="w-full max-w-lg rounded-lg border border-border bg-card p-8 shadow-sm">
          <span className="flex size-12 items-center justify-center rounded-lg bg-accent text-accent-foreground">
            <CheckCircle2 className="size-6" />
          </span>
          <h1 className="mt-5 font-heading text-2xl font-semibold tracking-tight text-foreground">
            {t.home.welcome}
          </h1>
          <p className="mt-2 text-sm leading-relaxed text-muted-foreground text-pretty">
            {t.home.placeholder}
          </p>

          <dl className="mt-6 grid gap-3 rounded-lg border border-border bg-muted/40 p-4 text-sm">
            <div className="flex items-center justify-between gap-4">
              <dt className="text-muted-foreground">{t.home.signedInAs}</dt>
              <dd className="font-medium text-foreground">
                {profile.displayName || profile.email}
              </dd>
            </div>
            {profile.roles.length > 0 ? (
              <div className="flex items-center justify-between gap-4">
                <dt className="text-muted-foreground">{t.home.role}</dt>
                <dd className="font-medium text-foreground">
                  {profile.roles.join(", ")}
                </dd>
              </div>
            ) : null}
          </dl>

          <LoadingButton
            type="button"
            variant="outline"
            size="lg"
            loading={signingOut}
            loadingText={t.common.loading}
            onClick={handleSignOut}
            className="mt-6 h-11 w-full"
          >
            <LogOut className="size-4" />
            {t.home.signOut}
          </LoadingButton>
        </div>
      </main>
    </div>
  )
}

function AuthEntry() {
  const { t } = useI18n()

  return (
    <div className="flex min-h-svh flex-col">
      <header className="flex items-center justify-between gap-3 p-5 sm:px-8">
        <Logo showTagline />
        <div className="flex items-center gap-2">
          <LanguageSwitcher />
          <ThemeToggle />
        </div>
      </header>

      <main className="flex flex-1 items-center justify-center px-5 pb-16 sm:px-6">
        <div className="w-full max-w-md text-center">
          <h1 className="font-heading text-3xl font-semibold tracking-tight text-foreground text-balance sm:text-4xl">
            {t.aside.trustTitle}
          </h1>
          <p className="mx-auto mt-4 max-w-sm text-sm leading-relaxed text-muted-foreground text-pretty">
            {t.aside.trustBody}
          </p>

          <div className="mt-8 flex flex-col gap-3">
            <Link
              href="/login"
              className={cn(buttonVariants({ size: "lg" }), "h-11 w-full")}
            >
              {t.login.signInCta}
            </Link>
            <Link
              href="/register"
              className={cn(
                buttonVariants({ variant: "outline", size: "lg" }),
                "h-11 w-full",
              )}
            >
              {t.register.createAccount}
            </Link>
          </div>
        </div>
      </main>
    </div>
  )
}
