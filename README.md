PayrocLoadBalancer

A lightweight, extensible Layer 4 (TCP) load balancer written in C#. Supports round-robin load distribution, health checking, connection draining, and backend failover.

🚀 Features

🌀 Round-robin routing for TCP clients

✅ Backend health checking (with automatic failover & recovery)

🧹 Draining mode for graceful backend removal

⚠️ Timeout handling for dead/unresponsive connections

🔧 Configuration via JSON

📝 Basic logging to help monitor connections and errors.

📦 Getting Started
1. Clone & Build
git clone https://github.com/fbloodworth/PayrocLoadBalancer.git
cd PayrocLoadBalancer
dotnet build


Or open the solution in Visual Studio and press F5.

2. Run the Load Balancer
dotnet run --project PayrocLoadBalancer


This will start the balancer on port 8000 (by default) and begin forwarding connections to configured backends.

⚙️ Configuration

Backends are configured via config.json in the root directory:

config.json:

{
  "Backends": [
    { "Host": "127.0.0.1", "Port": 9001 },
    { "Host": "127.0.0.1", "Port": 9002 },
    { "Host": "127.0.0.1", "Port": 9003 }
  ]
}


📌 Make sure config.json is set to Copy to Output Directory: Always in Visual Studio.

🩺 Health Checking

Each backend is checked every 2 seconds (default). If a backend becomes unreachable:

It is marked Draining — existing connections are preserved.

Once ActiveConnections = 0, it's marked Down and skipped entirely.

If it recovers (becomes reachable), it’s marked Up again.

Health checking is implemented via simple TCP handshake attempts with timeout.

🧪 Manual Testing

Start dummy servers:

ncat -l --keep-open --listen -p 9001
ncat -l --keep-open --listen -p 9002
ncat -l --keep-open --listen -p 9003


Run the load balancer.

Connect a client:

ncat localhost 8000


Kill a backend → watch it get marked as draining → down.

📌 Assumptions

This project makes the following assumptions to stay simple and testable:

❌ No SSL/TLS termination
The balancer forwards raw TCP traffic without decrypting or inspecting it. SSL termination should be handled by upstream proxies or the destination service.

📦 TCP-only (Layer 4)
This balancer only supports TCP. It does not support UDP, ICMP, HTTP-aware routing, or any application-layer protocols.

⚙️ Minimal configuration

Backend services are configured statically via a JSON file (config.json).

No admin UI, config hot-reload, or remote control.

Configuration reload requires restarting the process.

🧱 Limitations

Layer 4 only: forwards raw TCP traffic (not UPD); no protocol-specific logic (e.g., HTTP headers).

No SSL termination.

All health checks are TCP-based.

Configuration changes require a restart (no hot reload).

No built-in metrics or admin UI.

🛠 Possible Improvements

📊 Metrics export (Prometheus, StatsD)

🔁 Pluggable load balancing strategy (least connections, random, etc.)

🔄 Hot-reloadable configuration (watch for config.json changes)