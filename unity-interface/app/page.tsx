// Removed Tabs imports as they are no longer needed
import SimulationSetup from "./simulation-setup"
// Removed SimulationResults import

export default function Home() {
  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <header className="border-b bg-white dark:bg-gray-950 dark:border-gray-800">
        <div className="container flex h-16 items-center px-4 sm:px-6 lg:px-8">
          <h1 className="text-xl font-bold">Unity Simulation Interface</h1>
          <div className="ml-auto flex items-center space-x-4">
            <span className="text-sm text-muted-foreground">Project: Plane Simulation</span>
          </div>
        </div>
      </header>
      {/* Removed Tabs wrapper, directly rendering SimulationSetup */}
      <main className="container px-4 py-6 sm:px-6 lg:px-8">
        <SimulationSetup />
      </main>
      <footer className="border-t bg-white dark:bg-gray-950 dark:border-gray-800">
        <div className="container flex h-10 items-center px-4 sm:px-6 lg:px-8">
          <span className="text-sm text-muted-foreground">Status: Ready</span>
          <span className="ml-auto text-sm text-muted-foreground">v1.0.0</span>
        </div>
      </footer>
    </div>
  )
}
