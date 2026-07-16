"use client"

import { AuthLayout } from "@/components/auth/auth-layout"
import { LoginForm } from "@/components/auth/login-form"
import { useI18n } from "@/lib/i18n/provider"

export default function LoginPage() {
  const { t } = useI18n()

  return (
    <AuthLayout title={t.login.title} subtitle={t.login.subtitle}>
      <LoginForm />
    </AuthLayout>
  )
}
