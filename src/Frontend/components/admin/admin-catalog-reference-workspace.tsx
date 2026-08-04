"use client"

import Link from "next/link"
import { ArrowRight, Tags } from "lucide-react"

import { buttonVariants } from "@/components/ui/button"
import { useI18n } from "@/lib/i18n/provider"
import { cn } from "@/lib/utils"

export function AdminCatalogReferenceWorkspace() {
  const { t } = useI18n()

  return (
    <section
      id="admin-catalog-references"
      className="mt-7"
      aria-labelledby="admin-catalog-references-title"
    >
      <div>
        <h2
          id="admin-catalog-references-title"
          className="font-heading text-xl font-semibold text-foreground"
        >
          {t.admin.manageReferences}
        </h2>
        <p className="mt-1 text-sm text-muted-foreground">
          {t.admin.manageReferencesDescription}
        </p>
      </div>

      <div className="mt-5 grid gap-4 md:grid-cols-2">
        <ReferenceLink
          href="/admin/categories"
          title={t.admin.categories}
          description={t.admin.manageCategoriesDescription}
          action={t.admin.openCategoryManagement}
        />
        <ReferenceLink
          href="/admin/brands"
          title={t.admin.brands}
          description={t.admin.manageBrandsDescription}
          action={t.admin.openBrandManagement}
        />
      </div>
    </section>
  )
}

function ReferenceLink({
  href,
  title,
  description,
  action,
}: {
  href: string
  title: string
  description: string
  action: string
}) {
  return (
    <article className="flex min-h-44 flex-col rounded-lg border border-border bg-background p-5">
      <span className="flex size-10 items-center justify-center rounded-lg bg-primary/10 text-primary">
        <Tags className="size-5" />
      </span>
      <h3 className="mt-4 font-heading text-lg font-semibold text-foreground">
        {title}
      </h3>
      <p className="mt-1 flex-1 text-sm leading-6 text-muted-foreground">
        {description}
      </p>
      <Link
        href={href}
        className={cn(buttonVariants({ variant: "outline" }), "mt-4 w-fit")}
      >
        {action}
        <ArrowRight />
      </Link>
    </article>
  )
}
