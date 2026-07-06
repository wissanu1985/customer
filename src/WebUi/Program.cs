using Application;
using Infrastructure;
using WebUi.Components;
using WebUi.Services;

var builder = WebApplication.CreateBuilder(args);

// Layer wiring: Application (Mediator + validators + behaviours) + Infrastructure (EF Core + repositories)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Ant Design Blazor (components + MessageService/ModalService/NotificationService)
builder.Services.AddAntDesign();

// Global error dialog service — shows modal popup on unexpected errors
builder.Services.AddScoped<ErrorDialogService>();

// Scoped mediator — creates a fresh DI scope per Send() to avoid DbContext concurrency in Blazor Server
builder.Services.AddScoped<ScopedMediator>();

// Blazor Server
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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
