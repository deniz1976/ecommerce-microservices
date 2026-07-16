"use client"

import Link from "next/link"

import { AuthButton } from "@/components/auth/auth-button"
import { useI18n } from "@/lib/i18n/provider"

export function RegisterForm() {
  const { t } = useI18n()

  return (
    <div className="flex flex-col gap-5">
      <AuthButton
        label={t.register.continueWithAuth0}
        loadingText={t.common.loading}
        options={{ returnTo: "/onboarding/role", screenHint: "signup" }}
      />

      <p className="text-center text-xs leading-relaxed text-muted-foreground">
        {t.register.auth0Note}
      </p>

      <p className="text-center text-sm text-muted-foreground">
        {t.register.haveAccount}{" "}
        <Link
          href="/login"
          className="font-medium text-primary underline-offset-4 hover:underline"
        >
          {t.register.loginLink}
        </Link>
      </p>
    </div>
  )
}
