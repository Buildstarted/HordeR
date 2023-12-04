using HordeR.Server.Packets;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json.Nodes;

namespace HordeR.Server;

public abstract class GameServer
{
    private static readonly object packetLocker = new();

    private readonly ILogger logger;
    private readonly IHubContext<GameHub> hubContext;
    private readonly Looper looper;
    private readonly ConcurrentQueue<(Connection client, int type, JsonNode message)> clientPackets;
    private readonly Dictionary<int, List<(Action<IServerBoundPacket> Callback, object OriginalMethod)>> packetHandlers;
    private readonly Dictionary<int, Func<PacketConstructorInfo, IServerBoundPacket>> packetTypeRegister;
    private ImmutableDictionary<string, Connection> connections;

    private long worldTick;

    protected virtual void ClientConnected(Connection connection) { }
    protected virtual void ClientDisconnected(Connection connection) { }
    protected IHubContext<GameHub> HubContext => hubContext;

    public abstract void Tick();

    public long WorldTick => worldTick;
    public int TPS { get; }

    public GameServer(int tps, ILogger<GameServer> logger, IHubContext<GameHub> hub)
    {
        this.TPS = tps == 0 ? 20 : tps;
        this.logger = logger;
        this.hubContext = hub;
        looper = new Looper();
        clientPackets = new();
        worldTick = 0;
        packetHandlers = new();
        packetTypeRegister = new();
        connections = ImmutableDictionary<string, Connection>.Empty;

        RegisterPacketTypes();
    }

    private void RegisterPacketTypes()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IServerBoundPacket).IsAssignableFrom(p) && !p.IsInterface);

        foreach (var type in types)
        {
            var typeid = (int?)type.GetProperty("Type")!.GetValue(null);
            //create packet is optional
            var createPacketProperty = type.GetProperty("CreatePacket")?.GetValue(null);
            if (createPacketProperty is not null)
            {
                var createPacket = (Func<PacketConstructorInfo, IServerBoundPacket>?)createPacketProperty;
                if (typeid is null || createPacket is null)
                {
                    logger.LogError($"Failed to register packet type {type.Name}");
                    continue;
                }

                packetTypeRegister.Add(typeid.Value, createPacket);
            }
        }
    }

    public void AddPacketHandler<T>(Action<T> handler) where T : IServerBoundPacket
    {
        var type = T.Type;

        if (!packetHandlers.ContainsKey(type))
        {
            packetHandlers.Add(type, new());
        }

        Action<IServerBoundPacket> action = (packet) => { handler.Invoke((T)packet); };

        packetHandlers[type].Add((action, handler));
    }

    public void RemovePacketHandler<T>(Action<T> handler) where T : IServerBoundPacket
    {
        var type = T.Type;

        if (packetHandlers.ContainsKey(type))
        {
            foreach (var item in packetHandlers[type])
            {
                if (item.OriginalMethod.Equals(handler))
                {
                    packetHandlers[type].Remove(item);
                    break;
                }
            }
        }
    }

    public void Start()
    {
        logger.LogInformation("Server started");

        looper.Start();
        looper.AddCallback(new TimedCallback(TPS, OnTick));
    }

    private void OnTick(float delta)
    {
        worldTick++;

        //todo: handle all messages from signalr
        //todo: update game state
        (Connection client, int type, JsonNode message)[] packets;

        //atomically remove all the packets from the queue
        //this prevents new packets from being added to the queue while we are processing them
        lock (packetLocker)
        {
            packets = clientPackets.ToArray();
            clientPackets.Clear();
        }

        foreach(var packet in packets)
        {
            if (packetHandlers.ContainsKey(packet.type))
            {
                foreach (var handler in packetHandlers[packet.type].ToList())
                {
                    var constructor = packetTypeRegister[packet.type];
                    var package = new PacketConstructorInfo(packet.client, packet.message);
                    var packetimpl = constructor(package);

                    handler.Callback(packetimpl);
                }
            }
        }

        //create a tick packet that is sent to any handlers
        //after the client packets have been processed
        var tick = (int)PacketType.Connect;
        var tickPacket = new TickPacket(worldTick);
        if (packetHandlers.ContainsKey((int)PacketType.Tick))
        {
            foreach (var handler in packetHandlers[(int)PacketType.Tick])
            {
                handler.Callback(tickPacket);
            }
        }

        Tick();
    }

    public virtual void Broadcast<T>(T packet) where T : IClientBoundPacket => Broadcast<T>([packet]);
    public virtual void Broadcast<T>(IEnumerable<T> packets) where T : IClientBoundPacket => hubContext.Clients.All.SendAsync("packets", packets);

    public virtual void Broadcast<T>(string groupName, T packet) where T : IClientBoundPacket => Broadcast<T>(groupName, [packet]);
    public virtual void Broadcast<T>(string groupName, IEnumerable<T> packets) where T : IClientBoundPacket => hubContext.Clients.Group(groupName).SendAsync("packets", packets);

    public virtual void Broadcast<T>(string[] groupNames, T packet) where T : IClientBoundPacket => Broadcast<T>(groupNames, [packet]);
    public virtual void Broadcast<T>(string[] groupNames, IEnumerable<T> packets) where T : IClientBoundPacket => hubContext.Clients.Groups(groupNames).SendAsync("packets", packets);

    public virtual void Broadcast<T>(IReadOnlyList<Connection> connections, T packet) where T : IClientBoundPacket => Broadcast<T>(connections, [packet]);
    public virtual void Broadcast<T>(IReadOnlyList<Connection> connections, IEnumerable<T> packets) where T : IClientBoundPacket => hubContext.Clients.Clients(connections.Select(c => c.ConnectionId).ToArray()).SendAsync("packets", packets);

    public virtual void Send<T>(Connection connection, T packet) where T : IClientBoundPacket => Send<T>(connection, [packet]);
    public virtual void Send<T>(Connection connection, IEnumerable<T> packets) where T : IClientBoundPacket => hubContext.Clients.Client(connection.ConnectionId).SendAsync("packets", packets);

    public virtual void AddToGroup(Connection connection, string groupName) => _ = hubContext.Groups.AddToGroupAsync(connection.ConnectionId, groupName);
    public virtual void RemoveFromGroup(Connection connection, string groupName) => _ = hubContext.Groups.RemoveFromGroupAsync(connection.ConnectionId, groupName);

    public virtual Connection CreateConnection(string connectionId)
    {
        var connection = new Connection(connectionId, this);
        connections = connections.Add(connectionId, connection);

        ClientConnected(connection);

        return connection;
    }

    public virtual Connection RemoveConnection(string connectionId)
    {
        var connection = connections[connectionId];
        connections = connections.Remove(connectionId);

        ClientDisconnected(connection);

        return connection;
    }

    public virtual Connection GetConnection(string connectionId)
    {
        if (connections.ContainsKey(connectionId))
        {
            return connections[connectionId];
        }

        return null;
    }

    public void OnPacketReceived(Connection connection, int type, JsonNode? message = null)
    {
        if (type < 256)
        {
            switch ((PacketType)type)
            {
                case PacketType.Ping:
                    int clientTick = message["tick"].GetValue<int>();
                    connection.SetPing(worldTick - clientTick);
                    connection.Send(new PongPacket(worldTick, clientTick));
                    break;
                case PacketType.Connect:
                    lock (packetLocker)
                    {
                        clientPackets.Enqueue((connection, type, null));
                        break;
                    }
                case PacketType.Disconnect:
                    lock (packetLocker)
                    {
                        clientPackets.Enqueue((connection, type, null));
                        break;
                    }
            }
        }
        else
        {
            lock (packetLocker)
            {
                clientPackets.Enqueue((connection, type, message));
            }
        }
    }

    #region Looper
    //Taken from EndGate https://github.com/NTaylorMullen/EndGate.Server/tree/master/EndGate.Server/EndGate.Server.Core/Loopers
    internal interface ILooper : IDisposable
    {
        void Start();
        void AddCallback(TimedCallback callback);
        void RemoveCallback(TimedCallback callback);
    }

    internal class Looper : ILooper
    {
        private object _updateLock;
        private long _running;

        // Used to stop callback execution on callback additions or looper stops (within a Callback itself)
        private bool _callbacksModified;
        private List<TimedCallback> _callbacks;

        public Looper()
        {
            // Default values
            _updateLock = new object();
            _running = 0;
            _callbacksModified = false;
            _callbacks = new List<TimedCallback>();
        }

        public void AddCallback(TimedCallback callback)
        {
            lock (_updateLock)
            {
                _callbacks.Add(callback);
                _callbacksModified = true;
            }
        }

        public void RemoveCallback(TimedCallback callback)
        {
            lock (_updateLock)
            {
                _callbacks.Remove(callback);
                _callbacksModified = true;
            }
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(Run);
        }

        private void Run(object state)
        {
            var timer = Stopwatch.StartNew();
            long elapsedTime;

            if (Interlocked.Exchange(ref _running, 1) == 0)
            {
                while (true)
                {
                    // This will never have contention unless the user is setting the fps, callback or stopping the looper
                    lock (_updateLock)
                    {
                        // Verify we're still running
                        if (Interlocked.Read(ref _running) == 0)
                        {
                            break;
                        }

                        _callbacksModified = false;

                        foreach (TimedCallback callback in _callbacks)
                        {
                            elapsedTime = timer.ElapsedMilliseconds - callback.LastTriggered;

                            if (elapsedTime >= callback.TriggerFrequency)
                            {
                                callback.LastTriggered = timer.ElapsedMilliseconds;
                                callback.Callback((float)elapsedTime / 1000);

                                // See if we modified 
                                if (_callbacksModified)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    Thread.Yield();
                }
            }

            timer.Stop();
        }

        public void Dispose()
        {
            lock (_updateLock)
            {
                _callbacks.Clear();
                _callbacks = null;
                Interlocked.Exchange(ref _running, 0);
                _callbacksModified = true;
            }
        }
    }

    internal class LooperCallback
    {
        public LooperCallback()
        {
        }

        public LooperCallback(Action<float> callback)
        {
            Callback = callback;
        }

        public Action<float> Callback { get; set; }
    }

    internal class TimedCallback : LooperCallback
    {
        private int _fps;

        public TimedCallback()
        {
        }

        public TimedCallback(int fps, Action<float> callback) :
            base(callback)
        {
            Fps = fps;
        }

        public int TriggerFrequency { get; private set; }
        public long LastTriggered { get; set; }
        public int Fps
        {
            get
            {
                return _fps;
            }
            set
            {
                _fps = value;

                TriggerFrequency = (_fps != 0) ? 1000 / _fps : 0;
            }
        }
    }
    #endregion
}
