"use client"

import { AuthLayout } from "@/components/auth/auth-layout"
import { RegisterForm } from "@/components/auth/register-form"
import { useI18n } from "@/lib/i18n/provider"

export default function RegisterPage() {
  const { t } = useI18n()

  return (
    <AuthLayout title={t.register.title} subtitle={t.register.subtitle}>
      <RegisterForm />
    </AuthLayout>
  )
}
