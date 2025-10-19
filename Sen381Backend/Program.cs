using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sen381.Data_Access;
using Sen381.Business.Services;
using Sen381Backend.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Sen381Backend API", Version = "v1" });
});

// Supabase service as a singleton (parameterless ctor reads appsettings.json)
builder.Services.AddSingleton<SupaBaseAuthService>();

// Your other backend services
builder.Services.AddSingleton<SupaBaseAuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IUserService, UserService>();   // ✅ FIX
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<SubjectService>();
builder.Services.AddScoped<TopicService>();

// SignalR for real-time communication
builder.Services.AddSignalR();

// CORS for your WASM app (adjust ports if needed)
const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins("https://localhost:7097", "http://localhost:5097")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();
app.MapControllers();

// Map SignalR hubs
app.MapHub<TutoringSessionHub>("/tutoringsessionhub");
app.MapHub<MessagingHub>("/messaginghub");

app.Run();
