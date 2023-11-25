using HordeR.Server;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:5212");

builder.Services.AddSignalR();
builder.Services.AddHordeRServer<Server>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHordeRServer();

app.Run();
