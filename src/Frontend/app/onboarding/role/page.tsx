"use client"

import { AuthLayout } from "@/components/auth/auth-layout"
import { RoleOnboarding } from "@/components/auth/role-onboarding"
import { useI18n } from "@/lib/i18n/provider"

export default function RoleOnboardingPage() {
  const { t } = useI18n()

  return (
    <AuthLayout title={t.onboarding.title} subtitle={t.onboarding.subtitle}>
      <RoleOnboarding />
    </AuthLayout>
  )
}
