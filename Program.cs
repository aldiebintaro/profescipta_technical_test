var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Add Session Services
builder.Services.AddDistributedMemoryCache(); 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

// Enable Session Middleware
app.UseSession();

app.UseAuthorization();
app.MapRazorPages();
app.Run();
