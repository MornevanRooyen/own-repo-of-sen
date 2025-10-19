using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend;
using Sen381.Data_Access;
using System;
using System.Net.Http;
using Microsoft.AspNetCore.SignalR.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ✅ Load config from wwwroot/appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ✅ Backend API base URL
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7228/")
});

// ✅ Register services
builder.Services.AddScoped<SupaBaseAuthService>();
builder.Services.AddScoped<Frontend.Services.AuthService>();
builder.Services.AddScoped<Frontend.Services.TutorApplicationService>();
builder.Services.AddScoped<Frontend.Services.SubjectService>();
builder.Services.AddScoped<Frontend.Services.SubjectTutorService>();
builder.Services.AddScoped<Frontend.Services.SubjectSubscriptionService>();
builder.Services.AddScoped<Frontend.Services.BookingSessionService>();
builder.Services.AddScoped<Frontend.Services.CalendarService>();
builder.Services.AddScoped<Frontend.Services.TopicService>();

// SignalR for real-time communication
builder.Services.AddScoped(sp => new HubConnectionBuilder()
    .WithUrl("https://localhost:7228/tutoringSessionHub")
    .Build());

await builder.Build().RunAsync();
