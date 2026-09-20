using FeedbackPlatform.Application;
using FeedbackPlatform.Application.Common.Interfaces;
using FeedbackPlatform.Infrastructure;
using FeedbackPlatform.Infrastructure.Persistence;
using FeedbackPlatform.Web.Auth;
using FeedbackPlatform.Web.ErrorHandling;
using FeedbackPlatform.Web.RateLimiting;
using FeedbackPlatform.Web.Seed;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options => options.Filters.Add<MvcExceptionFilter>());

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
    })
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName, options => { });

builder.Services.AddAuthorization();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Feedback Platform API", Version = "v1" });

    var apiKeyScheme = new OpenApiSecurityScheme
    {
        Name = ApiKeyAuthenticationHandler.HeaderName,
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "API key generated for your application (X-Api-Key header)."
    };
    options.AddSecurityDefinition("ApiKey", apiKeyScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" } }] = []
    });
});

var app = builder.Build();

// Apply the base schema and seed the first admin account before serving traffic.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DatabaseMigrator>().Migrate();
    await AdminSeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
}

app.UseForwardedHeaders();

// Applies in every environment, including Development, so API consumers always get the documented
// problem+json error shape instead of the HTML/verbose-JSON developer exception page.
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api"),
    branch => branch.UseExceptionHandler());

if (!app.Environment.IsDevelopment())
{
    // In Development, portal (non-API) pages fall through to the host's built-in developer exception page.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Placed after UseAuthorization: only here has the "ApiKey" scheme (a non-default scheme,
// authenticated lazily per [Authorize(AuthenticationSchemes=...)]) actually populated HttpContext.User.
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api"),
    branch => branch.UseMiddleware<ApiKeyRateLimitingMiddleware>());

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllers();

app.Run();
