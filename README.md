# SmartX
# Part 1: Data Ingestion and Validation 
 
## Overview
 
Smart-X is a system designed to monitor environments, climate controls etc(e.g. Moisture sensor, valves, electrical readings). This repository contains Part 1 of the Smart-X Portfolio of Evidence: the basic data ingestion, consisting of a .NET Minimal API backend (`SmartX.Api`) and a Windows Forms client (`SmartX.Client`).
 

 
- Visual Studio 2026 (or later) with the following installed:
  - .NET desktop development
  - ASP.NET and web development
- .NET 10 SDK (or the version targeted by both projects, confirm via each project's Properties)
 

## Setup and Running Instructions
 
1. Clone this repository:

  		git clone <repository-url>
 
2. Open `Solution1.sln` in Visual Studio.
3. Restore NuGet packages 
4. Set both `SmartX.Api` and `SmartX.Client` as startup projects:
5. Run the application 
6. The API console window will display `Now listening on: http://localhost:5056` and the WinForms  page will open automatically.
 

## Design Decisions
  
### HTTP instead of HTTPS (local development)
 
This project communicates over plain HTTP (`http://localhost:5056`) rather than HTTPS. An unresolved TLS/schannel handshake issue was encountered in this specific environment (confirmed via both browser and `curl` testing, across a freshly regenerated and trusted development certificate). HTTP is an accepted and common practice for local development. 

 
### `SensorCategory` naming: Climate / Electrical / Control
 
The brief's example categories (Environmental, Power Consumption, Actuator) are implemented as `Climate`, `Electrical`, and `Control`. 

 
### `DeploymentLocation` (hierarchy validation)
 
Sensor deployment location is structured as follows (Facility → Zone → Sub-Zone), each referencing its parent. A recursive `ValidateHierarchy()` method is used to confirm every level is linked before a sensor is permitted to register. 

 
### `TelemetryPacket<T>` excludes `SensorName`
 
`TelemetryPacket<T>` carries a `SensorId` only, not a duplicated `SensorName`. The sensor's name is looked up via the `SensorId` against the sensor registry when needed, rather than being duplicated across every telemetry packet. This to avoid inconsistent data if a sensor is ever renamed, and sets up the`Dictionary<Guid, Sensor>` coming in part 2
 

### `List<Sensor>` storage (Part 1)
 
Registered sensors are currently stored in a `List<Sensor>` in `SensorRegistry`. This is deliberate for Part 1, as required by the brief and will be upgraded to a `Dictionary<Guid, Sensor>` in Part 2 for search by unique identifier.
 

## Advanced OOP Concepts Implemented (Part 1)
 
 Generics :`TelemetryPacket<T>` 
 Operator overloading : `PowerReading` (`+`, `>`, `<`)   (will be implemented fully in the next part )
 Recursion : `DeploymentLocation.ValidateHierarchy()` 
 Collections : `SensorRegistry` (`List<Sensor>`) 
 
## Known Limitations
 
- Data is stored in-memory only; restarting the API clears all registered sensors and telemetry (acceptable for this part of the assessment as per the brief's note on data seeding).
- HTTPS is disabled for local development (see above)
