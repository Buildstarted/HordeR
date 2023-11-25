import { HubConnectionBuilder, HubConnectionState, LogLevel } from "https://cdn.jsdelivr.net/npm/@microsoft/signalr/+esm";

const packetHandlers = {};
let activeState = null;
let tps = 20;
let clientWorldTick = 0;
let prevTime = performance.now();
let tickPrevTime;
let frames = 0;
let tickTime = 0;
let fps = 60;
let serverWorldTick = 0;
let tickOffset = 0;
let lastSyncTime = 0;

function handleIncomingPacket(packet) {
    const type = packet.type;
    if (packetHandlers[type]) {
        for (let h in packetHandlers[type]) {
            packetHandlers[type][h](packet);
        }
    }
}

function handleIncomingPackets(packets) {
    for (let p in packets) {
        const packet = packets[p];
        handleIncomingPacket(packet);
    }
}

const connection = new HubConnectionBuilder()
    .withUrl("/game")
    //.configureLogging(LogLevel.Debug)
    .build();

connection.on("packet", handleIncomingPacket);
connection.on("packets", handleIncomingPackets);

connection.onclose(error => {
    console.info("signalr hub disconnected", error);
    handleIncomingPacket({ type: "disconnected" });
});

async function start() {
    try {
        await connection.start();
        const settings = await connection.invoke("GetSettings");
        tps = settings.tps;
        clientWorldTick = settings.worldTick;
        tickPrevTime = performance.now();
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
}

function calculateFPS(time) {
    if (time >= prevTime + 1000) {
        fps = (frames * 1000) / (time - prevTime);
        frames = 0;
        prevTime = time;
    }
}

function calculateTPS(time) {
    while (tickTime > 1000 / tps) {
        tickTime -= 50;
        clientWorldTick++;
        if (activeState) {
            activeState.tick();
        }
    }
}

export function update() {
    frames++;
    let time = performance.now();
    tickTime += time - tickPrevTime;
    tickPrevTime = time;

    calculateFPS(time);
    calculateTPS(time);

    if (activeState) {
        activeState.update();
    }

    doSync();
}

export function draw() {
    if (activeState) { activeState.draw(); }
}

async function doSync() {
    const now = performance.now();
    if (lastSyncTime + 5000 < now) {
        if (connection.state === HubConnectionState.Connected) {
            const result = await connection.invoke("Sync", clientWorldTick);
            tickOffset = result.offset;
            clientWorldTick = result.serverTick;
        }
        lastSyncTime = now;
        tickTime = 0;
    }
}

export function changeState(state) {
    if (activeState) {
        activeState.exitState();
    }
    activeState = state;
    activeState.enterState();
}

export function addPacketHandler(packetId, callback) {
    if (!packetHandlers[packetId]) {
        packetHandlers[packetId] = [];
    }
    packetHandlers[packetId].push(callback);
}

export function removePacketHandler(packetId, callback) {
    if (packetHandlers[packetId]) {
        packetHandlers[packetId] = packetHandlers[packetId].filter(x => x !== callback);
    }
}

export function sendPacket(packet) {
    if (!packet.type) {
        throw new Error("packet.type must be defined");
    }
    if (connection.state === HubConnectionState.Connected) {
        connection.invoke("Packet", packet);
    }
}

export async function connect() {
    const selfId = await start();
    return selfId;
}

export class GameState {

    enterState() { }

    exitState() { }

    tick() { }

    update() { }

    draw() { }
}