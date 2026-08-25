(function () {
  try {
    var stored = localStorage.getItem("app.theme")
    var theme =
      stored === "light" || stored === "dark"
        ? stored
        : window.matchMedia("(prefers-color-scheme: dark)").matches
          ? "dark"
          : "light"
    var root = document.documentElement
    root.classList.add(theme)
    root.style.colorScheme = theme
  } catch {}
})()
