export type Workspace = "Admin" | "Seller" | "Customer"

const WORKSPACE_STORAGE_KEY = "app.workspace"
const workspaces: readonly Workspace[] = ["Admin", "Seller", "Customer"]

export function getStoredWorkspace(): Workspace | null {
  try {
    const stored = window.localStorage.getItem(WORKSPACE_STORAGE_KEY)
    return workspaces.find((workspace) => workspace === stored) ?? null
  } catch {
    return null
  }
}

export function storeWorkspace(workspace: Workspace): void {
  try {
    window.localStorage.setItem(WORKSPACE_STORAGE_KEY, workspace)
  } catch {
    return
  }
}
