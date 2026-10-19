# Smart-X IoT System

**Name:** Yaseera  **Student number:** st10456786
**Module:** PROG7312 – Part 2
**Demo video:** [https://youtu.be/KpA9nhaarZs]

Smart-X is a small system that manages IoT devices (sensors and switches).
It has two programs:

- **SmartX.API** – the server. It stores devices and handles their data.
- **SmartX.Client** – the Windows app (Windows Forms) that I use to control everything.

I also made **SmartX.Simulator**, a small console program that pretends to be real devices so I can test things.

---

## What is in Part 2

### 1. Real-Time Command Stream
| What | Data structure | Where it is used |
|---|---|---|
| Incoming readings wait in line | `Queue` | Normal readings are processed in the order they arrive |
| Urgent readings jump the line | `PriorityQueue` | Critical readings (like a 6000 W power spike) are processed first |
| Undo the last command | `Stack` | The last command is removed first (last in, first out) |
| Find a device fast by its MAC address | `Dictionary` | Instant lookup instead of searching a whole list |
| Readings sorted by time | `SortedDictionary` | Each sensor keeps its history in time order |
| No duplicate alerts | `HashSet` | The same alert for the same device is only raised once |

### 2. Predictive Actions and Recommendations
- **Suggested Actions:** The system watches what I do after an alert. If I do the same thing a few times, it suggests it next time (for example: "Turn Pump A OFF").
- **Automated Insights:** The system warns me when a device:
  - stopped sending data (silent for more than 2 minutes),
  - is close to its limit (80% of the critical value),
  - sends a strange reading (far away from its normal values).

---

## How to run it

You need Visual Studio and .NET 10.

1. Open `Solution1.slnx` in Visual Studio.
2. Right-click the solution → **Configure Startup Projects** → choose **Multiple startup projects**.
3. Set `SmartX.API` and `SmartX.Client` to **Start**.
4. Press **F5**. The API runs at `http://localhost:5056`.

### Docker (not tested)
I wrote a `Dockerfile` and a `docker-compose.yml` that would run the API in a container.
**I could not test them.** Docker Desktop would not start on my computer, so I do not know if they work.
To run the project, please use Visual Studio (steps above).
If Docker does work on your computer, the command is `docker compose up --build` and the API would be at `http://localhost:5056`.


### Run the simulator
With the API running, open a terminal in the solution folder and run:

```
dotnet run --project SmartX.Simulator
```

It registers 5 devices (2 climate sensors, 1 power meter, 2 switches) and sends readings every 2 seconds.
Press **Q** to stop it.

---

## How to test the Queue and PriorityQueue

1. Start the API and the simulator.
2. In the simulator press **P**. This **pauses** the processing, so items stay in the queues.
3. Press **S** twice. This sends a **6000 W power spike** (a critical reading).
4. Open `http://localhost:5056/ingestion/status` in a browser.
   You will see `criticalQueued` is **2** and `standardQueued` keeps going up.
5. Press **P** again to resume.
6. Check the status again. The numbers go back to 0, and `processed` goes up.
   The critical readings were handled **before** the normal ones.
7. Open the Command Stream page in the client. A **PowerSpike** alert is shown.

## How to test the Dictionary

1. Open `http://localhost:5056/sensors` to see all the devices.
2. In the client, go to the Command Stream page and type part of a device name in the search box (for example "Pump"). The list filters straight away.
3. Try to register a device with a MAC address that already exists (for example `AA:BB:CC:00:00:01`). The API refuses it with an error, because the MAC address dictionary already has it.
   Writing the MAC in lowercase or with dashes does not work either, because it is cleaned up first.

## How to test the HashSet (no duplicate alerts)

1. In the simulator press **D** (disconnects a sensor).
2. Press **D** again. The second time says the alert was a **duplicate and ignored**.
3. The client shows only **one** alert for that device.
4. Press **R** to clear alerts.

## How to test the Stack (Undo)

1. In the client Command Stream page, select a device and click **Turn On**, then **Turn Off**.
2. Click **Undo**. The last command is reversed.
3. Click **Undo** again until there is nothing left. The app then says "Nothing to undo."

## How to test the SortedDictionary (history)

Open `http://localhost:5056/history/[sensor id]` in a browser (get an id from `/sensors`).
The readings come back in time order, even if they were received out of order.

## How to test Suggested Actions

1. In the simulator press **T**. This teaches the system a pattern: power spike → Pump A OFF → Pump A ON, two times.
2. The client now shows a suggestion like **"Turn Pump A OFF"** with a percentage.
3. Click the suggestion and press **Apply**. The command is sent.

## How to test Automated Insights

| Key in simulator | What it does | Insight shown (after ~30 seconds) |
|---|---|---|
| **N** | Sends 4200 W | "Near limit" |
| **U** | Climate reading of 30 | "Unusual reading" |
| (stop the simulator for 2 minutes) | No data arrives | "Silent" device |

---

## Other useful endpoints

| Address | What it shows |
|---|---|
| `/sensors` | All devices |
| `/devices` | Devices with on/off state and alert count |
| `/alerts` | Active alerts |
| `/ingestion/status` | Queue sizes and processed count |
| `/commands/history` | Commands that were sent |
| `/insights/suggestions` | Suggested actions |
| `/insights/devices` | Automated insights |

The file `SmartX.API.http` has ready-made requests to test these in Visual Studio.

## Things to know
- All data is kept in memory. When the API restarts, everything is cleared.
- Part 1 (registering devices and uploading files) still works from the main menu.

## GitHub
[https://github.com/yas33ra/SmartX-App.git]
