using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext with connection string from appsettings
var conn = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<ProjektGrupowy.WebApplication.Data.AppDbContext>(options =>
    options.UseNpgsql(conn));

var app = builder.Build();

// Ensure database created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProjektGrupowy.WebApplication.Data.AppDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

// Serve static files (wwwroot) and controllers
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

// Keep swagger available at /swagger; root will serve wwwroot/index.html if present

app.Run();
