# Feedback Log

Track changes made in response to lecturer feedback. 

## Part 1 feedback

| # | Lecturer feedback | Fix | Status | Commit |
|---|---|---|---|---|
| 1 | Input validation bugs (MAC, location, category) | Shared validator on the API plus client-side checks; duplicate MACs are rejected. MACs are cleaned up (trimmed, upper case, `-` changed to `:`) so `aa-bb-...` and `AA:BB:...` count as the same device. | Done | |
| 2 | Generics: boxing/unboxing concerns | Created and used `TelemetryPacket<T>` with typed telemetry endpoints. | Done | |
| 3 | Operator overloading without structural purpose | Plan was to use `PowerReading` on real telemetry in the dashboard. | Deferred: no dashboard was built in Part 2, so there was no real place to use it. Not done. | |
| 4 | No jagged/multi-dimensional arrays | Plan was a `HistoricalBatchStore` (`float[][]` flushed into `List<T>`). | Deferred: Part 2 used `SortedDictionary` for sensor history instead. Not done. | |
| 5 | Recursion lacks depth safety | Max-depth guard and cycle detection in `ValidateHierarchy()`. | Done | |
| 6 | File attachment not working | Files upload to an `Uploads` folder and the path is saved on the sensor. | Partly done (confirm): extra hardening (size limit, extension allow-list, encryption) is not done. | |
| 7 | No list used for user-reported issues | Added an alert tracker that keeps active alerts (spikes, disconnects, extreme climate readings) and shows them in the Command Stream page. It uses a `HashSet` so the same alert is never added twice. | Done, but it uses a `HashSet`, not a `List<Alert>` (confirm). | |
| 8 | Layout broken / no resizing | The Command Stream page uses `Anchor`, a minimum size, and a resizable layout. | Partly done (confirm): other Part 1 forms are not fully checked. | |
| 9 | Lists combined haphazardly with legacy structures | Part 2 uses one clear flow: reading → `Queue` / `PriorityQueue` → store. | Partly done: the Part 1 forms were not reworked. | |
| 10 | README accuracy | Rewritten from the code that exists, with test steps for each data structure. | Done (once the README is committed) | |

## Part 2 work (self-review)

| # | Requirement | What I built | Status | Commit |
|---|---|---|---|---|
| 1 | Queue / PriorityQueue / Stack | `Queue` for normal readings, `PriorityQueue` for critical ones (processed first), `Stack` for undo. | Done | |
| 2 | Dictionary / SortedDictionary | `Dictionary` for finding devices by MAC and by id. `SortedDictionary` for each sensor's history, sorted by time. | Done | |
| 3 | HashSet | Alerts are keyed by sensor and alert type, so duplicates are ignored. | Done | |
| 4 | Suggested Actions | A pattern engine counts what I do after an alert and suggests it next time. | Done | |
| 5 | Automated Insights | Rules for silent devices, devices near their limit, and unusual readings. | Done | |
| 6 | Device simulator | Console app with keys to test spikes, disconnects and patterns. | Done | |
| 7 | Docker (optional) | `Dockerfile` and `docker-compose.yml` written. | Deferred: Docker Desktop would not start on my computer, so I could not test them. Not required by the brief. | |

## Known limits
- All data is kept in memory, so restarting the API clears devices, alerts and learned patterns.
- A spike only counts as a new pattern event after its alert is cleared first.
- Insights depend on timing (2 minutes for "silent", 10 minutes for the suggestion window).