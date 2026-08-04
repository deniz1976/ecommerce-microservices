"use client"

import type { ChangeEventHandler } from "react"

import { ApiError } from "@/lib/api/client"

export function ReferenceTextField({
  label,
  value,
  onChange,
  hint,
  slug = false,
}: {
  label: string
  value: string
  onChange: (value: string) => void
  hint?: string
  slug?: boolean
}) {
  return (
    <label className="grid min-w-0 gap-2 text-sm font-medium text-foreground">
      {label}
      <input
        required
        minLength={2}
        maxLength={slug ? 160 : 256}
        pattern={slug ? "[a-z0-9]+(?:-[a-z0-9]+)*" : undefined}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="h-10 w-full min-w-0 rounded-md border border-input bg-background px-3 font-normal"
      />
      {hint ? (
        <span className="text-xs font-normal text-muted-foreground">{hint}</span>
      ) : null}
    </label>
  )
}

export function ReferenceActiveField({
  checked,
  onChange,
  label,
}: {
  checked: boolean
  onChange: (checked: boolean) => void
  label: string
}) {
  const handleChange: ChangeEventHandler<HTMLInputElement> = (event) => {
    onChange(event.target.checked)
  }

  return (
    <label className="flex items-center gap-2 text-sm font-medium text-foreground">
      <input
        type="checkbox"
        checked={checked}
        onChange={handleChange}
        className="size-4 rounded border-input"
      />
      {label}
    </label>
  )
}

export function resolveReferenceApiMessage(error: unknown, fallback: string) {
  if (!(error instanceof ApiError) || !error.data || typeof error.data !== "object") {
    return fallback
  }

  const message = Reflect.get(error.data, "message")
  return typeof message === "string" && message.trim() ? message : fallback
}
