# sql-trainer-app

A Blazor Server (interactive server render mode) web app built with MudBlazor.

## Short description

#### Start the app with:
```bash
dotnet watch
```

#### Publish the app to `./publish/win-x64/SqlApp.exe` with:
```bash
dotnet publish -c Release -r win-x64 -o ./publish/win-x64
```

## Long Description

## Get started (development)

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download) installed on the development machine.

### Check or install .NET 10 on Windows

1. If you're unsure whether you have the right SDK version installed, open **Command Prompt** or **PowerShell** and run:

   ```powershell
   dotnet --list-sdks
   ```

2. If the output includes a version starting with `10.0.`, the .NET 10 SDK is installed.
   If `dotnet` is not recognized or there is no `10.0.x` version, install the SDK.
3. Download the **.NET 10 SDK** for Windows from the
   [official .NET 10 download page](https://dotnet.microsoft.com/download/dotnet/10.0).
   Choose the installer for your Windows architecture (usually x64), run it, and
   then open a new Command Prompt or PowerShell window.
4. Run `dotnet --list-sdks` again to confirm that a `10.0.x` SDK appears.

### Running locally (development)

Running stale build:
```bash
dotnet run
```

Running with hot reload enabled (automatic browser refresh on code changes):
```bash
dotnet watch
```

Running the app will automatically serve it
at `http://localhost:5166` (or `https://localhost:7011`).

It will also make the app available to other devices on the same network at `http://<your-ip>:5166` (or `https://<your-ip>:7011`).
Your IP will be shown in the console output when the app starts.
If we want to disable this in the future, go to `Properties/launchSettings.json` and replace `0.0.0.0` with `localhost`.

## Publishing to SqlApp.Exe

The app publishes to a **single, fully self-contained `SqlApp.exe`** for Windows.

From the project folder (`sql-trainer-app/`, where `SqlApp.csproj` lives), run:

```bash
dotnet publish -c Release -r win-x64 -o ./publish/win-x64
```

This produces:

```
publish/win-x64/
  SqlApp.exe          <- this is the entire app. Run it.
  appsettings.json     <- optional (logging config); safe to delete if you don't need it
```

To make the resulting `.exe` work straight out of the box, the .NET runtime and all the static web assets (CSS, JS, favicon) are embedded inside the `.exe` itself.

The resulting `SqlApp.exe` will be ~50MB in size.


> Note: `SqlApp.exe` is ~50 MB because it embeds the full .NET runtime plus all
> static web assets. That's the expected size trade-off for a true single-file,
> zero-dependency executable.

### What happens when `SqlApp.exe` is run

- It starts a local web server bound to `http://localhost:5166`.
- It automatically opens the default web browser to that URL.
- Closing the console window (or pressing `Ctrl+C`) stops the server.

### Publishing for other platforms

Replace `win-x64` with another [Runtime Identifier (RID)](https://learn.microsoft.com/dotnet/core/rid-catalog)
to target a different OS/architecture, e.g.:

```bash
dotnet publish -c Release -r linux-x64 -o ./publish/linux-x64
dotnet publish -c Release -r osx-arm64 -o ./publish/osx-arm64
```

(Automatic browser launch and single-file packaging should work the same way on those
platforms too.)
