# Automation.Velopack

A reusable Velopack bootstrapper library that provides automated update management with flexible channel configuration for .NET applications.

## Overview

This library simplifies integrating Velopack updates into your .NET applications by providing:

- Automatic update checking and installation
- Flexible channel configuration (Stable, Prerelease, Beta, etc.)
- Multiple configuration methods: environment variables, command-line arguments, or marker files
- Support for both global and application-specific channel settings

## Getting Started

### Installation

Add a reference to the `Automation.Velopack` project in your application:

```xml
<ItemGroup>
  <ProjectReference Include="..\Automation.Velopack\Automation.Velopack.csproj" />
</ItemGroup>
```

### Basic Usage

In your application's startup code:

```csharp
using Automation.Velopack;

// Call this early in your application startup
Velopack.Build().Run();
VelopackBootstrapper.Startup("YourAppName");
```

## Channel Configuration

The library resolves update channels in the following priority order:

1. **Environment Variable** - `VELOPACK_CHANNEL` (highest priority)
2. **Command Line Argument** - `--channel=ChannelName`
3. **Global Channel File** - `%LOCALAPPDATA%\Automation\.channel`
4. **Application Channel File** - `%LOCALAPPDATA%\YourAppName\.channel`
5. **Default** - "Stable" (lowest priority)

### Creating Channel Files

Use the `CreateChannelFile` method to create channel marker files programmatically:

```csharp
// Create a global channel file (affects all apps using this library)
VelopackBootstrapper.CreateChannelFile(ChannelScope.Global, "YourAppName", "Prerelease");

// Create an application-specific channel file
VelopackBootstrapper.CreateChannelFile(ChannelScope.Application, "YourAppName", "Beta");
```

### Command Line Usage

Users can specify a channel via command line:

```bash
YourApp.exe --channel=Prerelease
```

### Environment Variable Usage

Set the environment variable for CI/CD or testing:

```bash
set VELOPACK_CHANNEL=Development
YourApp.exe
```

## Channel Scope

- **Global** (`ChannelScope.Global`): Stored in `%LOCALAPPDATA%\Automation\.channel` - affects all applications using this library
- **Application** (`ChannelScope.Application`): Stored in `%LOCALAPPDATA%\YourAppName\.channel` - specific to one application

## Build and Test

Build the solution:

```bash
dotnet build
```

Run the unit tests:

```bash
dotnet test
```

The test project includes comprehensive tests for:
- Channel resolution logic
- Channel file creation (global and application-scoped)
- Priority ordering of configuration sources

## Requirements

- .NET 8.0 or higher
- Velopack NuGet package

## Project Structure

- **Automation.Velopack** - Main library project
- **Automation.Velopack.UnitTests** - xUnit test project

## License

[Specify your license here]
