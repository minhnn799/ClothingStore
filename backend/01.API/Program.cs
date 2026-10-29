using _01.API;
using _03._Infrastructure;
using _03._Infrastructure.Identity;
using _03._Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();
var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "ClothingStore API v1"));
}
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
if (!string.Equals(builder.Configuration["Storage:Provider"], "Cloudinary", StringComparison.OrdinalIgnoreCase))
{
    var uploads = Path.Combine(app.Environment.ContentRootPath, "uploads");
    Directory.CreateDirectory(uploads);
    app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(uploads), RequestPath = "/uploads" });
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
using (var scope = app.Services.CreateScope())
{
    if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}
app.Run();
public partial class Program;
