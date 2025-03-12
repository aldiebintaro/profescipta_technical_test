var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages
builder.Services.AddRazorPages();

// Add Session Services
builder.Services.AddDistributedMemoryCache(); // Required for session storage
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Protect against client-side scripts
    options.Cookie.IsEssential = true; // Ensure session works even if tracking is disabled
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

// Enable Session Middleware
app.UseSession();

app.UseAuthorization();
app.MapRazorPages();
app.Run();
