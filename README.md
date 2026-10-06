# sql-trainer-app

A Blazor Server (interactive server render mode) web app built with MudBlazor.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) installed on the development machine.
- No runtime needs to be installed on end-user machines — the published build is self-contained.

## Running locally (development)

```bash
dotnet run
```

This uses `Properties/launchSettings.json` and will automatically open your browser
at `http://localhost:5166` (or `https://localhost:7011`).

## Publishing a standalone Windows build — literally one `.exe`

The app publishes to a **single, fully self-contained `SqlApp.exe`** for Windows.
No .NET install on the target machine, no companion `wwwroot` folder, no asset
manifest file — just the one executable.

This works because all static web assets (CSS, JS, favicon, MudBlazor's
resources, Blazor's own `blazor.web.js`) are embedded directly into the `.exe`
as assembly resources at build time, and served at runtime via a custom
`EmbeddedFileProvider` (see the `EmbedStaticWebAssetsAsResources` MSBuild
target in `SqlApp.csproj` and the static file setup in `Program.cs`).

From the project folder (`SqlApp/`, where `SqlApp.csproj` lives), run:

```bash
dotnet publish -c Release -r win-x64 -o ./publish/win-x64
```

This produces:

```
publish/win-x64/
  SqlApp.exe          <- this is the entire app. Run it.
  appsettings.json     <- optional (logging config); safe to delete if you don't need it
```

That's it. No `wwwroot/`, no `.staticwebassets.endpoints.json` — everything
needed to render the page (styles, scripts, favicon) ships inside the `.exe`
itself.

### What happens when `SqlApp.exe` is run

- It starts a local web server bound to `http://localhost:5166`.
- It automatically opens the default web browser to that URL.
- Closing the console window (or pressing `Ctrl+C`) stops the server.

### Distributing the app

Just send `SqlApp.exe` (and optionally `appsettings.json`) to your colleagues.
No zip folder required, no installation, no terminal, no .NET runtime needed —
they just double-click the `.exe`.

> Note: `SqlApp.exe` is ~50 MB because it embeds the full .NET runtime plus all
> static web assets. That's the expected size trade-off for a true single-file,
> zero-dependency executable.


### Publishing for other platforms

Replace `win-x64` with another [Runtime Identifier (RID)](https://learn.microsoft.com/dotnet/core/rid-catalog)
to target a different OS/architecture, e.g.:

```bash
dotnet publish -c Release -r linux-x64 -o ./publish/linux-x64
dotnet publish -c Release -r osx-arm64 -o ./publish/osx-arm64
```

(Automatic browser launch and single-file packaging work the same way on those
platforms too.)

## Project structure

- `Program.cs` — app startup, Kestrel configuration, and the auto browser-launch logic.
- `Components/` — Razor components and pages.
- `appsettings.json` — configuration (logging, allowed hosts).
- `Properties/launchSettings.json` — dev-only run profiles (not used by the published `.exe`).

