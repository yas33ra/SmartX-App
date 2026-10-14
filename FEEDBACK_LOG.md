# Feedback Log

Tracks every change made in response to lecturer feedback. Part 3 requires this file to be submitted, so add an entry each time a fix is committed.

## Part 1 feedback

| # | Lecturer feedback | Planned fix | Status | Commit |
|---|---|---|---|---|
| 1 | Input validation bugs (MAC, location, category) | Shared validator on the API plus client-side checks; reject duplicate MACs | Done | |
| 2 | Generics: boxing/unboxing concerns | Create and use `TelemetryPacket<T>` with typed telemetry endpoints | Done | |
| 3 | Operator overloading without structural purpose | Use `PowerReading` on real telemetry in the dashboard | To do | |
| 4 | No jagged/multi-dimensional arrays | `HistoricalBatchStore` (`float[][]` flushed into `List<T>`) | To do | |
| 5 | Recursion lacks depth safety | Max-depth guard and cycle detection in `ValidateHierarchy()` |  Done | |
| 6 | File attachment not working | Harden upload (size limit, extension allow-list, safe filename, encryption); show attachment in UI | To do | |
| 7 | No list used for user-reported issues | Engagement feature plus `List<Alert>` (spikes and disconnects) | To do | |
| 8 | Layout broken / no resizing | `Dock`/`Anchor`, `TableLayoutPanel`, consistent typography | To do | |
| 9 | Lists combined haphazardly with legacy structures | One clear pipeline: raw array to generic `List<T>` | To do | |
| 10 | README accuracy | Rewrite from the code that exists | To do | |
