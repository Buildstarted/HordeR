using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HordeR.Server;
public static class WebApplicationExtensions
{
    public static void AddGameServer(this IServiceCollection services)
    {
        services.AddSingleton<GameServer>();
    }

    public static void UseGameServer(this WebApplication app)
    {
        app.MapHub<GameHub>("/game");   
    }
}
