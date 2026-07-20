"use client"

import { useRouter } from "next/navigation"
import { useEffect, useState } from "react"
import { Info, Loader2 } from "lucide-react"

import { FormError } from "@/components/auth/form-error"
import { LoadingButton } from "@/components/auth/loading-button"
import { RoleSelector } from "@/components/auth/role-selector"
import { getProfile, updateRole } from "@/lib/api/auth"
import { refreshAccessToken } from "@/lib/auth/auth0"
import { useI18n } from "@/lib/i18n/provider"
import type { PublicRole } from "@/types"

export function RoleOnboarding() {
  const { t } = useI18n()
  const router = useRouter()

  const [role, setRole] = useState<PublicRole | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [formError, setFormError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [checkingProfile, setCheckingProfile] = useState(true)

  useEffect(() => {
    let active = true

    getProfile()
      .then((profile) => {
        if (!active) return

        if (profile.isOnboardingComplete) {
          router.replace("/")
          return
        }

        setCheckingProfile(false)
      })
      .catch(() => {
        if (active) router.replace("/login")
      })

    return () => {
      active = false
    }
  }, [router])

  async function handleConfirm() {
    setFormError(null)
    if (!role) {
      setError(t.validation.selectRole)
      return
    }
    setError(null)
    setSubmitting(true)
    try {
      await updateRole({ role })
      await refreshAccessToken()
      router.push("/")
    } catch {
      setFormError(t.errors.roleUpdateFailed)
      setSubmitting(false)
    }
  }

  if (checkingProfile) {
    return (
      <div className="flex justify-center py-8">
        <Loader2 className="size-5 animate-spin text-muted-foreground" />
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-5">
      <FormError message={formError} />

      <RoleSelector value={role} onChange={setRole} error={error} />

      <div className="flex items-start gap-2 rounded-lg border border-border bg-muted/50 px-3 py-2.5 text-xs leading-relaxed text-muted-foreground">
        <Info className="mt-0.5 size-4 shrink-0" aria-hidden="true" />
        <span>{t.onboarding.note}</span>
      </div>

      <LoadingButton
        type="button"
        size="lg"
        loading={submitting}
        loadingText={t.onboarding.savingRole}
        onClick={handleConfirm}
        className="h-11 w-full"
      >
        {t.onboarding.confirmRole}
      </LoadingButton>
    </div>
  )
}
