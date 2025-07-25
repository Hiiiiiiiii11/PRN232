var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

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
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ✅ IMPORTANT: UseSession() must be AFTER UseRouting() but BEFORE UseAuthorization()
app.UseSession();
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
