"use client"

import { useState } from "react"
import { ShieldCheck } from "lucide-react"

import { LoadingButton } from "@/components/auth/loading-button"
import { loginWithAuth0, type LoginOptions } from "@/lib/auth/auth0"
import { cn } from "@/lib/utils"

interface AuthButtonProps {
  label: string
  loadingText?: string
  options?: LoginOptions
  className?: string
}

export function AuthButton({
  label,
  loadingText,
  options,
  className,
}: AuthButtonProps) {
  const [loading, setLoading] = useState(false)

  async function handleClick() {
    setLoading(true)
    try {
      await loginWithAuth0(options)
    } catch {
      setLoading(false)
    }
  }

  return (
    <LoadingButton
      type="button"
      size="lg"
      loading={loading}
      loadingText={loadingText}
      onClick={handleClick}
      className={cn("h-11 w-full text-sm", className)}
    >
      <ShieldCheck className="size-4" aria-hidden="true" />
      {label}
    </LoadingButton>
  )
}
