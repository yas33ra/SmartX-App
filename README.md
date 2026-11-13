# Smart-X IoT System

**Name:** Yaseera  **Student number:** st10456786
**Module:** PROG7312 – Part 2
**Demo video:** 

Smart-X is a simulated IoT system for monitoring distributed environments such as hydroponic farms or smart grid installations. Sensors send readings to an API, and a Windows Forms app lets an operator watch the devices, send commands and look at how the devices are connected to each other.

It is built in three parts:

| Part | Module | What it does |
|---|---|---|
| 1 | Sensor management | Register sensors, validate them, ingest readings |
| 2 | Command Stream | Control devices, alerts, suggested actions, insights |
| 3 | Network Topology | Mesh of devices, routing, minimum spanning tree, reading history |

All three open from the main menu of the client.


## Project structure


SmartX-App/
 Solution1/
    SmartX.API/        
    SmartX.Client/     
    SmartX.Simulator/  

All data is kept in memory. If you restart the API, everything is wiped and the simulator registers the devices again.

## How to run it

You need Visual Studio with the **.NET desktop development** and **ASP.NET and web development** workloads, and the .NET 10 SDK.

1. Clone the repository and open `Solution1/Solution1.slnx`.
2. Right-click the solution, choose **Configure Startup Projects**, then **Multiple startup projects**. Set `SmartX.API` and `SmartX.Client` to **Start**, with the API first.
3. Press **F5**. The API listens on `http://localhost:5056` and the main menu opens.
4. Optional: right-click `SmartX.Simulator` and choose **Debug > Start new instance**. It registers 5 test devices, builds the mesh and sends readings.

### Simulator keys

Click the simulator console window, then press a key:

| Key | What it does |
|---|---|
| S | send a normal reading |
| N | send a noisy reading |
| H, U, D, R, P, T | the Part 1 and 2 test actions |
| **M** | build the mesh (the links between devices) |
| **K** | take the next device down (Mains first, then Greenhouse) |
| **L** | bring every device back up |
| Q | quit |

---

## Part 3: Network Topology and Mesh Routing

In a real IoT system the devices do not all talk straight to the server. They pass messages to each other and the messages hop towards a **gateway**. Part 3 models that.

### What the Topology page shows

- A drawing of the mesh. Each circle is a device and each line is a link. Down devices are drawn in a different colour.
- The highlighted lines are the **cheapest tree** that connects every device (the minimum spanning tree).
- **Route tab**: type a MAC address, sensor id or `gateway` (or click a device). The page shows the cheapest route and the fewest-hops route to the gateway. You can mark a device DOWN or UP and watch the routes change.
- **History log tab**: choose a time range and see a sensor's past readings, newest first.
- **Links tab**: add or remove a link between two devices and set its cost.
- The bar at the bottom shows how many devices are connected, down and unreachable, and the cost of the cheapest tree.

### Data structure 1: AVL tree (sensor history)

**Where:** `SmartX.API/Storage/AvlTree.cs`, used by `SensorHistory.cs`.

**What it is:** a binary search tree that keeps itself balanced. Every reading is stored with its timestamp as the key. After each insert the tree checks the height of each side of the nodes it passed through, and does a **rotation** if one side becomes too tall. This keeps the height close to log2(n).

**Why I chose it:** a sensor sends a reading every few seconds, so history grows quickly. A plain list has to look at everything to answer "readings between 10:00 and 10:30". A balanced tree jumps to 10:00 and walks forward. It also never turns into a long chain, which an ordinary search tree can do when timestamps arrive in order.

| Operation | Cost |
|---|---|
| Insert a reading | O(log n) |
| Find one timestamp | O(log n) |
| Range query | O(log n + k), where k is how many readings are returned |
| Latest N readings | O(log n + N) |


Each sensor has its own tree, capped at 20,000 readings (the oldest is removed when it is full). The History tab shows the tree's height next to the readings, so you can see it stays close to perfectly balanced. The same numbers come from `GET /history/{sensorId}/stats`.

### Data structure 2: Graph (the mesh)

**Where:** `SmartX.API/Storage/MeshGraph.cs`.

**What it is:** devices are **vertices** and links are **edges**. Each link has a **cost** that stands for power use. Links work in both directions, so the graph is undirected and weighted. The gateway is a vertex with a fixed id.

**How it is stored:** as an **adjacency list**, where each device keeps a list of its neighbours and the cost to each one. I chose this over a matrix because a mesh has only a few links per device, so a matrix would be mostly empty.


Devices that are DOWN are kept in a separate set and the algorithms skip them. Taking a device down does not delete its links, so bringing it back is instant.

### Algorithm 1: Breadth-First Search (BFS)

**Where:** `Bfs` and `RouteToGateway` in `SmartX.API/Storage/MeshRouter.cs`.

**What it does:** starting from a device, BFS visits every device one hop away, then every device two hops away, and so on. It uses a **queue**. While it runs it remembers the **parent** of each device (who it was reached from).

It answers the three questions in the brief:

- **Which devices are connected?** Everything BFS reaches.
- **Which devices are down?** The down set, reported by `GET /mesh/status`.
- **Where should a message hop next?** Follow the parents back towards the gateway. The first one is the **next hop**.


BFS finds the route with the **fewest hops** and runs in O(V + E). When a device goes down, BFS finds another way round if there is one. Devices with no path left are reported as **cut off**.

### Algorithm 2: Prim's minimum spanning tree

**Where:** `MinimumSpanningTree` and `RouteViaMst` in `MeshRouter.cs`.

**What it does:** finds the set of links that connects every reachable device for the **lowest total cost**, which means the most power-efficient routes. Prim's starts at the gateway and keeps adding the cheapest link that reaches a new device. A **priority queue** (a binary min-heap) hands back the cheapest link each time. Ties are broken by the order the links were added, so the result is always the same.


**The cheapest route is not always the fewest hops.** For example, from Valve B the cheapest route costs 7 and takes 3 hops, but the fewest-hops route takes 2 hops and costs 10. The Route tab shows both and lets you highlight either one.

### Part 3 API endpoints

| Method and path | What it does |
|---|---|
| `GET /mesh` | all devices (MAC, category, down status) and all links |
| `POST /mesh/links` | add a link (`from`, `to`, `cost`) |
| `DELETE /mesh/links?from=&to=` | remove a link |
| `POST /mesh/nodes/{node}/down` and `/up` | take a device down or bring it back |
| `GET /mesh/reachable?from=` | devices reachable from a device (BFS) |
| `GET /mesh/route/{node}` | fewest-hops route to the gateway |
| `GET /mesh/status` | connected, down, cut off and healing devices |
| `GET /mesh/mst` | the minimum spanning tree and its total cost |
| `GET /mesh/mst/route/{node}` | cheapest route compared with fewest-hops route |
| `GET /history/{sensorId}` | readings, with optional time range and limit |
| `GET /history/{sensorId}/stats` | AVL tree count, height and ideal height |


---

## Part 2: Command Stream

This page lets the operator control devices and shows what the system has learned.

| Feature | Data structure | Why |
|---|---|---|
| Command history and undo | **Stack** | undo reverses the most recent command first (last in, first out) |
| Critical alerts handled in order | **Priority queue** | the most urgent alert is handled first |
| Alert deduplication | **HashSet** | the same alert for the same sensor is only shown once |
| Find a device by MAC or id | **Dictionary** | instant lookup instead of searching a list |
| Recent searches | **Queue** (maximum 10) | the oldest search leaves first when the list is full |
| Suggested actions | **HashSet** of active suggestions | a suggestion is only announced when it first appears |

**Predictive action engine.** The engine learns which action the operator usually does after a certain event (for example after an alert, or after a search). It counts these pairs. When a pair has happened at least twice and makes up at least 40% of the follow-ups, it becomes a suggestion with its evidence, for example "After you searched pump, you did this 2 of 2 times."

**Pushed suggestions.** The page does not poll for suggestions. A background service (`SuggestionWatcher`) checks every second whether the suggestions changed, using the HashSet. If they did, it writes the new list into a `Channel<T>` for each connected client, and the API sends it over a **Server-Sent Events** connection (`GET /insights/suggestions/stream`). The client keeps that connection open and updates the list the moment something arrives, and reconnects by itself if the API restarts. Devices, alerts and the command history still refresh on a 3-second timer.

**Automated insights.** Rules look at each sensor's recent readings and flag a device that has gone silent, is close to a critical limit, or is reading far from its own recent average.

## Part 1: Sensor management

Sensors are registered with a name, category (Climate, Electrical or Control), deployment location and MAC address. The API validates every field and returns clear error messages. Readings are stored and checked against critical rules to raise alerts.

### Design decisions

**HTTP instead of HTTPS (local development).** The project uses plain HTTP (`http://localhost:5056`). A TLS handshake problem could not be resolved on my development machine (confirmed with the browser and `curl`, with a freshly regenerated trusted certificate). Everything stays on `localhost`, so HTTP is an accepted choice for local work. In production, HTTPS would be enforced at the hosting layer.

**`DeploymentLocation` recursion.** A location is a chain of `DeploymentLocation` objects (Facility, Zone, Sub-Zone), each pointing to its parent. A recursive `ValidateHierarchy()` walks up the chain to check every level before a sensor can register.

**`TelemetryPacket<T>` has no `SensorName`.** It carries only a `SensorId`. The name is looked up from the registry when needed, so a renamed sensor never leaves stale names in old packets.

**Sensor registry.** Sensors are stored in a `Dictionary<Guid, Sensor>` for instant lookup by id and MAC (upgraded from the `List<Sensor>` used in Part 1).

---

## Testing

`SmartX.API/SmartX.API.http` contains ready-made requests you can run from Visual Studio with **Send request**. It is grouped like this:

- Part 1 and 2 requests: registering sensors, readings, alerts, commands, suggestions
- **M1 to M16**: building and changing the mesh
- **B1 to B15**: BFS, reachable devices, routes and down devices
- **P1 to P10**: minimum spanning tree and route comparison
- **H1 to H3**: search history and suggestions

The API keeps everything in memory, so restart it for a clean start.

## Docker (written but NOT tested)

The project contains a `Dockerfile`, `docker-compose.yml` and `.dockerignore` for the API. **I could not test them**, because Docker Desktop would not start on my computer. They may need changes before they work. The project is meant to be run from Visual Studio as described above.

## Known limitations

- All data is in memory. Restarting the API wipes sensors, links, alerts and learned patterns.
- Sensor locations are typed in as text. A reusable list of locations is something I would add next.
- The mesh is built from the links that are added. It does not discover devices on its own.
- Docker files are untested (see above).
- Some Part 1 feedback items are not finished. See `FEEDBACK_LOG.md` for the honest status of each one.


[https://github.com/yas33ra/SmartX-App.git]
