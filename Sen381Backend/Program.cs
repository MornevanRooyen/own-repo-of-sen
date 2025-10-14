using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sen381.Data_Access;
using Sen381.Business.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Supabase service as a singleton (parameterless ctor reads appsettings.json)
builder.Services.AddSingleton<SupaBaseAuthService>();

// Your other backend services
builder.Services.AddSingleton<SupaBaseAuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IUserService, UserService>();   // ✅ FIX
builder.Services.AddScoped<NotificationService>();
;

// CORS for your WASM app (adjust ports if needed)
const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins("https://localhost:7097", "http://localhost:5097")
              .AllowAnyHeader()
              .AllowAnyMethod();
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
app.Run();
