using System.Diagnostics;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.FileProviders;
using SqlTrainer.App.Components;
using SqlTrainer.UI;

var builder = WebApplication.CreateBuilder(args);

// Listen on the loopback interface only, never on other network adapters. Port 0 lets the
// OS pick a free port, so a second app on the same port can't block the start. A fixed
// port can be set for debugging, e.g. `dotnet run -- --Port 5166`.
// This overrides any ASPNETCORE_URLS / --urls / launchSettings address on purpose.
var port = builder.Configuration.GetValue("Port", 0);
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, port));

// Composition root: each layer registers its own services.
builder.Services.AddSqlTrainerUi();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

// Serve static web assets (CSS, favicon, MudBlazor JS/CSS, blazor.web.js, scoped
// component JS) straight out of this assembly's embedded resources, so the published
// .exe is truly standalone — no companion wwwroot folder or asset manifest required.
// In Development (`dotnet run`), prefer the real files on disk so edits show up
// immediately without a rebuild; fall back to the embedded copies otherwise.
var assembly = Assembly.GetExecutingAssembly();
var embeddedWebRootProvider = new EmbeddedFileProvider(assembly, $"{assembly.GetName().Name}.wwwroot");
app.Environment.WebRootFileProvider = Directory.Exists(app.Environment.WebRootPath)
    ? new CompositeFileProvider(new PhysicalFileProvider(app.Environment.WebRootPath), embeddedWebRootProvider)
    : embeddedWebRootProvider;
app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Routes).Assembly);

// "Beenden" in the app posts here. Closing the browser tab does not stop the server,
// so this is the visible way to quit. The plain HTML answer replaces the Blazor page,
// so the user sees a final message instead of the reconnect dialog.
app.MapPost("/quit", async (HttpContext context, IAntiforgery antiforgery, IHostApplicationLifetime lifetime) =>
{
    if (!await antiforgery.IsRequestValidAsync(context))
    {
        return Results.BadRequest();
    }

    context.Response.OnCompleted(() =>
    {
        lifetime.StopApplication();
        return Task.CompletedTask;
    });

    return Results.Content(
        """
        <!DOCTYPE html>
        <html lang="de">
        <head><meta charset="utf-8" /><title>SQL-Trainer beendet</title></head>
        <body style="font-family: sans-serif; margin: 3rem;">
            <h1>SQL-Trainer wurde beendet.</h1>
            <p>Sie können dieses Browserfenster jetzt schließen.</p>
        </body>
        </html>
        """,
        "text/html; charset=utf-8");
});

// Show where the app runs and how to stop it, then open the default browser
// (can be switched off with `--OpenBrowser false`, e.g. for automated runs).
app.Lifetime.ApplicationStarted.Register(() =>
{
    var addressFeature = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
    var url = addressFeature?.Addresses.FirstOrDefault();
    if (url is null)
    {
        return;
    }

    Console.WriteLine($"SQL-Trainer läuft unter {url}");
    Console.WriteLine("Beenden: in der App oben rechts auf \"Beenden\" klicken oder dieses Fenster schließen (Strg+C).");

    if (app.Configuration.GetValue("OpenBrowser", true))
    {
        TryOpenBrowser(url);
    }
});

app.Run();

static void TryOpenBrowser(string url)
{
    try
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Could not automatically open a browser: {ex.Message}");
        Console.WriteLine($"Please open {url} manually.");
    }
}
