"use client"

import { Loader2 } from "lucide-react"

import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"

type ButtonProps = React.ComponentProps<typeof Button>

interface LoadingButtonProps extends ButtonProps {
  loading?: boolean
  loadingText?: string
}

export function LoadingButton({
  loading = false,
  loadingText,
  disabled,
  children,
  className,
  ...props
}: LoadingButtonProps) {
  return (
    <Button
      disabled={loading || disabled}
      aria-busy={loading}
      className={cn(className)}
      {...props}
    >
      {loading ? (
        <>
          <Loader2 className="size-4 animate-spin" aria-hidden="true" />
          {loadingText ?? children}
        </>
      ) : (
        children
      )}
    </Button>
  )
}
