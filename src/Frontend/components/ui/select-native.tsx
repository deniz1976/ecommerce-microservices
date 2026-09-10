import { ChevronDownIcon } from "lucide-react"

import { cn } from "@/lib/utils"

/**
 * Native select styled with the same tokens as `Input`. Used for filter rows
 * where the richer `Select` primitive would add no value.
 */
function SelectNative({ className, children, ...props }: React.ComponentProps<"select">) {
  return (
    <div className="relative">
      <select
        data-slot="select-native"
        className={cn(
          "h-8 w-full appearance-none rounded-lg border border-border bg-background py-1 pr-8 pl-2.5 text-sm font-normal text-foreground outline-none transition-all focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 disabled:pointer-events-none disabled:opacity-50 dark:bg-input/30",
          className,
        )}
        {...props}
      >
        {children}
      </select>
      <ChevronDownIcon
        className="pointer-events-none absolute top-1/2 right-2.5 size-3.5 -translate-y-1/2 text-muted-foreground"
        aria-hidden="true"
      />
    </div>
  )
}

export { SelectNative }
