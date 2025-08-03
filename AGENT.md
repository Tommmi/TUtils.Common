# AGENT.md - TUtils.Common Development Guide

## Build/Test Commands
- **Build**: `dotnet build source\TUtils3.sln` or `msbuild source\TUtils3.sln` or open in Visual Studio
- **Run all tests**: `dotnet test source\TUtils.Common.Test\TUtils.Common.Test.csproj` or use Visual Studio Test Explorer
- **Run single test**: Use Test Explorer in Visual Studio or `dotnet test --filter "TestAsyncEvent1"` (method name) or `dotnet test --filter "AsyncEventTest"` (class name)
- **Target Framework**: .NET 8.0 (migrated from .NET Framework 4.6.1)

## Project Structure
```
source/
├── TUtils3.sln                    # Main solution file
├── TUtils.Common/                 # Core library
│   ├── Async/                     # Async utilities and patterns
│   ├── CommandLine/               # Command line parsing
│   ├── Common/                    # Core utilities (IndexedTable, FilePath, etc.)
│   ├── DependencyInjection/       # DI container utilities
│   ├── Extensions/                # Extension methods
│   ├── Logging/                   # TLog/ITLog logging framework
│   ├── Reflection/                # Reflection utilities
│   ├── Security/                  # Crypto (symmetric/asymmetric)
│   └── Transaction/               # Transaction management
├── TUtils.Common.EF/              # Entity Framework Core utilities
│   └── Transaction/               # EF transaction management
├── TUtils.Common.MVC/             # ASP.NET Core MVC utilities
│   ├── Unity/                     # DI integration
│   └── Extensions and Attributes  # MVC helpers
└── TUtils.Common.Test/            # Unit tests
    ├── Cert/                      # Test certificates
    └── Various test files         # Component tests
```

## Architecture
- **Main Projects**: TUtils.Common (core), TUtils.Common.EF (Entity Framework Core), TUtils.Common.MVC (ASP.NET Core MVC), TUtils.Common.Test (unit tests)
- **Core Components**: Logging (TLog/ITLog), Crypto (symmetric/asymmetric), AsyncEvent, ThreadStarter, IndexedTable, FilePath utilities, DependencyInjection, CommandLine parsing
- **Test Framework**: MSTest with Microsoft.VisualStudio.TestTools.UnitTesting
- **Dependencies**: 
  - Core: log4net, System.Data.SqlClient
  - EF: Microsoft.EntityFrameworkCore (SQL Server, SQLite), Microsoft.Extensions.Configuration
  - MVC: Microsoft.AspNetCore.App, Microsoft.Extensions.DependencyInjection, Newtonsoft.Json
  - Test: EntityFramework 6.4.4, LinqKit

## Code Style & Conventions
- **Naming**: PascalCase for public members, camelCase for private fields, prefix interfaces with 'I'
- **Class prefix**: 'T' for main classes (TFilePath, TLog, TThreadStorage)
- **Imports**: Use `using` statements, reference TUtils.Common namespaces
- **Comments**: XML documentation for public APIs, inline comments sparingly
- **File organization**: Group by functionality (Common/, Logging/, Security/, Extensions/)
- **Error handling**: Use exceptions, provide detailed error messages
- **Async**: Use async/await pattern with CancellationToken support
