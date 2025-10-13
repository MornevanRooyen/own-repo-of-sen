using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sen381.Data_Access;
using Sen381.Business.Services;
using Frontend;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 🔐 Read Supabase config
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:AnonKey"];


// 🧠 Register your services
builder.Services.AddSingleton(new SupaBaseAuthService(supabaseUrl, supabaseKey));
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<Frontend.Services.AuthService>();

// ✅ Register ONE HttpClient — pointing to your backend API
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7097/") // backend API base
});

await builder.Build().RunAsync();
