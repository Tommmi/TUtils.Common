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
  - Core: log4net 3.1.0, System.Data.SqlClient 4.9.0
  - EF: Microsoft.EntityFrameworkCore 8.0.18 (SQL Server, SQLite), Microsoft.Extensions.Configuration 8.0.0
  - MVC: Microsoft.AspNetCore.App, Microsoft.Extensions.DependencyInjection 8.0.1, Newtonsoft.Json 13.0.3
  - Test: Microsoft.EntityFrameworkCore.SqlServer 8.0.18, MSTest.TestFramework 3.10.0, MSTest.TestAdapter 3.10.0, Microsoft.NET.Test.Sdk 17.14.1, LinqKit 1.3.8

## Key Components Index
- **TLog**: source/TUtils.Common/Logging/TLog.cs - Main logging implementation with thread-attached values
- **ITLog**: source/TUtils.Common/Logging/ITLog.cs - Core logging interface
- **AsyncEvent**: source/TUtils.Common/Async/AsyncEvent.cs - Event for asynchronous waiting with await keyword
- **AsyncThreadStarter**: source/TUtils.Common/Async/AsyncThreadStarter.cs - Creates threads with async/await support
- **TFilePath**: source/TUtils.Common/Common/TFilePath.cs - File path manipulation and utilities
- **TThreadStorage**: source/TUtils.Common/Common/TThreadStorage.cs - Thread-bound object storage
- **IndexedTable**: source/TUtils.Common/Common/IndexedTable.cs - High-performance indexed data structure
- **CommandLineArgs**: source/TUtils.Common/CommandLine/CommandLineArgs.cs - Command line argument parsing
- **SymmetricCrypt**: source/TUtils.Common/Security/Symmetric/ - AES encryption utilities
- **RSA/Certificates**: source/TUtils.Common/Security/Asymmetric/ - RSA encryption and certificate handling
- **IDIContainer**: source/TUtils.Common/DependencyInjection/IDIContainer.cs - Technology-independent DI container
- **Extensions**: source/TUtils.Common/Extensions/ - Extensive extension methods for strings, arrays, types
- **ITransactionService**: source/TUtils.Common/Transaction/ITransactionService.cs - Transaction management
- **AssemblyLib**: source/TUtils.Common/Reflection/AssemblyLib.cs - Assembly search and manipulation

## Code Style & Conventions
- **Naming**: PascalCase for public members, camelCase for private fields, prefix interfaces with 'I'
- **Class prefix**: 'T' for main classes (TFilePath, TLog, TThreadStorage)
- **Imports**: Use `using` statements, reference TUtils.Common namespaces
- **Comments**: XML documentation for public APIs, inline comments sparingly
- **File organization**: Group by functionality (Common/, Logging/, Security/, Extensions/)
- **Error handling**: Use exceptions, provide detailed error messages
- **Async**: Use async/await pattern with CancellationToken support
