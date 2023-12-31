﻿using demo.Packets.ClientBound;
using demo.Packets.ServerBound;
using HordeR.Server;
using HordeR.Server.Packets;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Immutable;

public class Server : GameServer
{
    private ImmutableDictionary<string, Player> players;

    public Server(ILogger<Server> logger, IHubContext<GameHub> hub) : base(20, logger, hub)
    {
        players = ImmutableDictionary<string, Player>.Empty;

        AddPacketHandler<InputPacket>(OnInputPacket);
        AddPacketHandler<JoinPacket>(OnJoinPacket);
        AddPacketHandler<ChatMessagePacket>(OnChatMessagePacket);
        AddPacketHandler<DisconnectionPacket>(OnDisconnectionPacket);
    }

    protected override void ClientConnected(Connection connection)
    {
        base.ClientConnected(connection);
    }

    public Player? GetPlayer(string connectionId)
    {
        if (players.ContainsKey(connectionId))
        {
            return players[connectionId];
        }

        return null;
    }

    public void RemovePlayer(string connectionId)
    {
        players = players.Remove(connectionId);
    }

    public override void Tick()
    {
        var allplayers = players.ToList();
        Broadcast(allplayers.Select(x => new EntityPositionPacket(x.Value.Id, x.Value.X, x.Value.Y, x.Value.Sequence)));
    }

    private void OnJoinPacket(JoinPacket packet)
    {
        var player = new Player(packet.Connection, packet.Name);

        //send all existing players to this client
        player.Connection.Send(players.Select(p => new PlayerConnectedPacket(p.Value)));

        //add the player to the list of players
        players = players.Add(packet.Connection.ConnectionId, player);

        //send login success to the client
        player.Connection.Send(new LoginSuccessPacket(player));

        //broadcast to all other clients that this player has joined
        Broadcast(new PlayerConnectedPacket(player));
    }

    private void OnChatMessagePacket(ChatMessagePacket packet)
    {
        var player = GetPlayer(packet.Connection.ConnectionId);
        Broadcast(new ChatMessageSendPacket(player.Name, packet.Message));
    }

    private void OnInputPacket(InputPacket packet)
    {
        if (!players.ContainsKey(packet.Connection.ConnectionId)) { return; }
        var player = players[packet.Connection.ConnectionId];
        player.HandleInput(packet);
    }

    private void OnDisconnectionPacket(DisconnectionPacket packet)
    {
        if(players.ContainsKey(packet.Connection.ConnectionId))
        {
            var player = players[packet.Connection.ConnectionId];
            RemovePlayer(packet.Connection.ConnectionId);
            Broadcast(new PlayerDisconnectedPacket(player.Id));
        }
    }
}
