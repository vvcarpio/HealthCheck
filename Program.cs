using HealthCheck;

var builder = WebApplication.CreateBuilder(args);
IConfiguration Configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks()
    .AddCheck("ICMP_01", new ICMPHealthCheck("www.ryadel.com", 100))
    .AddCheck("ICMP_02", new ICMPHealthCheck("www.google.com", 100))
    .AddCheck("ICMP_03", new ICMPHealthCheck("www.does-not-exist-aaa.com", 100));

var app = builder.Build();
app.MapHealthChecks("/hc", new CustomHealthCheckOptions());
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions()
{
    OnPrepareResponse = context =>
    {
        if (context.File.Name == "isOnline.txt")
        {
            // disable caching for these files
            context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
            context.Context.Response.Headers.Add("Expires", "-1");
        } else
        {
            // Retrieve cache configuration from appsettings.json.
            context.Context.Response.Headers["Cache-Control"] = Configuration["StaticFiles:Headers:Cache-Control"];
        }
    }
});
app.UseRouting();

app.MapFallbackToFile("index.html");

app.Run();
