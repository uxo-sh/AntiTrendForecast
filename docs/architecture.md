# Anti-Trend Forecast Engine — Architecture

## Overview

The application uses a **3-layer architecture** where layers communicate only downward.

```
┌─────────────────────────────────────────┐
│  Layer 1 — DIRECTIVE (Intent)           │
│  WinUI 3 Dashboard + Input Validation   │
│  Namespace: AntiTrendForecast.Directive   │
├─────────────────────────────────────────┤
│  Layer 2 — ORCHESTRATION (Decisions)    │
│  Pipeline, Fatigue Index, Pivot Score   │
│  Namespace: AntiTrendForecast.Orchestration│
├─────────────────────────────────────────┤
│  Layer 3 — EXECUTION (Doing the work)   │
│  Python Interop, SQLite, Web Scrapers   │
│  Namespace: AntiTrendForecast.Execution   │
└─────────────────────────────────────────┘
```

## Dependency Direction

```
Directive → Orchestration → Execution
```

- **Directive** depends on Orchestration (never on Execution directly).
- **Orchestration** depends on Execution.
- **Execution** has no internal project dependencies.

## Key Components

| Layer | Component | Responsibility |
|-------|-----------|----------------|
| Directive | `InputValidator` | Validates keyword and timeframe input |
| Directive | `DashboardViewModel` | MVVM view-model for the UI |
| Orchestration | `PipelineOrchestrator` | Coordinates full analysis workflow |
| Orchestration | `PivotScoreCalculator` | Calculates the Pivot Score |
| Orchestration | `FatigueIndexAnalyzer` | Computes market fatigue index |
| Execution | `PythonRunner` | Launches Python scripts |
| Execution | `TrendRepository` | SQLite data access |
| Execution | `DatabaseInitializer` | Creates database schema |

## Tech Stack

- **UI:** WinUI 3 (Windows App SDK)
- **Logic:** C# / .NET 10
- **Processing:** Python (Scrapy & PyTorch) via `PythonRunner`
- **Database:** SQLite via `Microsoft.Data.Sqlite`
- **Logging:** Serilog
