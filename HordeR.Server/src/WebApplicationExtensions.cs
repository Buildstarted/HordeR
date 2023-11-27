using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HordeR.Server;
public static class WebApplicationExtensions
{
    public static void AddHordeRServer<T>(this IServiceCollection services) where T : GameServer
    {
        services.AddSingleton<GameServer, T>();
    }

    public static void UseHordeRServer(this WebApplication app, string url = "/game")
    {
        app.MapHub<GameHub>(url);

        var server = app.Services.GetService<GameServer>();
        if(server is null)
        {
            throw new Exception("You must call AddHordeRServer<T> before calling UseHordeRServer");
        }

        server.Start();
    }
}
