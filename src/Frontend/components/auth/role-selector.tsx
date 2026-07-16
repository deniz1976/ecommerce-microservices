"use client"

import { Check, ShoppingBag, Store } from "lucide-react"

import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"
import { PUBLIC_ROLES, type PublicRole } from "@/types"

interface RoleSelectorProps {
  value: PublicRole | null
  onChange: (role: PublicRole) => void
  error?: string | null
}

const ROLE_ICONS: Record<PublicRole, React.ComponentType<{ className?: string }>> = {
  Customer: ShoppingBag,
  Seller: Store,
}

export function RoleSelector({ value, onChange, error }: RoleSelectorProps) {
  const { t } = useI18n()

  return (
    <div
      role="radiogroup"
      aria-label={t.register.chooseRole}
      aria-invalid={Boolean(error)}
      className="grid gap-3 sm:grid-cols-2"
    >
      {PUBLIC_ROLES.map((role) => {
        const Icon = ROLE_ICONS[role]
        const copy = role === "Customer" ? t.roles.customer : t.roles.seller
        const selected = value === role

        return (
          <button
            key={role}
            type="button"
            role="radio"
            aria-checked={selected}
            onClick={() => onChange(role)}
            className={cn(
              "group relative flex flex-col gap-3 rounded-lg border p-4 text-left transition-all",
              "focus-visible:outline-none focus-visible:ring-3 focus-visible:ring-ring/25",
              selected
                ? "border-primary bg-accent/60 shadow-sm ring-1 ring-primary/40"
                : "border-border bg-card hover:border-primary/40 hover:bg-accent/30",
            )}
          >
            <span
              aria-hidden="true"
              className={cn(
                "absolute right-3 top-3 flex size-5 items-center justify-center rounded-full border transition-all",
                selected
                  ? "border-primary bg-primary text-primary-foreground"
                  : "border-border bg-background text-transparent",
              )}
            >
              <Check className="size-3" />
            </span>

            <span
              className={cn(
                "flex size-10 items-center justify-center rounded-lg transition-colors",
                selected
                  ? "bg-primary text-primary-foreground"
                  : "bg-muted text-muted-foreground group-hover:text-foreground",
              )}
            >
              <Icon className="size-5" />
            </span>

            <span className="flex flex-col gap-1">
              <span className="font-heading text-sm font-semibold text-foreground">
                {copy.name}
              </span>
              <span className="text-xs leading-relaxed text-muted-foreground">
                {copy.description}
              </span>
            </span>

            <ul className="mt-1 flex flex-col gap-1.5">
              {[copy.feature1, copy.feature2, copy.feature3].map((feature) => (
                <li
                  key={feature}
                  className="flex items-center gap-2 text-xs text-muted-foreground"
                >
                  <Check
                    className={cn(
                      "size-3.5 shrink-0",
                      selected ? "text-primary" : "text-muted-foreground/60",
                    )}
                    aria-hidden="true"
                  />
                  {feature}
                </li>
              ))}
            </ul>
          </button>
        )
      })}

      {error ? (
        <p className="text-xs font-medium text-destructive sm:col-span-2">
          {error}
        </p>
      ) : null}
    </div>
  )
}
