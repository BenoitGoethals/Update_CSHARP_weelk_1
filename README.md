# 📚 Book Repository

A small, cleanly-layered **console application** for managing a personal book
collection. Built as a study in **Clean Architecture**, **SOLID** design, and
modern **C# 14 / .NET 10** conventions — with two interchangeable storage
backends and a full unit-test suite.

---

## ✨ Features

- **Full CRUD** over a book catalogue: add, update, remove, and list books.
- **Pluggable persistence** — choose your storage backend at startup:
  - 📄 **JSON file** (`db/books.json`)
  - 🗄️ **SQLite database** (`db/books.db`)
- **Two-tier validation** — invalid data never reaches storage:
  - *Input level*: type-safe console readers re-prompt until each field is well-formed.
  - *Repository level*: every write is re-validated as a safety net, independent of the caller.
- **Async all the way** — non-blocking file and database I/O.
- **Command-pattern menu** — each menu action is an isolated, testable command.
- **65 unit tests** (xUnit) covering the domain, repository, and both stores.

---

## 🧱 Architecture

The code is organized into layers with dependencies pointing **inward** toward
the domain:

```
┌──────────────────────────────────────────────────────────┐
│  Presentation   Menu · Commands · ConsoleInput · Views    │
│                 (console I/O, input validation)           │
└───────────────┬──────────────────────────────────────────┘
                │ depends on IBookRepository / IBookValidator
┌───────────────▼──────────────────────────────────────────┐
│  Repository     BookRepository (orchestration + guards)   │
└───────────────┬──────────────────────────────────────────┘
                │ depends on IBookDataStore
┌───────────────▼──────────────────────────────────────────┐
│  Domain         Book · Genre · BookValidator (rules)      │
│                 IBookDataStore · IBookValidator (ports)   │
└───────────────▲──────────────────────────────────────────┘
                │ implements IBookDataStore
┌───────────────┴──────────────────────────────────────────┐
│  Persistence    DataStoreJson · DataStoreSqlite           │
└──────────────────────────────────────────────────────────┘
```

**Composition root** (`Program.cs`) wires the layers together — it picks the
data store, injects the validator, and starts the console loop. All
dependencies flow through interfaces (`IBookRepository`, `IBookDataStore`,
`IBookValidator`), so any piece can be swapped or mocked in isolation.

### Project layout

```
Update_CSHARP_weelk_1/
├── Domain/                 # Entities, enums, and abstractions (ports)
│   ├── Book.cs
│   ├── Genre.cs
│   ├── BookValidator.cs        # IBookValidator implementation
│   ├── IBookValidator.cs
│   └── IBookDataStore.cs
├── Repository/             # Persistence-agnostic orchestration
│   ├── BookRepository.cs
│   └── IBookRepository.cs
├── Persistence/           # Concrete storage adapters
│   ├── DataStoreJson.cs
│   └── DataStoreSqlite.cs
├── Presentation/          # Console UI, commands, and input handling
│   ├── ConsoleApp.cs
│   ├── Menu.cs
│   ├── Commands.cs
│   ├── ConsoleInput.cs
│   ├── BookConsoleView.cs
│   └── DataStoreSelector.cs
└── Program.cs             # Composition root / entry point

Update_CSHARP_weelk_1.Tests/   # xUnit test suite
```

---

## 🚀 Getting started

### Prerequisites

- [.NET SDK **10.0**](https://dotnet.microsoft.com/download) or later

### Run the app

```bash
dotnet run --project Update_CSHARP_weelk_1
```

On launch you'll be asked which storage backend to use, then presented with the
menu:

```
=== Select Data Store ===
1. JSON file
2. SQLite database

=== Book Repository Menu ===
1. Add a new book
2. Update an existing book
3. Remove a book by ISBN
4. List all books
5. Exit application
```

### Run the tests

```bash
dotnet test
```

### Build

```bash
dotnet build
```

---

## 🧪 Validation rules

A book is considered valid when:

| Field   | Rule                                              |
|---------|---------------------------------------------------|
| ISBN    | Required, non-blank, unique                       |
| Title   | Required, non-blank                               |
| Author  | Required, non-blank                               |
| Year    | Between `1` and the current year                  |
| Genre   | One of the defined `Genre` values                 |

The console readers enforce **type correctness** as you type (e.g. a
non-numeric year is rejected and re-prompted), and `BookValidator` enforces the
**business rules** at the repository boundary.

---

## 🛠️ Tech stack

- **C# 14** on **.NET 10**
- **System.Text.Json** — JSON persistence
- **Microsoft.Data.Sqlite** — SQLite persistence
- **xUnit** — testing
- Analyzers enabled (`latest-recommended`) for style and quality enforcement

---

## 📄 License

Released under the [MIT License](LICENSE) © 2026 Benoit Goethals.
