using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using SqlApp.Components;

var builder = WebApplication.CreateBuilder(args);

// When the app is launched as the published .exe (no ASPNETCORE_URLS/launchSettings
// in play), bind to a fixed, well-known localhost port so we know what URL to open.
var hasExplicitUrls = !string.IsNullOrWhiteSpace(builder.Configuration["ASPNETCORE_URLS"])
                       || !string.IsNullOrWhiteSpace(builder.Configuration["urls"]);
const string defaultUrl = "http://localhost:5166";
if (!hasExplicitUrls)
{
    builder.WebHost.UseUrls(defaultUrl);
}

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();


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
    .AddInteractiveServerRenderMode();

// Print the local network address(es) so the app can be reached from other devices on the LAN,
// and (when running as the packaged exe, not `dotnet run` in dev) auto-open the default browser.
app.Lifetime.ApplicationStarted.Register(() =>
{
    var addressFeature = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
    var localIp = GetLocalIPAddress();

    if (addressFeature is not null && localIp is not null)
    {
        foreach (var address in addressFeature.Addresses)
        {
            var uri = new Uri(address);
            Console.WriteLine($"Accessible at: {uri.Scheme}://{localIp}:{uri.Port}");
        }
    }

    if (!app.Environment.IsDevelopment())
    {
        var launchUrl = addressFeature?.Addresses.FirstOrDefault() ?? defaultUrl;
        TryOpenBrowser(launchUrl);
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

static string? GetLocalIPAddress()
{
    foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
    {
        if (networkInterface.OperationalStatus != OperationalStatus.Up)
        {
            continue;
        }

        if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
        {
            continue;
        }

        foreach (var unicastAddress in networkInterface.GetIPProperties().UnicastAddresses)
        {
            if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
            {
                return unicastAddress.Address.ToString();
            }
        }
    }

    return null;
}

