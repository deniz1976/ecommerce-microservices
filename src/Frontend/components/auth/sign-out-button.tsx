"use client"

import { Loader2Icon, LogOutIcon } from "lucide-react"
import { useState } from "react"

import { Button } from "@/components/ui/button"
import { logoutFromAuth0 } from "@/lib/auth/auth0"
import { useI18n } from "@/lib/i18n/provider"

export function SignOutButton() {
  const { t } = useI18n()
  const [signingOut, setSigningOut] = useState(false)

  async function handleSignOut() {
    setSigningOut(true)
    await logoutFromAuth0("/login")
  }

  return (
    <Button
      type="button"
      variant="outline"
      size="sm"
      onClick={handleSignOut}
      disabled={signingOut}
    >
      {signingOut ? <Loader2Icon className="animate-spin" /> : <LogOutIcon />}
      <span className="hidden sm:inline">{t.home.signOut}</span>
    </Button>
  )
}
