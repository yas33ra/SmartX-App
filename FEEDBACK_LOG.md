# Feedback Log

Tracks the changes I made in response to lecturer feedback. Statuses are honest: "Deferred" means I did not do it.

## Part 1 feedback

| # | Lecturer feedback | Fix | Status | Commit |
|---|---|---|---|---|
| 1 | Input validation bugs (MAC, location, category) | Shared validator on the API plus client-side checks; duplicate MACs are rejected. MACs are cleaned up (trimmed, upper case, `-` changed to `:`) so `aa-bb-...` and `AA:BB:...` count as the same device. | Done | |
| 2 | Generics: boxing/unboxing concerns | Created and used `TelemetryPacket<T>` with typed telemetry endpoints. | Done | |
| 3 | Operator overloading without structural purpose | Plan was to use `PowerReading` on real telemetry in the dashboard. | Deferred: no dashboard was built, so there was no real place to use it. Not done. | |
| 4 | No jagged/multi-dimensional arrays | Plan was a `HistoricalBatchStore` (`float[][]` flushed into `List<T>`). | Deferred: Part 2 stored history in a `SortedDictionary`, and Part 3 replaced that with an AVL tree. Jagged arrays are still not used. | |
| 5 | Recursion lacks depth safety | Max-depth guard and cycle detection in `ValidateHierarchy()`. | Done | |
| 6 | File attachment not working | Files upload to an `Uploads` folder and the path is saved on the sensor. | Partly done: size limit, extension allow-list and encryption are not done. | |
| 7 | No list used for user-reported issues | An alert tracker keeps active alerts (spikes, disconnects, extreme climate readings) and shows them in the Command Stream page. It uses a `HashSet`, so the same alert is never added twice. | Done, using a `HashSet` instead of a `List<Alert>`. | |
| 8 | Layout broken / no resizing | The Command Stream page, Topology page and main menu resize with the window (`Anchor`, minimum sizes, buttons re-centred on resize). | Partly done: the Part 1 forms were not fully rechecked. | |
| 9 | Lists combined haphazardly with legacy structures | Part 2 uses one clear flow: reading → `Queue` / `PriorityQueue` → store. | Partly done: the Part 1 forms were not reworked. | |
| 10 | README accuracy | Rewritten from the code that exists, covering all three parts, with test steps. | Done | |

## Part 2 feedback, addressed during Part 3

| # | Lecturer feedback | Fix | Status | Commit |
|---|---|---|---|---|
| 1 | No explicit query history `Queue<T>` | `SearchHistory` holds a `Queue<string>` capped at 10 (oldest dropped first). `GET /insights/search-history` serves it, and the Command Stream page shows a "Recent searches" list (double-click to search again). | Done | |
| 2 | No dedicated `HashSet` for active suggestion dedup | `PatternEngine` keeps a `HashSet<string>` of the suggestions on offer. `SetEquals` detects when the set changes, so a suggestion is only announced once. | Done | |
| 3 | Prediction pipeline is polled, not pushed | `SuggestionWatcher` (background service) writes changes into a `Channel<T>` per client, sent over Server-Sent Events (`GET /insights/suggestions/stream`). The page listens on one open connection and reconnects if the API restarts. | Done for suggestions. Devices, alerts and command history still refresh on a 3 second timer. | |

## Part 2 work (self-review)

| # | Requirement | What I built | Status | Commit |
|---|---|---|---|---|
| 1 | Queue / PriorityQueue / Stack | `Queue` for normal readings, `PriorityQueue` for critical ones (processed first), `Stack` for undo. | Done | |
| 2 | Dictionary / SortedDictionary | `Dictionary` for finding devices by MAC and by id. `SortedDictionary` was used for sensor history in Part 2 and replaced by the AVL tree in Part 3. | Done | |
| 3 | HashSet | Alerts are keyed by sensor and alert type, so duplicates are ignored. | Done | |
| 4 | Suggested Actions | A pattern engine counts what I do after an event and suggests it next time. | Done | |
| 5 | Automated Insights | Rules for silent devices, devices near their limit, and unusual readings. | Done | |
| 6 | Device simulator | Console app with keys to test spikes, disconnects and patterns. | Done | |
| 7 | Docker (optional) | `Dockerfile`, `docker-compose.yml` and `.dockerignore` written. | Deferred: Docker Desktop would not start on my computer, so they are untested. Not required by the brief. | |

## Part 3 work

| # | Requirement | What I built | Status | Commit |
|---|---|---|---|---|
| 1 | Tree for timestamped telemetry with range queries | Generic `AvlTree<TKey,TValue>`, one tree per sensor, with range query, latest N and a height check (`GET /history/{id}/stats`). | Done | |
| 2 | Graph of the mesh | `MeshGraph`: undirected weighted adjacency list, gateway vertex, down-device set. | Done | |
| 3 | BFS: connected devices, down devices, where to hop | `MeshRouter.Bfs` with parent pointers; `/mesh/reachable`, `/mesh/route/{node}`, `/mesh/status`. | Done | |
| 4 | Minimum spanning tree for power-efficient routes | Prim's algorithm with a `PriorityQueue`; `/mesh/mst` and `/mesh/mst/route/{node}` compare cheapest and fewest-hops routes. | Done | |
| 5 | Topology page | Drawn mesh with Route, History log and Links tabs; track by MAC or id; mark nodes down or up. | Done | |
| 6 | Main menu with all three modules | Menu opens Sensor Ingestion, Command Stream and Network Topology. | Done | |
| 7 | README, completion report, technology recommendations | README covers all three parts; report and recommendations are in one PDF. | Done | |

## Known limits
- All data is kept in memory, so restarting the API clears devices, links, alerts and learned patterns.
- A spike only counts as a new pattern event after its alert is cleared first.
- Insights depend on timing (2 minutes for "silent", 10 minutes for the suggestion window).
