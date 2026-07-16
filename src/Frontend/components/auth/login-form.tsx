"use client"

import Link from "next/link"

import { AuthButton } from "@/components/auth/auth-button"
import { useI18n } from "@/lib/i18n/provider"

export function LoginForm() {
  const { t } = useI18n()

  return (
    <div className="flex flex-col gap-5">
      <AuthButton
        label={t.login.continueWithAuth0}
        loadingText={t.common.loading}
        options={{ returnTo: "/", screenHint: "login" }}
      />

      <p className="text-center text-sm text-muted-foreground">
        {t.login.noAccount}{" "}
        <Link
          href="/register"
          className="font-medium text-primary underline-offset-4 hover:underline"
        >
          {t.login.registerLink}
        </Link>
      </p>
    </div>
  )
}
