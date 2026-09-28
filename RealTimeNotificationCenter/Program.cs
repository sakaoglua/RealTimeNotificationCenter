using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealTimeNotificationCenter.Hubs;
using RealTimeNotificationCenter.Models;
using RealTimeNotificationCenter.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddSingleton<OnlineUserTrackerService>(); // Tüm SignalR hub'larý ve controller'lar için tek bir instance oluþturmak için singleton olarak ekliyoruz.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/User/SignIn";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//app.UseDefaultFiles(); // Serve default files like index.html
app.UseStaticFiles(); // Serve static files from wwwroot

app.UseHttpsRedirection();

app.UseAuthentication(); // BUNU EKLE
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=SignIn}/{id?}");
app.Run();
