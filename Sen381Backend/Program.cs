using Sen381.Data_Access;
using Sen381.Business.Services;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<SupaBaseAuthService>();
builder.Services.AddScoped<SupaBaseAuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IUserService, UserService>();


// ✅ Add Supabase service
builder.Services.AddScoped<SupaBaseAuthService>();

// Register Supabase.Client for storage/database access used by FileController
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];
if (!string.IsNullOrWhiteSpace(supabaseUrl) && !string.IsNullOrWhiteSpace(supabaseKey))
{
    var options = new SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = true
    };
    builder.Services.AddSingleton(new Client(supabaseUrl, supabaseKey, options));
}

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("https://localhost:7097") 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


var app = builder.Build();
// Configure the HTTP request pipeline
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