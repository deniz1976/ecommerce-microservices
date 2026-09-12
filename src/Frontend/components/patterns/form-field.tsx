import type { ComponentProps, ReactNode } from "react"

import { Input } from "@/components/ui/input"
import { SelectNative } from "@/components/ui/select-native"

import { cn } from "@/lib/utils"

interface FormFieldProps {
  label: string
  children: ReactNode
  hint?: string
  error?: string | null
  className?: string
}

export function FormField({ label, children, hint, error, className }: FormFieldProps) {
  return (
    <label className={cn("grid min-w-0 gap-1.5 text-sm font-medium", className)}>
      {label}
      {children}
      {hint ? <span className="text-xs font-normal text-muted-foreground">{hint}</span> : null}
      {error ? (
        <span className="text-xs font-normal text-destructive" role="alert">
          {error}
        </span>
      ) : null}
    </label>
  )
}

interface TextFieldProps extends Omit<ComponentProps<"input">, "onChange"> {
  label: string
  value: string
  onChange: (value: string) => void
  hint?: string
}

export function TextField({ label, value, onChange, hint, ...inputProps }: TextFieldProps) {
  return (
    <FormField label={label} hint={hint}>
      <Input value={value} onChange={(event) => onChange(event.target.value)} {...inputProps} />
    </FormField>
  )
}

interface SelectFieldProps {
  label: string
  value: string
  onChange: (value: string) => void
  options: Array<{ id: string; name: string }>
  hint?: string
  className?: string
}

export function SelectField({ label, value, onChange, options, hint, className }: SelectFieldProps) {
  return (
    <FormField label={label} hint={hint} className={className}>
      <SelectNative value={value} onChange={(event) => onChange(event.target.value)}>
        {options.map((option) => (
          <option key={option.id} value={option.id}>
            {option.name}
          </option>
        ))}
      </SelectNative>
    </FormField>
  )
}
