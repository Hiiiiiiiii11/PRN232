using DrugUserPreventionUI.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

// Configure API settings
builder.Services.Configure<ApiConfiguration>(
    builder.Configuration.GetSection("ApiSettings"));

// Register ApiConfiguration as singleton
builder.Services.AddSingleton<ApiConfiguration>(provider =>
{
    var config = new ApiConfiguration();
    builder.Configuration.GetSection("ApiSettings").Bind(config);
    return config;
});

// ✅ Add Authentication & Authorization
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Login?handler=Logout";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.Name = ".DrugPrevention.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

// ✅ Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".DrugPrevention.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // Remove UseHsts() for Docker deployment
    // app.UseHsts();
}

// Only redirect to HTTPS in development, not in Docker
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

// ✅ IMPORTANT: UseSession() must be AFTER UseRouting() but BEFORE UseAuthorization()
app.UseSession();

// ✅ Add Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// ✅ FIXED: Redirect to correct login page
app.MapGet(
    "/",
    context =>
    {
        context.Response.Redirect("/Index"); // Capital L
        return Task.CompletedTask;
    }
);

app.Run();
