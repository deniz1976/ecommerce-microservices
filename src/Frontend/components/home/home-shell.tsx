"use client"

import { useRouter } from "next/navigation"
import { useEffect, useState } from "react"
import { CheckCircle2Icon, Loader2Icon, LogOutIcon } from "lucide-react"

import { AdminDashboard } from "@/components/admin/admin-dashboard"
import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { LoadingButton } from "@/components/auth/loading-button"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { SellerDashboard } from "@/components/seller/seller-dashboard"
import { CustomerDashboard } from "@/components/customer/customer-dashboard"
import { getProfile } from "@/lib/api/auth"
import { logoutFromAuth0 } from "@/lib/auth/auth0"
import { useI18n } from "@/lib/i18n/provider"
import { getStoredWorkspace, storeWorkspace, type Workspace } from "@/lib/workspace/preference"
import type { UserProfile } from "@/types"

type AuthState =
  | { status: "loading" }
  | { status: "unauthenticated" }
  | { status: "authenticated"; profile: UserProfile }

export function HomeShell() {
  const { t } = useI18n()
  const router = useRouter()
  const [state, setState] = useState<AuthState>({ status: "loading" })
  const [selectedWorkspace, setSelectedWorkspace] = useState<Workspace | null>(null)
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

        setSelectedWorkspace(getStoredWorkspace())
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
        <Loader2Icon className="size-6 animate-spin text-muted-foreground" />
        <span className="sr-only">{t.common.loading}</span>
      </main>
    )
  }

  if (state.status === "unauthenticated") {
    return <CustomerDashboard />
  }

  const { profile } = state

  function openWorkspace(workspace: Workspace) {
    storeWorkspace(workspace)
    setSelectedWorkspace(workspace)
  }
  const activeWorkspace =
    selectedWorkspace && profile.roles.includes(selectedWorkspace)
      ? selectedWorkspace
      : profile.roles.includes("Admin")
        ? "Admin"
        : profile.roles.includes("Seller")
          ? "Seller"
          : profile.roles.includes("Customer")
            ? "Customer"
            : null

  if (activeWorkspace === "Admin") {
    return (
      <AdminDashboard
        profile={profile}
        onOpenSellerWorkspace={profile.roles.includes("Seller")
          ? () => openWorkspace("Seller")
          : undefined}
      />
    )
  }

  if (activeWorkspace === "Seller") {
    return (
      <SellerDashboard
        profile={profile}
        onOpenAdminWorkspace={profile.roles.includes("Admin")
          ? () => openWorkspace("Admin")
          : undefined}
      />
    )
  }

  if (activeWorkspace === "Customer") {
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
        <div className="w-full max-w-lg border border-border bg-card p-8">
          <span className="flex size-12 items-center justify-center rounded-lg bg-accent text-accent-foreground">
            <CheckCircle2Icon className="size-6" />
          </span>
          <h1 className="mt-5 font-heading text-2xl font-semibold tracking-tight">
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
            <LogOutIcon className="size-4" />
            {t.home.signOut}
          </LoadingButton>
        </div>
      </main>
    </div>
  )
}
