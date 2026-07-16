"use client"

import { Check, ShieldCheck, Zap } from "lucide-react"

import { LanguageSwitcher } from "@/components/auth/language-switcher"
import { Logo } from "@/components/auth/logo"
import { ThemeToggle } from "@/components/auth/theme-toggle"
import { useI18n } from "@/lib/i18n/provider"

interface AuthLayoutProps {
  title: string
  subtitle: string
  children: React.ReactNode
  footer?: React.ReactNode
}

export function AuthLayout({
  title,
  subtitle,
  children,
  footer,
}: AuthLayoutProps) {
  const { t } = useI18n()

  const points = [
    { icon: ShieldCheck, text: t.aside.point1 },
    { icon: Zap, text: t.aside.point2 },
    { icon: Check, text: t.aside.point3 },
  ]

  return (
    <div className="flex min-h-svh flex-col lg:grid lg:grid-cols-[1.05fr_1fr]">
      <aside className="relative hidden flex-col justify-between overflow-hidden bg-sidebar p-10 lg:flex xl:p-14">
        <Logo showTagline />

        <div className="max-w-md">
          <h2 className="font-heading text-3xl font-semibold tracking-tight text-foreground text-balance xl:text-4xl">
            {t.aside.trustTitle}
          </h2>
          <p className="mt-4 text-base leading-relaxed text-muted-foreground text-pretty">
            {t.aside.trustBody}
          </p>

          <ul className="mt-8 flex flex-col gap-4">
            {points.map(({ icon: Icon, text }) => (
              <li key={text} className="flex items-center gap-3">
                <span className="flex size-9 items-center justify-center rounded-lg bg-accent text-accent-foreground">
                  <Icon className="size-4" />
                </span>
                <span className="text-sm text-foreground">{text}</span>
              </li>
            ))}
          </ul>
        </div>

        <p className="text-xs text-muted-foreground">
          © {new Date().getFullYear()} {t.brand.name}
        </p>
      </aside>

      <div className="flex flex-1 flex-col">
        <header className="flex items-center justify-between gap-3 p-5 sm:p-6">
          <div className="lg:hidden">
            <Logo />
          </div>
          <div className="ml-auto flex items-center gap-2">
            <LanguageSwitcher />
            <ThemeToggle />
          </div>
        </header>

        <main className="flex flex-1 items-center justify-center px-5 pb-10 sm:px-6">
          <div className="w-full max-w-md">
            <div className="mb-6 flex flex-col gap-1.5">
              <h1 className="font-heading text-2xl font-semibold tracking-tight text-foreground text-balance sm:text-3xl">
                {title}
              </h1>
              <p className="text-sm leading-relaxed text-muted-foreground text-pretty">
                {subtitle}
              </p>
            </div>

            {children}

            {footer ? <div className="mt-6">{footer}</div> : null}
          </div>
        </main>
      </div>
    </div>
  )
}
