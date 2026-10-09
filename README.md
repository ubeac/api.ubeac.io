# uBeac IoT Platform: Backend (`api.ubeac.io`)

uBeac was a multi-tenant IoT platform for connecting off-the-shelf gateways, phones and custom devices, decoding their data, and turning it into live dashboards. This repository is its backend: ingestion over HTTP and MQTT, a queue-driven processing pipeline, the REST API, the identity server and the real-time push service.

Created by [Momentaj](https://momentaj.com/), a Toronto AI engineering firm.

> **Status: retired, published for reference.** uBeac was developed from 2017 to 2020 and ran as a hosted service at `ubeac.io`. The hosted service has been retired, and the source is published here under the MIT license. The code is a 2019 snapshot on .NET Core 2.2, which is out of support. **Read [Security notes](#17-known-limitations-and-security-notes) before running any part of it.**

| Repository | What it is |
|---|---|
| **[api.ubeac.io](https://github.com/ubeac/api.ubeac.io)** (this repo) | .NET backend: ingestion hubs, processing workers, REST API, identity, real-time |
| [ui.ubeac.io](https://github.com/ubeac/ui.ubeac.io) | Vue customer web app (`app.ubeac.io`): onboarding, gateways, devices, dashboards |
| [admin.ubeac.io](https://github.com/ubeac/admin.ubeac.io) | The first operator console and UX prototypes (2017 to 2018), later absorbed into the web app |
| [OSMonitoring](https://github.com/ubeac/OSMonitoring) | Edge agent: a computer's own sensors (CPU, memory, disks, network, temperatures) as a uBeac device |
| [SBCGateway](https://github.com/ubeac/SBCGateway) | Edge agent: a Raspberry Pi as a BLE, Bluetooth and Wi-Fi scanning gateway |

Public user documentation from the service's era lives at [ubeac.github.io/docs](https://ubeac.github.io/docs/).

## Contents

1. [At a glance](#1-at-a-glance)
2. [Why uBeac: how it compares](#2-why-ubeac-how-it-compares)
3. [Tech stack](#3-tech-stack)
4. [High-level architecture](#4-high-level-architecture)
5. [Runtime topology](#5-runtime-topology)
6. [The ingestion pipeline](#6-the-ingestion-pipeline)
7. [Gateway decoders](#7-gateway-decoders)
8. [Tenancy, identity and security model](#8-tenancy-identity-and-security-model)
9. [Real-time delivery](#9-real-time-delivery)
10. [Data model and storage](#10-data-model-and-storage)
11. [REST API](#11-rest-api)
12. [Solution structure](#12-solution-structure)
13. [Getting started (local development)](#13-getting-started-local-development)
14. [Developer workflows](#14-developer-workflows)
15. [Configuration reference](#15-configuration-reference)
16. [Deployment (as it ran)](#16-deployment-as-it-ran)
17. [Known limitations and security notes](#17-known-limitations-and-security-notes)
18. [History](#18-history)
19. [License](#19-license)

---

## 1. At a glance

| | |
|---|---|
| **What it does** | Accepts sensor data from gateways over HTTP(S) and MQTT(S), decodes vendor payloads, auto-registers devices and sensors, stores raw and time-series data per tenant, and streams everything live to browsers |
| **Deployable services** | 5 web services and 6 background workers |
| **Shared libraries** | 20 component libraries, plus 4 developer tool projects and a browser test page |
| **Code size** | 378 C# files, about 16,000 lines of code (excluding blanks and comments) |
| **Messaging** | 7 RabbitMQ work queues, MessagePack + LZ4 payloads |
| **Storage** | MongoDB replica set: 4 logical databases plus one log database per service |
| **Device support** | 15 decoders (including a CSV prototype) for BLE gateways, phone apps and generic JSON; iBeacon, Eddystone, Ruuvi and Minew parsing built in |
| **Sensor catalog** | 24 sensor types, 38 units, SI prefixes from 10⁻²⁴ to 10²⁴ |
| **REST surface** | 45 actions across 12 controllers, plus 11 account endpoints on the identity server |

## 2. Why uBeac: how it compares

uBeac was built for a specific kind of customer: a facility, retailer or integrator with a building full of Bluetooth beacons and sensors, a few off-the-shelf BLE-to-Wi-Fi gateways, and no appetite for writing firmware or cloud glue code. The design choices below follow from that customer, and they are what set uBeac apart from general-purpose IoT platforms of its time (2018 to 2020).

**1. Off-the-shelf gateways work as they ship.** Most commercial BLE gateways can already POST their scan results to a URL or publish them over MQTT, but each vendor uses its own payload format. uBeac meets them where they are. A user picks the gateway model from a catalog, pastes a URL into the gateway's settings page, and uBeac decodes the vendor format server-side. Nothing is installed on the gateway, and no code is written by the user.

**2. Decoders are data, not deployments.** Each gateway firmware in the catalog carries its decoder as C# source. The processing worker compiles all of them at start-up with Roslyn. Supporting a new gateway model means adding a catalog entry and restarting one worker, not shipping a new release of the platform.

**3. Deep BLE knowledge is built in.** Shared parsers handle iBeacon and all four Eddystone frame types (UID, URL, TLM, EID). The decoders add Ruuvi RAWv1 and Minew sensor frames, and unpack vendor envelopes for Minew, Ingics, Jaalee, April Brothers (binary MessagePack), BlueCats and Mist. On a general platform, this parsing is code the customer writes.

**4. Devices provision themselves.** The first time a known device reports, uBeac creates the device and its sensors and infers each sensor's data schema from the payload. New data keys are added as they appear. Nearby BLE beacons are deliberately *not* auto-registered, which keeps a busy lobby from flooding the account. They appear in a live "unregistered devices" view, where one click adopts them.

**5. Buildings and floors are first-class.** The data model is Team → Building → Floor (with an uploaded floor-plan image) → Gateway and Device, each placed at a position on the plan. Indoor views are a core feature, not a custom widget.

**6. Real time is the default, down to the raw request.** Every stage pushes to the browser over SignalR, from each raw HTTP or MQTT request to each decoded sensor value. The web app shows a live inspector per gateway with the raw payload, the decoded devices and any decoder exceptions side by side. The most common question in IoT integration, "why isn't my data showing up?", is answered on one screen.

**7. Two minutes to a first dashboard.** The onboarding wizard shows a QR code; scanning it turns a phone into a gateway that streams its own sensors. uBeac detects the first data live and generates a default dashboard bound to those sensors.

**8. Tenant isolation at every layer.** Each team gets its own ingestion subdomain (`{namespace}.hub.ubeac.io`), its own time-series collections with a retention TTL, and its own MQTT topic space. The MQTT hub rewrites topics per team, so it doubles as a private broker for the team's own subscribers.

**9. A pipeline sized for one small team to run.** Six single-purpose workers connected by queues can each be scaled, restarted or debugged on its own. MessagePack with LZ4 keeps queue traffic small, counters are updated incrementally at write time, and MongoDB change streams keep every service's routing cache current without polling.

### Comparison

A summary of capabilities as they stood during uBeac's active years (2018 to 2020). Other platforms have changed since, so check their current documentation.

| | **uBeac** | **AWS IoT Core / Azure IoT Hub** | **ThingsBoard (open source)** | **Ubidots / ThingSpeak (hosted dashboards)** |
|---|---|---|---|---|
| Connecting an off-the-shelf BLE gateway | Pick the model from a catalog; decoding is server-side and built in | Register a device identity (X.509 or key-based) and write your own payload transformation | BLE through the separate ThingsBoard IoT Gateway service on a Bluetooth-capable host, mapped per device in JSON, or custom converters | Send the platform's own format, or write a transformation function |
| Device provisioning | Automatic from traffic, with schema inference and beacon adoption | Explicit registration (bulk and fleet provisioning available) | Create each device and its access token (provisioning APIs available) | Ubidots creates devices on first post; ThingSpeak channels are created by hand |
| Dashboards | Built in: 6 widget types, including indoor floor plans and custom-code widgets; generated during onboarding | Not part of the core service; a separate product or tool | Rich built-in dashboards | Built in |
| Indoor model | Buildings, floors and positioned devices as core entities | Generic device metadata | Generic assets; floor plans through image-map widgets | Generic devices |
| Live debugging | Raw payload, decoded result and decoder errors per request, in the browser | Logs and metrics in a separate console | Debug mode on rule chains | Limited |
| Protocols | HTTP(S), MQTT(S), MQTT over WebSockets | MQTT, HTTPS, plus AMQP (Azure) and LoRaWAN (AWS) | MQTT, HTTP, CoAP, LwM2M, SNMP | HTTP, MQTT |
| Rules, alerts, device commands | Not built | Yes | Yes (rule engine) | Yes (alerts) |

**Where others were stronger.** uBeac never shipped a rules or alerting engine, device commands or over-the-air updates, and it did not cover CoAP or LoRaWAN. Device identity was per gateway (URL, optional headers, IP lists, MQTT credentials) rather than per-device certificates. Hyperscaler platforms also bring global scale and compliance programs that a small platform can't match. uBeac's bet was different: the fastest path from "I bought a gateway" to "I see my building live."

## 3. Tech stack

| Area | Technology |
|---|---|
| Runtime | .NET Core 2.2, ASP.NET Core 2.2, C# 7.1 and later |
| Identity | IdentityServer4 2.4 with ASP.NET Core Identity and a custom MongoDB user store |
| Messaging | RabbitMQ (RabbitMQ.Client 5.1), MessagePack-CSharp 1.7 with LZ4 compression |
| Database | MongoDB (driver 2.8.1), run as a replica set because the services depend on change streams; Decimal128 values and GeoJSON points |
| MQTT broker | MQTTnet 3.0.8 (TCP and WebSockets), hosted inside ASP.NET Core |
| Real time | ASP.NET Core SignalR 1.1 |
| Runtime code compilation | Roslyn (Microsoft.CodeAnalysis.CSharp 3.1) |
| Logging | Serilog 2.8 with a MongoDB sink (one collection per level) and rich enrichers |
| API documentation | Swashbuckle 4.0 (Swagger UI at `/doc`) |
| Mapping and validation | AutoMapper (through AutoMapper.Extensions.Microsoft.DependencyInjection 6.1), DataAnnotations with a global validation filter |
| Other | IPAddressRange 3.2 (IP allow and deny lists), Newtonsoft.Json 12, MsgPack.Cli 1.0 (binary gateway payloads) |
| Hosting | IIS (in-process) for web services, Windows Services for workers |

## 4. High-level architecture

```mermaid
flowchart LR
  subgraph Edge["Devices and gateways"]
    BLE["BLE-to-Wi-Fi gateways<br/>Minew, Ingics, Jaalee,<br/>April Brothers, BlueCats, Mist"]
    PH["Phones<br/>websensor web app,<br/>Android and iOS sensor apps"]
    DIY["Custom devices<br/>ESP32, Raspberry Pi, Pycom"]
  end
  subgraph Platform["uBeac backend (this repository)"]
    ING["Ingestion<br/>HttpHub and MqttHub"]
    PIPE["Processing pipeline<br/>RabbitMQ and 6 workers"]
    DB[("MongoDB<br/>replica set")]
    APP["Application services<br/>REST API, Identity, SocketApi"]
  end
  UI["Web app<br/>ui.ubeac.io"]
  SUB["Team MQTT subscribers"]
  BLE -- "HTTP(S) or MQTT(S)" --> ING
  PH -- "HTTP(S)" --> ING
  DIY -- "HTTP(S) or MQTT(S)" --> ING
  ING -- "enqueue" --> PIPE
  PIPE -- "store" --> DB
  PIPE -- "live data" --> APP
  DB -- "queries and change streams" --> APP
  APP -- "REST, OAuth 2.0, SignalR" --> UI
  ING -- "per-team topic relay" --> SUB
```

Three ideas carry the design:

- **Ingest fast, process later.** The hubs do only what must happen on the request path: resolve the tenant and gateway from an in-memory cache, check the gateway's security settings, and enqueue. Everything else happens in workers.
- **One envelope through the whole pipeline.** A single `GatewayData` message travels from hub to browser, and each stage enriches it: raw body, then decoded devices, then resolved entity IDs.
- **The database is also the event bus for configuration.** When a user edits a gateway, device or dashboard through the API, MongoDB change streams push the change to every service that caches it, and to the browser.

## 5. Runtime topology

The platform has two paths. The **data path** carries sensor traffic from gateways to storage and to the browser. The **control path** carries user actions (creating a gateway, editing a dashboard) and propagates them to every service that caches configuration.

### 5.1 Data path

Edge labels are RabbitMQ queue names.

```mermaid
flowchart LR
  GW["Gateways and devices"]
  HH["uBeac.HttpHub<br/>{namespace}.hub.ubeac.io"]
  MH["uBeac.MqttHub<br/>MQTT 1883, WebSocket /mqtt"]
  D["Dispatcher"]
  P["Processor<br/>firmware decoders"]
  PP["PostProcessor<br/>auto-provisioning"]
  GP["GatewayProcessor"]
  SP["SensorProcessor"]
  DP["DeviceProcessor"]
  SOCK["uBeac.SocketApi<br/>SignalR hub"]
  UI["Browser"]
  M3[("GatewayData_{team}<br/>raw requests, 90-day TTL")]
  M2[("SensorData_{team}<br/>time series, 90-day TTL")]
  M4[("DeviceSummary")]
  M1[("uBeac main DB<br/>devices, sensors, gateways")]

  GW -- "HTTP(S)" --> HH
  GW -- "MQTT(S)" --> MH
  HH -- toDispatcher --> D
  MH -- toDispatcher --> D
  D -- toProcessor --> P
  P -- toPostProcessor --> PP
  D -- "toPostProcessor<br/>(already structured)" --> PP
  PP -- "create devices and sensors" --> M1
  PP -- toGatewayProcessor --> GP
  PP -- toSensorProcessor --> SP
  PP -- toDeviceProcessor --> DP
  GP -- "archive" --> M3
  GP -- "request counters" --> M1
  SP -- "bulk insert" --> M2
  DP -- "upsert" --> M4
  GP -- toGatewayDataWebSocket --> SOCK
  SOCK -- "SignalR groups" --> UI
```

### 5.2 Control path

```mermaid
flowchart LR
  UI["Browser<br/>app.ubeac.io"]
  IDS["uBeac.Idsrv<br/>idsrv.ubeac.io"]
  API["uBeac.Api<br/>api.ubeac.io"]
  M1[("uBeac main DB")]
  DATA[("SensorData, GatewayData,<br/>DeviceSummary")]
  HUBS["HttpHub and MqttHub<br/>team and gateway cache"]
  PP["PostProcessor<br/>device and sensor cache"]
  SOCK["uBeac.SocketApi"]

  UI -- "1. password grant" --> IDS
  IDS -- "users and profiles" --> M1
  UI -- "2. REST with Bearer token" --> API
  API -- "entities" --> M1
  API -- "history queries" --> DATA
  M1 -. "3. change streams" .-> HUBS
  M1 -. "change streams" .-> PP
  M1 -. "change streams" .-> SOCK
  SOCK -- "4. ChangeLog/{team}" --> UI
```

A user who adds a gateway in the web app calls the API, which writes to MongoDB. The hubs pick up the new gateway from the change stream within moments and start accepting its traffic. SocketApi sends a `ChangeLog` event, and every open browser on that team updates its gateway list. No service polls, and no service needs a restart.

### 5.3 Processes

| Process | Kind | Responsibility |
|---|---|---|
| `uBeac.HttpHub` | Web service | HTTP ingestion. Resolves tenant and gateway from the host name and path, enforces gateway security and a per-gateway rate limit, and enqueues |
| `uBeac.MqttHub` | Web service with an embedded MQTT broker | MQTT ingestion over TCP and WebSockets. Authenticates gateways by client ID, enqueues, and relays messages to subscribers in the same team |
| `uBeac.Dispatcher` | Worker | Routes each message to decoding, or straight to post-processing when it is already structured |
| `uBeac.Processor` | Worker | Runs the gateway's firmware decoder to turn the raw payload into devices and sensor readings |
| `uBeac.PostProcessor` | Worker | Auto-provisions devices and sensors, resolves them to IDs, and fans out to three queues |
| `uBeac.GatewayProcessor` | Worker | Archives the raw request, updates gateway counters, and forwards the message for live delivery |
| `uBeac.SensorProcessor` | Worker | Writes sensor readings to the team's time-series collection |
| `uBeac.DeviceProcessor` | Worker | Maintains per-device summaries: first and last seen, request count |
| `uBeac.Api` | Web service | REST management API for teams, buildings, gateways, devices, dashboards and the device catalog |
| `uBeac.Idsrv` | Web service | OAuth 2.0 and OpenID Connect token service, registration and account flows |
| `uBeac.SocketApi` | Web service | SignalR hub that pushes live data and entity changes to browsers |

## 6. The ingestion pipeline

### 6.1 End to end

```mermaid
sequenceDiagram
  autonumber
  participant G as Gateway
  participant H as HttpHub or MqttHub
  participant D as Dispatcher
  participant P as Processor
  participant PP as PostProcessor
  participant GP as GatewayProcessor
  participant SP as SensorProcessor
  participant DP as DeviceProcessor
  participant S as SocketApi
  participant B as Browser
  G->>H: POST {ns}.hub.ubeac.io/{gatewayUrl}, or MQTT publish to {gatewayUrl}
  H->>H: resolve team and gateway (cache), check security, rate limit
  H->>D: toDispatcher: GatewayData (Body, plus Bytes over HTTP)
  alt already structured (URL or topic addressed a single sensor)
    D->>PP: toPostProcessor
  else raw vendor payload
    D->>P: toProcessor (original bytes)
    P->>P: firmware decoder fills RawDevices
    P->>PP: toPostProcessor
  end
  PP->>PP: create unknown devices and sensors, infer schema, resolve IDs
  par raw archive and live view
    PP->>GP: toGatewayProcessor
    GP->>GP: insert GatewayData_{team}, increment gateway counters
    GP->>S: toGatewayDataWebSocket
    S-->>B: SignalR onGroupData
  and time series
    PP->>SP: toSensorProcessor
    SP->>SP: insert into SensorData_{team}
  and device summary
    PP->>DP: toDeviceProcessor
    DP->>DP: upsert DeviceSummary
  end
```

### 6.2 Stage by stage

1. **Ingestion.** `HttpHub` accepts `POST`, `PUT`, `PATCH` or `GET` on `{namespace}.hub.ubeac.io/{gatewayUrl}` and copies the body into the envelope as both text and bytes. `MqttHub` accepts a publish to topic `{gatewayUrl}` and keeps only the text form, so decoders for binary payloads (April Brothers) work over HTTP only. Both also accept an already-structured form that skips decoding: `.../{gatewayUrl}/devices/{deviceUid}/sensors/{sensorUid}`, with the value in the query string (HTTP) or the payload (MQTT).
2. **Dispatcher.** A message with structured devices and no body goes straight to `toPostProcessor`. Everything else goes, byte for byte, to `toProcessor`.
3. **Processor.** Looks up the decoder compiled for the gateway's `FirmwareId` and calls `Process(gatewayData)`, which fills `RawDevices` from `Body` or `Bytes`. See [Gateway decoders](#7-gateway-decoders).
4. **PostProcessor.** Holds an in-memory model of every team's devices and sensors, kept current by change streams. For each decoded device it:
   - adds unknown sensors to known devices, and adds new data keys to a sensor's `Schema`;
   - creates the device and all its sensors if the device is unknown **and** the decoder marked it `IsValid`;
   - resolves UIDs to entity IDs and builds the `Devices` list.

   It then publishes to three queues in parallel. Sensors with `Persist = false` are stripped from the time-series copy, so they show up live but are not stored.
5. **GatewayProcessor.** Forwards the full envelope for live delivery, sets `LastRequestDate` and increments `RequestCount` on the gateway, and archives the whole request in `GatewayData_{teamId}`.
6. **SensorProcessor.** Flattens every reading and bulk-inserts (unordered) into `SensorData_{teamId}`. Location maps such as `{lat, long}` are stored as GeoJSON points.
7. **DeviceProcessor.** Upserts one `DeviceSummary` per team, gateway and device, with `$inc` on the request count and `$setOnInsert` for the first-seen date.
8. **SocketApi.** Drops messages older than 60 seconds, then pushes to SignalR groups at team, gateway, device and sensor level, including wildcard variants. See [Real-time delivery](#9-real-time-delivery).

### 6.3 Queues

All queues use RabbitMQ's default exchange, with the routing key equal to the queue name. Each consumer acknowledges manually and forwards failed messages to an exception queue named `_{queue}`.

| Queue | Producer | Consumer | Prefetch | Payload |
|---|---|---|---|---|
| `toDispatcher` | HttpHub, MqttHub | Dispatcher | 200 | Raw envelope |
| `toProcessor` | Dispatcher | Processor | 200 | Raw envelope, unchanged bytes |
| `toPostProcessor` | Dispatcher, Processor | PostProcessor | 200 | Envelope with `RawDevices` |
| `toGatewayProcessor` | PostProcessor | GatewayProcessor | 200 | Full envelope |
| `toSensorProcessor` | PostProcessor | SensorProcessor | 200 | Slim envelope: IDs, time, `Devices` (persisted sensors only) |
| `toDeviceProcessor` | PostProcessor | DeviceProcessor | 100 | Slim envelope: `Devices` |
| `toGatewayDataWebSocket` | GatewayProcessor | SocketApi | none | Full envelope |

### 6.4 The message envelope

Every queue carries the same `GatewayData` type (`Components/uBeac.Models`), serialized with MessagePack and LZ4.

```mermaid
classDiagram
  direction LR
  class GatewayData {
    Guid Id
    string TraceId
    Guid TeamId
    Guid GatewayId
    Guid FirmwareId
    Guid? FloorId
    DateTime DateTime
    string Url
    string RequestProtocol
    string RequestMethod
    string Body
    byte[] Bytes
    List~string~ Exceptions
  }
  class DeviceRawData {
    string Uid
    DateTime DateTime
    bool IsValid
  }
  class SensorRawData {
    string SensorUid
    SensorTypes Type
    SensorUnits Unit
    SensorPrefixes Prefix
    DateTime DateTime
    Map Data
  }
  class DeviceData {
    Guid Id
    string Uid
    Guid GatewayId
    DateTime DateTime
  }
  class SensorData {
    Guid SensorId
    Guid GatewayId
    DateTime DateTime
    Map Data
  }
  GatewayData "1" *-- "many" DeviceRawData : RawDevices (decoded)
  DeviceRawData "1" *-- "many" SensorRawData : Sensors
  GatewayData "1" *-- "many" DeviceData : Devices (resolved)
  DeviceData "1" *-- "many" SensorData : Sensors
```

`Data` is a `Dictionary<string, decimal>`: `{ "value": 22.5 }` for a single value, or several keys for multi-axis readings such as `{ "x": 0.1, "y": 0.2, "z": 9.8 }` or `{ "lat": 43.8, "long": -79.3 }`.

## 7. Gateway decoders

### 7.1 How decoding works

Every gateway points to a **Firmware** in the global catalog (Manufacturer → Product → Firmware). A firmware's `Processor` field holds the C# source of a class that implements one method:

```csharp
namespace uBeac.IoT.Processing
{
    public interface IProcessor
    {
        void Process(GatewayData gatewayData);   // read Body or Bytes, fill gatewayData.RawDevices
    }
}
```

```mermaid
flowchart LR
  CAT[("Firmware catalog<br/>Firmware.Processor = C# source")] --> START["Processor worker starts"]
  START --> LOAD["ProcessorPool loads every firmware"]
  LOAD --> COMPILE["ProcessorBuilder compiles each one with Roslyn<br/>into an in-memory assembly"]
  COMPILE --> CACHE["Instance of the IProcessor type,<br/>cached by FirmwareId"]
  COMPILE -. "compile error" .-> NOOP["DefaultProcessor (no-op)"]
  MSG["Message from toProcessor"] --> PICK["pool[gatewayData.FirmwareId]"]
  CACHE --> PICK
  PICK --> RUN["Process(gatewayData)"]
  RUN --> NEXT["SetDefaultProperties, then send to toPostProcessor"]
```

Decoders compile against `System`, LINQ, regular expressions, `uBeac.Models` (which includes the shared iBeacon, Eddystone and generic-JSON helpers), Newtonsoft.Json and MsgPack.Cli. The built-in decoders catch parsing errors and add them to `gatewayData.Exceptions`, which the web app shows next to the raw request. An exception a decoder doesn't catch sends the message to the `_toProcessor` queue, and it never reaches the live view.

### 7.2 Supported gateways and formats

Reference sources for every decoder live in `Tools/ProcessDevelopment/`.

| Decoder | Gateway, app or format | Input | What it extracts |
|---|---|---|---|
| `uBeacGenericGatewaySingleSensor` | uBeac generic JSON, one sensor | `{ id, type, unit, prefix, ts, data }` | Any sensor type. Keys are matched against synonym lists (`mac`, `id`, `uid`, `name`...), timestamps in seconds or milliseconds, NMEA GPS strings |
| `uBeacGenericGatewayMultipleSensor` | uBeac generic JSON, one device | `{ id, ts, sensors: [ ... ] }` | As above, several sensors per device |
| `uBeacGenericGatewayMultipleDevice` | uBeac generic JSON, many devices | `[ { device }, ... ]` | As above, several devices per request |
| `CustomJsonGatewayProcessor` | Free-form JSON | `{ id, sensors: [ { temp, rssi, lux, ... } ] }` | Maps common key names to temperature, distance, illuminance, pressure, proximity, voltage and signal strength |
| `CustomCsvGatewayProcessor` | CSV lines | `sensorUid,type,unit,prefix,value` | Prototype |
| `MinewG1GatewayProcessor` | Minew G1 BLE gateway | JSON array of scan results | iBeacon, Eddystone, Minew sensor frames (battery, temperature, humidity, 3-axis acceleration, light), Ruuvi RAWv1 |
| `JaaleeGatewayProcessor` | Jaalee BLE gateway | `{ devices: [ { mac, data, rssi } ] }` | Same BLE parsers as Minew |
| `IngicsIGS01BleWifiProcessor` | INGICS iGS01 BLE-to-Wi-Fi | `$GPRP,<tag>,<gw>,<rssi>,<adv-hex>` lines | Walks BLE advertising structures: names, UUIDs, Eddystone, iBeacon, Minew, Ruuvi |
| `AprilBrothersV4GatewayProcessor` | April Brothers BLE gateway V4 | Binary MessagePack (`Bytes`) | Per-device MAC, RSSI and advertising data; Eddystone and iBeacon; battery, temperature, distance |
| `BlueCatEdgeRelayProcessor` | BlueCats Edge Relay | `{ edgeMAC, devices: [ ... ] }` | Signal strength, temperature, GPS location |
| `MistSystemsGatewayProcessor` | Mist Systems access points | BLE asset webhook | Signal strength, temperature |
| `AndroidRuuviStationProcessor` | Ruuvi Station Android app | `{ tags: [ ... ] }` | Humidity, pressure, temperature, voltage, acceleration, signal |
| `AndroidBeaconScannerProcessor` | Android "Beacon Scanner" app | `{ beacons: [ ... ] }` | Signal strength, distance, Eddystone telemetry |
| `AndroidDataCollectorProcessor` | Android "Data Collector" app | JSON array of sensor strings | Accelerometer, gyroscope, magnetometer, light, pressure, proximity, step counter, orientation, location |
| `IosSensorPhoneProcessor` | iOS "SensorPhone" app | `{ messages: [ ... ] }` | Acceleration, gyroscope, sound level, location |

BLE scanner decoders leave `IsValid = false`, so beacons nearby are listed as unregistered instead of being provisioned automatically. Generic JSON, phone apps and Ruuvi Station set `IsValid = true`.

Sample payloads for testing are in `Tools/ProcessorDebuger/Gateway Sample Data/`, and more are embedded as comments in `Tools/ProcessorDebuger/Program.cs`.

### 7.3 Sensor types, units and prefixes

Readings are typed with three enums in `Components/uBeac.Models`:

- **`SensorTypes`** (24): Custom, Location, Distance, Temperature, Humidity, Voltage, Acceleration, MagneticField, RotationalMotion, Proximity, Illuminance, Pressure, Counter, Orientation, Gyroscope, Sound, SignalStrength, Processor, Memory, DiskSpace, BandWidth, Revolution, Bus, plus NA.
- **`SensorUnits`** (38): from Percent, Centigrade and Meter to Decibel-milliwatts, Tesla and Hertz.
- **`SensorPrefixes`**: SI powers of ten from 10⁻²⁴ to 10²⁴.

`WebApps/uBeac.Api/wwwroot/SensorTypes.json` mirrors these for the web app, including which units are valid for each type.

## 8. Tenancy, identity and security model

### 8.1 Tenancy

- The tenant is the **Team**. Every entity carries a `TeamId`.
- Each team has a unique **namespace** (lowercase, 6 to 50 characters). It becomes the team's ingestion host: `{namespace}.hub.ubeac.io`.
- Each gateway has a **URL** that is unique within the team. It is the HTTP path and the MQTT topic root.
- Users join teams through **Access** records with the level `View` or `Admin`. The global `ADMINS` role (platform operators) passes every team check and manages the device catalog.
- Time-series and raw data live in **per-team collections**, created with their indexes when the team is created and dropped when it is deleted.
- MQTT topics are **prefixed with the team ID** inside the broker, so a client's subscriptions are confined to its own team (see the MQTT caveat in [Security notes](#17-known-limitations-and-security-notes)).

### 8.2 Authentication

```mermaid
sequenceDiagram
  autonumber
  participant B as Browser (web app)
  participant I as uBeac.Idsrv
  participant A as uBeac.Api
  participant S as uBeac.SocketApi
  participant M as MongoDB
  B->>I: POST /connect/token (password grant, scopes openid profile api idsrv socket roles)
  I->>M: find user (custom MongoDB user store), check password and lockout
  I-->>B: JWT access token (3 hours) with sub, email and role claims
  B->>A: REST request with Authorization: Bearer token
  A->>A: validate JWT (API resource uBeacApi)
  A->>M: load the user's Access records (team and level)
  A-->>B: ResultSet response
  B->>S: WebSocket /socket?access_token=token
  S->>S: validate JWT (API resource uBeacSocketApi)
  B->>S: Join("SensorData/{teamId}/*/*/*")
  S->>M: check View access to teamId
  S-->>B: onGroupData(...) as data arrives
```

Programs can call the API without a user by sending a **team API token** in an `AccessToken` header. A team admin creates tokens with the `View` or `Admin` level in the web app. The API decrypts the token, reads the team ID from it, and runs the request as a synthetic principal with that access level.

### 8.3 Gateway security

Gateways don't use OAuth. Each gateway carries its own `Security` settings, all optional and edited in the web app:

| Setting | HTTP | MQTT |
|---|---|---|
| Protocol enabled | `Http.Enabled` | `Mqtt.Enabled` |
| Encryption required | `Http.Ssl` (HTTPS only) | `Mqtt.Tls` |
| Credentials | Required custom headers and values | Username and password |
| Network | Allowed and denied IPs, ranges and CIDR blocks | Same lists |
| Identity | Namespace host plus gateway URL | MQTT client ID = gateway ID |

`HttpHub` checks every request in a fixed order. The source comments call the order "very important."

```mermaid
flowchart TD
  R["Request to {namespace}.hub.ubeac.io/{gatewayUrl}/..."] --> N{"Host has exactly 4 labels<br/>and the path is not empty?"}
  N -- no --> E1["404"]
  N -- yes --> T{"Same namespace and gateway<br/>seen in the last 950 ms?"}
  T -- yes --> E2["429 Too Many Requests"]
  T -- no --> G{"Team and gateway found<br/>in the in-memory cache?"}
  G -- no --> E3["404"]
  G -- yes --> S{"Gateway security"}
  S -- "no settings, SSL required,<br/>header mismatch or IP rejected" --> E4["401"]
  S -- "HTTP disabled" --> E5["403"]
  S -- pass --> OK["Build GatewayData, publish to toDispatcher, 200 OK"]
```

## 9. Real-time delivery

`uBeac.SocketApi` hosts one SignalR hub at `/socket`. Clients authenticate with the same JWT as the API, then `Join` or `Leave` groups. Joining checks that the user has at least `View` access to the team named in the group.

```mermaid
flowchart LR
  subgraph DataFeed["Data feed"]
    GP["GatewayProcessor"] -- "toGatewayDataWebSocket" --> SC["SocketConsumer<br/>drops messages older than 60 s"]
  end
  subgraph ChangeFeed["Change feed"]
    CS[("MongoDB change streams<br/>Team, Building, Floor, Gateway,<br/>Device, Sensor, Dashboard, Widget,<br/>DeviceSummary")] --> TR["Tracker services"]
  end
  SC --> HUB["BaseHub /socket"]
  TR --> HUB
  HUB -- "Gateway, GatewayData, DeviceRawData,<br/>DeviceData, SensorData groups" --> B["Browsers"]
  HUB -- "ChangeLog/{teamId}" --> B
```

| Group | Payload | Used by |
|---|---|---|
| `Gateway/{team}/{gateway or *}` | Heartbeat `{ Id, DateTime }` | Gateway online indicators |
| `GatewayData/{team}/{gateway or *}` | Full request: raw body, decoded devices, exceptions | Live request inspector, onboarding wizard |
| `DeviceRawData/{team}/{gateway or *}/{deviceUid or *}` | Decoded devices, including unregistered beacons | Live devices view |
| `DeviceData/{team}/{gateway or *}/{device or *}` | Readings of registered devices | Device pages |
| `SensorData/{team}/{gateway or *}/{device or *}/{sensor or *}` | Individual sensor readings (all 8 wildcard combinations) | Dashboards and widgets |
| `ChangeLog/{team}` | `{ Type, Action: Added, Updated or Deleted, Value }` | Keeps the web app's entity cache current |

## 10. Data model and storage

### 10.1 Entities

```mermaid
erDiagram
  TEAM ||--o{ ACCESS : grants
  USER ||--o{ ACCESS : holds
  USER ||--|| USERPROFILE : "same Id"
  TEAM ||--o{ BUILDING : owns
  BUILDING ||--o{ FLOOR : has
  FILE ||--o{ FLOOR : "floor plan"
  FLOOR |o--o{ GATEWAY : "placed on"
  FLOOR |o--o{ DEVICE : "placed on"
  TEAM ||--o{ GATEWAY : owns
  TEAM ||--o{ DEVICE : owns
  DEVICE ||--o{ SENSOR : has
  TEAM ||--o{ DASHBOARD : owns
  DASHBOARD ||--o{ WIDGET : contains
  MANUFACTURER ||--o{ PRODUCT : makes
  PRODUCT ||--o{ FIRMWARE : ships
  FIRMWARE ||--o{ GATEWAY : "decoder for"
  GATEWAY ||--o{ GATEWAYDATA : "raw requests"
  SENSOR ||--o{ SENSORDATA : readings
  DEVICE ||--o{ DEVICESUMMARY : "one per gateway"
```

| Entity | Key fields |
|---|---|
| All entities (`BaseEntity`) | `Id`, `TeamId`, `Name`, `CreateDate`/`UpdateDate`, `CreateBy`/`UpdateBy`, and `Attributes`, a free-form key-value bag the web app uses for icons, colors, pins and precision |
| `Team` | `Namespace`, `Description`, `Address`, embedded API `Tokens` |
| `Access` | `UserId`, `TeamId`, `Level` (View or Admin) |
| `Building` | `Address`, `Latitude`, `Longitude` |
| `Floor` | `BuildingId`, `PlanFileId` (the floor-plan image) |
| `Gateway` | `Url`, `FirmwareId`, `FloorId`, `X`/`Y` on the plan, `LastRequestDate`, `RequestCount`, embedded `Security` |
| `Device` | `Uid` (MAC address or app ID, unique per team), `FloorId`, `X`/`Y` |
| `Sensor` | `Uid` (unique per device), `DeviceId`, `Type`, `Unit`, `Prefix`, `Persist`, inferred `Schema` |
| `Dashboard`, `Widget` | Widgets have `Type`, a 12-column grid position (`X`, `Y`, `W`, `H`) and a JSON `Setting` |
| `Manufacturer`, `Product`, `Firmware` | The global device catalog. `Firmware.Processor` holds the decoder source |
| `UserProfile`, `User` | Profile data, and the identity record used by the identity server |
| `File` | Uploaded file metadata. Content is stored on disk by ID |

### 10.2 Databases and collections

| Database | Collections | Written by | Read by |
|---|---|---|---|
| `uBeac` | One per entity type (`Team`, `Gateway`, `Device`, `Sensor`, `User`...) | API, identity server, PostProcessor, GatewayProcessor | Everything |
| `SensorData` | `SensorData_{teamId}`, **one per team** | SensorProcessor | API (`Sensor/GetData`) |
| `GatewayData` | `GatewayData_{teamId}`, **one per team** | GatewayProcessor | API (`Gateway/GetData`) |
| `DeviceSummary` | `DeviceSummary` | DeviceProcessor | API, SocketApi (change stream) |
| `*Log` (one per service) | `Error`, `Warning`, `Information`, ... (one per Serilog level) | Each service's logger | Operators |

Per-team collections get their indexes when the team is created: a descending `DateTime` index with a **90-day TTL** on both, plus `DateTime + GatewayId` on gateway data and `SensorId + DateTime` and `GatewayId` on sensor data for history queries. (The web app's help text quotes 30 days of retention; the code sets 90.) Numbers are stored as `Decimal128`, and location readings as GeoJSON points. Summaries are maintained incrementally at write time; nothing uses aggregation pipelines.

## 11. REST API

### 11.1 Conventions

- **Routes** follow `/{controller}/{action}/`, for example `POST /Device/Add`.
- **Every response** is a `ResultSet<T>`: `{ "code": 200, "data": ..., "errors": [ { "code", "message" } ] }`. The HTTP status mirrors `code`.
- **Validation** uses DataAnnotations on input models and returns `400` with the errors.
- **Authorization** is enforced in the facade layer, per team and access level.
- **Swagger UI** is served at `/doc` on the API, identity server and SocketApi.

### 11.2 Request lifecycle through the layers

```mermaid
classDiagram
  direction LR
  class DeviceController {
    +Add(DeviceInputModelAdd)
    +Update(DeviceInputModelUpdate)
    +Remove(Guid id)
  }
  class DeviceFacade {
    +AddAsync(Device)
    checks team Admin access
    stamps CreateBy and UpdateBy
  }
  class DeviceService {
    +AddAsync(Device)
    rejects a duplicate Uid in the team
    stamps dates and new Id
  }
  class DeviceRepository {
    +InsertAsync(Device)
    MongoDB collection Device
  }
  class ISecurityContext {
    +HasAccess(level, teamId)
    loads the user's Access records
  }
  class Mapping {
    AutoMapper input model to entity
  }
  DeviceController --> Mapping : 1 map input
  DeviceController --> DeviceFacade : 2 call
  DeviceFacade --> ISecurityContext : 3 authorize
  DeviceFacade --> DeviceService : 4 business rules
  DeviceService --> DeviceRepository : 5 persist
```

Each layer has a single job:

| Layer | Base class | Job |
|---|---|---|
| Controller | `BaseEntityController` | Routing, model validation, mapping input models to entities, wrapping results |
| Facade | `BaseEntityFacade<T>` | **Authorization**: team access checks and audit fields |
| Service | `BaseEntityService<T>` | **Business rules**: uniqueness, cascades, blocking cross-tenant moves |
| Repository | `GenericRepository<T>` | **Data access**: one MongoDB collection per entity type |

After the write, the MongoDB change stream takes over: the PostProcessor caches the new device, and SocketApi pushes a `ChangeLog` event to the team's browsers.

### 11.3 Endpoints (`uBeac.Api`)

"View" and "Admin" are team access levels. `ADMINS` is the platform operator role.

| Controller | Actions | Access |
|---|---|---|
| **Team** | `GET GetAll`, `GET GetById/{id}` (the whole workspace in one call: buildings, floors, gateways, devices, sensors, summaries, dashboards, widgets, members), `POST Update`, `DELETE Remove/{id}` (cascades and drops the team's data collections), `POST InvokeToken`, `POST RemoveToken` | View to read, Admin to change |
| **Team** (continued) | `POST Add` (the creator becomes Admin), `GET Exists/{namespace}` | Any signed-in user |
| **Access** | `POST Add` (emails the added user), `POST Update`, `DELETE Remove/{id}` (keeps at least one Admin) | Admin |
| **Building** | `POST Add`, `POST Update`, `DELETE Remove/{id}` (removes floors, unplaces gateways and devices) | Admin |
| **Floor** | `POST Add`, `POST Update`, `DELETE Remove/{id}` | Admin |
| **Gateway** | `GET GetData` (paged raw request history), `POST Add`, `POST Update`, `DELETE Remove/{id}` (removes its data); `GET Exists/{url}` | View to read, Admin to change; `Exists` for any signed-in user |
| **Device** | `POST Add`, `POST Update`, `DELETE Remove/{id}` (removes sensors and data) | Admin |
| **Sensor** | `GET GetData` (paged time series by team, devices, sensors and date range), `POST Add`, `POST Update`, `DELETE Remove/{id}` | View to read, Admin to change |
| **Dashboard** | `POST Add`, `POST Update`, `POST UpdateWidgets` (saves the whole layout), `DELETE Remove/{id}` | Admin |
| **File** | `POST Upload` (up to 3 MB), `GET Download/{id}` | See [security notes](#17-known-limitations-and-security-notes) |
| **Manufacturer** | `GET GetAll` (public, cached 60 s: the full catalog tree, decoder source hidden), `POST Add`, `POST Update`, `DELETE Remove/{id}` | ADMINS to change |
| **Product** | `POST Add`, `POST Update`, `DELETE Remove/{id}` | ADMINS |
| **Firmware** | `POST Add`, `POST Update` (including decoder source), `DELETE Remove/{id}` | ADMINS |

Static resources: `/SensorTypes.json` (sensor catalog) and `/doc` (Swagger UI).

### 11.4 Account endpoints (`uBeac.Idsrv`)

Besides the standard OpenID Connect endpoints (`/connect/token`, `/connect/userinfo`, `/.well-known/openid-configuration`), the identity server exposes `User/{action}`:

| Action | Purpose |
|---|---|
| `Register` | Create an account (email, password, time zone) and send a confirmation email |
| `ConfirmEmail`, `ResendEmail` | Email confirmation |
| `ForgotPassword`, `ResetPassword`, `ChangePassword` | Password flows, with notification emails |
| `Claims`, `Profile`, `Update`, `GetUserProfileByEmail` | Profile read and update; lookup used when adding team members |
| `Logout` | Sign out |

Identity resources are `openid`, `profile` and `roles`. API resources are `uBeacApi` (scope `api`), `uBeacIdsrvApi` (scope `idsrv`) and `uBeacSocketApi` (scope `socket`). Clients are configured in `WebApps/uBeac.Idsrv/appsettings.json`.

## 12. Solution structure

### 12.1 Repository layout

```text
api.ubeac.io/
├── uBeac.sln
├── WebApps/                       Web services (ASP.NET Core, hosted in IIS)
│   ├── uBeac.Api/                 REST API: Controllers, Facades, Services, Repositories, InputModels
│   ├── uBeac.Idsrv/               IdentityServer4 and the account API
│   ├── uBeac.HttpHub/             HTTP ingestion
│   ├── uBeac.MqttHub/             MQTT broker and ingestion
│   └── uBeac.SocketApi/           SignalR hub for live data and entity changes
├── Applications/                  Queue workers (Windows Services, or console with --console)
│   ├── uBeac.Dispatcher/
│   ├── uBeac.Processor/
│   ├── uBeac.PostProcessor/
│   ├── uBeac.GatewayProcessor/
│   ├── uBeac.SensorProcessor/
│   └── uBeac.DeviceProcessor/
├── Components/                    Shared class libraries (see 12.2)
├── Tools/
│   ├── ProcessDevelopment/        Source of every gateway decoder
│   ├── ProcessorDebuger/          Console harness and sample payloads for testing decoders
│   ├── IndexCreator/              One-off index back-fill for per-team collections
│   ├── FileRemover/               Deletes uploaded files no entity references
│   └── SignalRClient/             Browser page that measures end-to-end latency
├── Files/
│   ├── IdentityServer/            Email templates, copied to the identity file store at deploy time
│   └── MongoDBIndexList.txt       Hand-kept index list from the earlier schema
├── Email_User_Access_Add_*.txt    "You've been added to a team" email template
└── Tasks.txt                      The web app's endpoint map from the pre-2019 API, kept as a migration checklist
```

Zipped legacy code, earlier archived projects and database backups that were once kept under `Files/` and `ArchivedProjects/` are not part of the public release.

### 12.2 Shared components

| Library | Purpose |
|---|---|
| `uBeac.Models` | Entities, the `GatewayData` envelope, sensor enums, shared BLE parsers (iBeacon, Eddystone) and generic-JSON decoding helpers |
| `uBeac.Serialization` | MessagePack + LZ4 message serializer |
| `uBeac.Messaging` | RabbitMQ `Producer` and `Consumer` base classes and DI registration |
| `uBeac.Hosted.Service` | Worker host: `BaseApp`, `ServiceStarter` (Windows Service or console) and a throughput monitor |
| `uBeac.Logging` | Serilog set-up with the MongoDB sink and enrichers |
| `uBeac.Repositories`, `uBeac.Repositories.MongoDB` | Repository contracts, the generic MongoDB repository and the change-stream `ChangeTracker` |
| `uBeac.IoT.Processing` | `IProcessor`, the Roslyn `ProcessorBuilder` and the `ProcessorPool` |
| `uBeac.HttpMqttCommon` | Logic shared by both hubs: the team and gateway cache, security checks |
| `uBeac.Security` | Per-request security context and access checks |
| `uBeac.Web` | Exception and logging middleware, CORS, authentication helpers |
| `uBeac.Web.Documentation` | Swagger set-up |
| `uBeac.WebSocket` | SignalR hub base class and the socket abstraction |
| `uBeac.Notifications` | Change notification types |
| `uBeac.Mappings` | AutoMapper wrapper |
| `uBeac.Caching` | In-memory cache, used for HTTP rate limiting |
| `uBeac.Services.Email` | SMTP email sender |
| `uBeac.Storage.File` | Named local-disk file stores |
| `uBeac.Common` | `ResultSet` envelope, errors and helpers |
| `uBeac.IoT.Models` | Earlier models, fully commented out and not in the solution |

### 12.3 Project dependencies

```mermaid
flowchart LR
  subgraph WebApps
    Api["Api"]
    Idsrv["Idsrv"]
    HttpHub["HttpHub"]
    MqttHub["MqttHub"]
    SocketApi["SocketApi"]
  end
  subgraph Workers["Applications"]
    Disp["Dispatcher"]
    Proc["Processor"]
    Post["PostProcessor"]
    GwP["GatewayProcessor"]
    SenP["SensorProcessor"]
    DevP["DeviceProcessor"]
  end
  subgraph Core["Core components"]
    Models["Models"]
    Messaging["Messaging"]
    Hosted["Hosted.Service"]
    Repos["Repositories.MongoDB"]
    IoTP["IoT.Processing"]
    HMC["HttpMqttCommon"]
    Security["Security"]
    Web["Web"]
    WS["WebSocket"]
  end
  Api --> Models & Repos & Security & Web
  Idsrv --> Repos & Web
  HttpHub --> HMC & Messaging & Web
  MqttHub --> HMC & Messaging
  SocketApi --> Messaging & Security & WS & Web
  Disp --> Hosted & Messaging
  Proc --> Hosted & Messaging & IoTP & Repos
  Post --> Hosted & Messaging & Repos
  GwP --> Hosted & Messaging & Repos
  SenP --> Hosted & Messaging & Repos
  DevP --> Hosted & Messaging & Repos
  HMC --> Models & Repos
  IoTP --> Models
  Security --> Models & Repos
  WS --> Security
  Web --> Models
```

The diagram shows the main edges only. Most projects also reference `Logging`, and the web services reference `Web.Documentation`, `Services.Email`, `Storage.File` and `Mappings` as needed. `Models` is the most widely shared library, referenced by 14 projects.

## 13. Getting started (local development)

> This code was built for Windows, IIS and .NET Core 2.2 and has not been maintained since 2020. These steps describe how it ran for its developers. Expect to retarget to a supported .NET version for anything beyond exploration.

### 13.1 Prerequisites

- **Windows** with **Visual Studio 2019** and the **.NET Core 2.2 SDK** (available from Microsoft's .NET download archive). Workers target `win10-x64`, and the API and identity server store files under `D:\Files` by default.
- **MongoDB 4.0**, running as a replica set (change streams require one). A single node is enough.
- **RabbitMQ 3.x**.
- Optional: an SMTP server for registration and password emails. A local mail catcher works.

A minimal Docker Compose file for the infrastructure (not part of the original repo):

```yaml
services:
  mongo:
    image: mongo:4.0
    command: ["--replSet", "rs0", "--bind_ip_all"]
    ports: ["27017:27017"]
  rabbitmq:
    image: rabbitmq:3-management
    ports: ["5672:5672", "15672:15672"]
```

```bash
docker compose up -d
docker compose exec mongo mongo --eval 'rs.initiate({_id:"rs0",members:[{_id:0,host:"localhost:27017"}]})'
```

### 13.2 Configure

Every service reads `appsettings.json` and then `appsettings.{ASPNETCORE_ENVIRONMENT}.json`. Secrets were removed before publication and replaced with placeholders such as `<db-password>`.

1. **Connection strings.** In each `appsettings.Development.json`, point the MongoDB strings at `mongodb://localhost:27017/<database>?replicaSet=rs0` and `RabbitMQConnection` at `amqp://guest:guest@localhost:5672/`. [Configuration reference](#15-configuration-reference) lists which service needs which string.
2. **File stores.** Create the folders named under `FileStorages` (by default `D:\Files\...`) or change the paths. Copy the email templates from `Files/IdentityServer/` into the folder named `IdentityServer`; the API and the identity server read them when they send mail.
3. **Identity server clients.** In `WebApps/uBeac.Idsrv/appsettings.json`, each client's `ClientSecrets[].Value` is the Base64 SHA-256 hash of its secret. Choose a secret and hash it:

   ```bash
   echo -n 'my-local-secret' | openssl dgst -sha256 -binary | base64
   ```

   Put the plain secret in the web app's `public/config.js` (`identityServerClientSecret`) and in the Swagger pages (`wwwroot/swagger.js`).
4. **Team token encryption.** Generate values for `AESKey` and `AESIV` in `WebApps/uBeac.Api/appsettings.json`:

   ```bash
   openssl rand -base64 32   # AESKey
   openssl rand -base64 16   # AESIV
   ```
5. **Email** (optional). Fill in `MailServerSettings` in the API and identity server settings.
6. **Local HTTP tokens.** `uBeac.SocketApi` validates tokens with `RequireHttpsMetadata = true` (`Startup.cs`). With the identity server on plain `http://localhost:60000`, set it to `false` for local work, as the API already does.

### 13.3 Run

Run each web service on Kestrel with `dotnet run`, and set the environment explicitly. **Workers default to `Production` when `ASPNETCORE_ENVIRONMENT` isn't set**, and `dotnet run` doesn't apply these projects' IIS Express launch profiles.

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
cd WebApps/uBeac.Idsrv;     dotnet run --urls http://localhost:60000
cd WebApps/uBeac.Api;       dotnet run --urls http://localhost:60004
cd WebApps/uBeac.SocketApi; dotnet run --urls http://localhost:55044
cd WebApps/uBeac.HttpHub;   dotnet run --urls http://localhost:60006
cd WebApps/uBeac.MqttHub;   dotnet run --urls http://localhost:60007   # MQTT broker on TCP 1883
cd Applications/uBeac.Dispatcher; dotnet run -- --console              # repeat for each worker
```

(Each command in its own terminal.) IIS Express also works for the API, identity server and SocketApi, but it only answers to `localhost`, so the HTTP hub has to run on Kestrel for the host-name routing in step 13.4.

| Service | Local address |
|---|---|
| `uBeac.Idsrv` | `http://localhost:60000` |
| `uBeac.Api` | `http://localhost:60004` (Swagger at `/doc`) |
| `uBeac.SocketApi` | `http://localhost:55044/socket` |
| `uBeac.HttpHub` | `http://localhost:60006` |
| `uBeac.MqttHub` | `http://localhost:60007`, MQTT on TCP 1883 |
| Workers in `Applications/` | Console; each prints its processed count every second |
| Web app ([ui.ubeac.io](https://github.com/ubeac/ui.ubeac.io)) | `http://localhost:60001` (`npm run serve`) |

```mermaid
flowchart LR
  subgraph Infra["Docker"]
    MONGO[("MongoDB rs0<br/>:27017")]
    RMQ[("RabbitMQ<br/>:5672")]
  end
  subgraph Web["Web services (dotnet run)"]
    IDS["Idsrv :60000"]
    API["Api :60004"]
    SOCK["SocketApi :55044"]
    HH["HttpHub :60006"]
    MH["MqttHub :60007 and :1883"]
  end
  subgraph Workers["Workers (--console)"]
    W["Dispatcher, Processor, PostProcessor,<br/>GatewayProcessor, SensorProcessor, DeviceProcessor"]
  end
  UI["Web app :60001"]
  CURL["curl or mosquitto_pub"]
  UI --> IDS & API & SOCK
  CURL --> HH & MH
  Web --> MONGO
  HH & MH --> RMQ
  RMQ <--> W
  W --> MONGO
  RMQ --> SOCK
```

### 13.4 Send your first reading

A new database has an empty device catalog, and the web app needs a firmware to create a gateway. Seed the catalog first, as an operator.

1. **Create an account** in the web app (`http://localhost:60001/register`). This also creates your first team, for example with namespace `myteam1`.
2. **Make yourself an operator.** Give your user the `ADMINS` role, then sign out and back in:

   ```bash
   docker compose exec mongo mongo uBeac --eval 'db.User.updateOne({ Email: "you@example.com" }, { $addToSet: { Roles: "ADMINS" } })'
   ```
3. **Seed the catalog.** In the web app's **Admin** menu, add a manufacturer, a product, and a firmware. Paste the source of `Tools/ProcessDevelopment/uBeacGenericGatewayMultipleSensor.cs` into the firmware's processor field, then restart the Processor worker so it compiles the new decoder.
4. **Add a gateway** with URL `mygateway` and that firmware. The web app fills in default security settings (HTTP and MQTT enabled).
5. **Post a reading.** The HTTP hub reads the team from the first label of a four-label host name, so set the `Host` header instead of editing DNS:

   ```bash
   curl -X POST "http://localhost:60006/mygateway/devices/device-1/sensors/temperature?type=4&unit=2&value=22.5" \
        -H "Host: myteam1.hub.ubeac.local"
   ```

   `type=4` is Temperature and `unit=2` is Centigrade. This URL form is already structured, so it skips the decoder. To exercise the decoder, post a JSON body to `http://localhost:60006/mygateway` instead; samples are in `Tools/ProcessorDebuger/Gateway Sample Data/`.
6. **Or publish over MQTT.** The client ID must be the gateway's ID, shown on the gateway's MQTT settings tab:

   ```bash
   mosquitto_pub -h localhost -p 1883 -i <gateway-id> -t "mygateway/devices/device-1/sensors/temperature" -m '22.5'
   ```
7. **Watch it arrive** on the gateway's **Live data** tab, or open `Tools/SignalRClient/index.html` to see messages and their end-to-end latency.

## 14. Developer workflows

### 14.1 Writing a gateway decoder

```mermaid
flowchart LR
  A["1. Save a real payload<br/>from the gateway"] --> B["2. Write the decoder in<br/>Tools/ProcessDevelopment"]
  B --> C["3. Run it in<br/>Tools/ProcessorDebuger"]
  C --> D{"Devices and<br/>sensors correct?"}
  D -- no --> B
  D -- yes --> E["4. POST Firmware/Add or Update<br/>with the source in Processor"]
  E --> F["5. Restart uBeac.Processor"]
  F --> G["6. Point a gateway at the firmware<br/>and check the Live data tab"]
```

A decoder is one class. The shared helpers in `uBeac.Models` do most of the work:

```csharp
using Newtonsoft.Json;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class MyGatewayProcessor : IProcessor
    {
        public void Process(GatewayData gatewayData)
        {
            if (string.IsNullOrEmpty(gatewayData.Body)) return;
            try
            {
                var msg = JsonConvert.DeserializeObject<MyPayload>(gatewayData.Body);
                foreach (var tag in msg.Tags)
                {
                    var device = new DeviceRawData { Uid = tag.Mac, DateTime = gatewayData.DateTime, IsValid = true };
                    device.Sensors.Add(new SensorRawData("temperature", gatewayData.DateTime,
                        SensorTypes.Temperature, SensorUnits.Centigrade, SensorPrefixes.One, tag.Temperature));
                    gatewayData.RawDevices.Add(device);
                }
            }
            catch (System.Exception ex)
            {
                gatewayData.Exceptions.Add(ex.Message);   // shown next to the request in the web app
            }
        }
    }
}
```

Set `IsValid = true` only when the payload identifies devices the user owns. Leave it `false` for scanners that report every beacon in range, so those devices wait to be adopted instead of being created automatically.

### 14.2 Adding an entity to the API

```mermaid
flowchart TD
  M["Entity class in Components/uBeac.Models<br/>(inherits BaseEntity, so it carries TeamId)"] --> IM["Input models in uBeac.Api/InputModels<br/>with DataAnnotations"]
  IM --> MAP["Mapping pairs in Startup.AddMappings"]
  MAP --> R["Repository: BaseEntityRepository&lt;T&gt;"]
  R --> S["Service: BaseEntityService&lt;T&gt;<br/>business rules and cascades"]
  S --> F["Facade: BaseEntityFacade&lt;T&gt;<br/>access checks"]
  F --> C["Controller: BaseEntityController<br/>Add, Update, Remove for free"]
  C --> DI["Register repository, service and facade<br/>in Startup.ConfigureServices"]
  DI --> RT["Optional: add a tracker in SocketApi<br/>so browsers get ChangeLog events"]
```

### 14.3 Adding a pipeline stage

Every worker follows the same pattern: a `Program` that calls `ServiceStarter.Run<App>()`, an `App` that wires services, and a consumer that handles one queue.

```mermaid
classDiagram
  class BaseApp {
    <<abstract>>
    +StartAsync()
    +ConfigureServices(services)*
    +Configure(serviceProvider)*
  }
  class Consumer {
    <<abstract>>
    +Init()
    +Handler(sender, args)*
  }
  class Producer {
    <<abstract>>
    +Send(data)
  }
  class App {
    +ConfigureServices(services)
    +Configure(serviceProvider)
  }
  class MyConsumer {
    +Handler(sender, args)
  }
  class MyProducer
  BaseApp <|-- App
  Consumer <|-- MyConsumer
  Producer <|-- MyProducer
  App ..> MyConsumer : AddRabbitMQClient, UseRabbitMq
  MyConsumer ..> MyProducer : forwards to the next queue
```

```csharp
public class App : BaseApp
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRabbitMQClient<MyConsumer>("MyConsumer");   // appsettings: RabbitMQ:MyConsumer
        services.AddRabbitMQClient<MyProducer>("MyProducer");
    }

    public override void Configure(IServiceProvider serviceProvider)
        => serviceProvider.UseRabbitMq<MyConsumer>();
}

public class MyConsumer : Consumer
{
    public MyConsumer(MessagingClientOptions<MyConsumer> options, ILogger<MyConsumer> logger)
        : base(options, logger) => Init();

    public override async Task Handler(object sender, DeliverEventArgs args)
    {
        var gatewayData = args.Get<GatewayData>();
        // ... do one job, then forward
    }
}
```

```json
{
  "RabbitMQ": {
    "MyConsumer": { "ConnectionStringName": "RabbitMQConnection", "QueueName": "toMyStage", "BatchSize": 200 },
    "MyProducer": { "ConnectionStringName": "RabbitMQConnection", "QueueName": "toNextStage" }
  }
}
```

`BatchSize` is the consumer's prefetch count. Add a producer for the new queue to the stage that feeds it, usually the PostProcessor.

## 15. Configuration reference

### 15.1 Environments

| | Development | Nightly | Production |
|---|---|---|---|
| Selected by | `ASPNETCORE_ENVIRONMENT=Development` | `Nightly` | `Production` (the workers' default) |
| API, identity, socket | `localhost` ports | `nightlyapi.`, `nightlyidsrv.`, `nightlysocket.ubeac.io` | `api.`, `idsrv.`, `socket.ubeac.io` |
| Web app | `localhost:60001` | `testui.ubeac.io`, `devui.ubeac.io` | `app.ubeac.io` |
| MongoDB | Single server | Single-node replica set `rs0`, main database `Nightly_uBeac` | Replica set `rs1` on `mongodb1` and `mongodb2.ubeac.io` |
| RabbitMQ | Local broker | Local broker | `rabbitmq1.ubeac.io` |

### 15.2 Connection strings by service

| Name | Database | Used by |
|---|---|---|
| `uBeacDBConnection` (`MongoDBConnection` in the hubs) | Main `uBeac` database | Api, Idsrv, hubs, Processor, PostProcessor, GatewayProcessor, SocketApi |
| `uBeacSensorDataDBConnection` | `SensorData` | Api, SensorProcessor |
| `uBeacGatewayDataDBConnection` | `GatewayData` | Api, GatewayProcessor |
| `uBeacDeviceSummaryDBConnection` | `DeviceSummary` | Api, DeviceProcessor, SocketApi |
| `LogConnection` | One log database per service | All services except the API, where the MongoDB log sink is switched off |
| `RabbitMQConnection` | Broker | Hubs, workers, SocketApi |
| `GeneralConnectionString` | Template for per-tenant databases | Configured but unused |

### 15.3 Other settings

| Setting | Where | Meaning |
|---|---|---|
| `RabbitMQ:{client}` | Hubs, workers, SocketApi | `QueueName`, `ConnectionStringName`, `BatchSize` (prefetch) |
| `AppConfig:Url`, `AppConfig:CorsOrigins` | Web services | Public URL and allowed browser origins |
| `IdsrvAuthority` | Api, SocketApi, Idsrv | Identity server URL used to validate tokens |
| `IdentityResources`, `ApiResources`, `Clients` | Idsrv | IdentityServer4 configuration, loaded in memory |
| `FileStorages` | Api, Idsrv | Named local folders: `PublicFileStorage` (uploads), `IdentityServer` (email templates), `DefinitionsStorage` (unused) |
| `MailServerSettings` | Api, Idsrv | SMTP host, port, sender and credentials |
| `AESKey`, `AESIV` | Api | Encryption for team API tokens (Base64) |
| `TrottlingAbsoluteExpiration` | HttpHub | Rate-limit window per gateway, in milliseconds (configured as 950; required) |
| `MaxPayloadLength` | MqttHub | Largest accepted MQTT message, in bytes (configured as 1 MB; if missing, every message is rejected) |
| `ProcessorPoolAssemblies` | Processor | Extra framework assemblies that decoders may reference |
| `Serilog` | All | Minimum log levels |

## 16. Deployment (as it ran)

There is no container, CI or infrastructure code in the repository. The layout below is reconstructed from project settings, configuration and host names.

```mermaid
flowchart TB
  subgraph Internet
    DEV["Gateways and devices"]
    USR["Users' browsers"]
  end
  subgraph DNS["DNS"]
    D1["*.hub.ubeac.io"]
    D2["*.mqtt.ubeac.io"]
    D3["api, idsrv, socket.ubeac.io"]
    D4["app.ubeac.io"]
  end
  subgraph IIS["Windows Server with IIS"]
    HH["HttpHub"]
    MH["MqttHub, with embedded broker"]
    API["Api"]
    IDS["Idsrv"]
    SOCK["SocketApi"]
    SPA["Web app (static files)"]
  end
  subgraph SVC["Windows Services"]
    WK["Six pipeline workers"]
  end
  subgraph Data["Data tier"]
    RMQ[("RabbitMQ<br/>rabbitmq1.ubeac.io")]
    RS[("MongoDB replica set rs1<br/>mongodb1 and mongodb2.ubeac.io")]
  end
  DEV --> D1 --> HH
  DEV --> D2 --> MH
  USR --> D4 --> SPA
  USR --> D3
  D3 --> API & IDS & SOCK
  HH & MH --> RMQ
  RMQ <--> WK
  WK --> RS
  API & IDS & SOCK & HH & MH --> RS
  RMQ --> SOCK
```

- Web services were published as IIS applications with in-process hosting (SocketApi runs out of process).
- Workers ran as Windows Services through a custom `IHostLifetime`.
- Ingestion relied on wildcard DNS: every team's namespace resolved to the HTTP hub, and the web app told users to connect MQTT clients to `{namespace}.mqtt.ubeac.io` on ports 1883 and 8883, or 80 and 443 over WebSockets. The broker in this code listens only on TCP 1883 and on WebSockets inside IIS, so MQTT over TLS on 8883 would have needed a TLS-terminating proxy that isn't in the repository.
- The web app was deployed by Azure Pipelines over FTPS (see [ui.ubeac.io](https://github.com/ubeac/ui.ubeac.io)).
- The earliest 2018 builds ran on Azure App Service before the move to self-managed servers.

## 17. Known limitations and security notes

This is a retired codebase. It is published to show how the platform was designed, not as software to deploy as-is.

### Security

- **Credentials.** All connection strings, keys and client secrets were replaced with placeholders before publication. Treat any credential that appeared in an earlier copy of this code as compromised.
- **Automatic operator role, and what it unlocks.** `User/Register` gives the platform-wide `ADMINS` role to any address on the company's own email domains, and sign-in doesn't require a confirmed email. So anyone could register such an address without owning it and become an operator. Operators can edit firmware decoders, and decoders are compiled and run with full trust inside the Processor, with no sandbox. Remove the domain rule, assign operators explicitly, require email confirmation, and sandbox decoders.
- **Signing key.** The identity server uses `AddDeveloperSigningCredential()`, which is meant for development only. Use a real certificate.
- **Team API tokens.** When no token matches, the access check returns the `View` level instead of "no access", so a revoked or made-up token for a known team ID still reads that team's data. Tokens are also encrypted with a static key and IV and generated with `System.Random`. Return "no access" by default and use a cryptographic random generator.
- **ChangeLog leaks settings.** SocketApi pushes whole Team and Gateway documents to every member with `View` access, including the team's API tokens and each gateway's security settings (MQTT password, required headers). This bypasses the stripping the REST API does for non-admins.
- **SignalR hub.** `SendToGroup`, `SendToAll` and `SendToClient` are open to any signed-in client, with no team check, so a client can inject fake readings into another team's dashboards. Restrict the hub to `Join` and `Leave`.
- **MQTT gaps.** A gateway with no security settings connects with no checks (the HTTP hub returns 401 in the same case). Changed settings apply only when a client reconnects. MQTT passwords are stored in plain text. A published topic that doesn't start with the gateway's URL skips the per-team prefix, which may let a client publish into another team's topic space.
- **File endpoints** (`File/Upload`, `File/Download`) don't require authentication.
- **Account lockout** is likely ineffective because the failed-attempt counter is written from a stale copy of the user.
- **Registration** doesn't verify the reCAPTCHA response that the web app collects.
- **Information exposure.** Error responses include stack traces; request logging records all headers, including tokens; `GetUserProfileByEmail` returns another user's full profile; `ForgotPassword` reveals whether an email is registered.
- **OAuth flow.** Browser clients use the resource-owner password grant, which current OAuth guidance deprecates. Use authorization code with PKCE.

### Reliability and correctness

- Queues are non-durable and messages non-persistent, so a broker restart loses in-flight data. Failed messages are acknowledged and parked in `_{queue}` queues that nothing reads.
- The "unacknowledged writes for speed" setting in the workers never takes effect, because the result of `WithWriteConcern` is discarded.
- A decoder that fails to compile silently falls back to a no-op, and decoder changes need a Processor restart.
- In-memory caches, auto-provisioning and SignalR groups are per instance, so scaling a stage out needs a SignalR backplane and unique indexes on `(TeamId, Uid)`.
- Main-database collections have no indexes created in code, for example on `Access.UserId`.
- Several code paths block on async calls with `.Result` or `.Wait()`.

### Technical debt

- .NET Core 2.2 has been out of support since December 2019, and IdentityServer4 is no longer maintained.
- There are no automated tests, CI pipeline or container images.
- Dead code includes `uBeac.IoT.Models`, the unused JavaScript-processor path, publish/subscribe classes nobody calls, and duplicate parsers in `Applications/uBeac.Processor/Models`.

### If you want to run it today

A reasonable modernization path: retarget to a current .NET LTS release; replace IdentityServer4 with a maintained OpenID Connect server and move browsers to authorization code with PKCE; make queues durable with dead-lettering and retries; containerize each service; sandbox decoders (or move them to a scripting runtime with limits); and add contract tests for every decoder using the sample payloads.

## 18. History

| Year | Milestone |
|---|---|
| 2017 | uBeac started. A first API on ASP.NET Web API and Entity Framework, hosted on Azure App Service; identity server prototypes; product and dashboard designs; the first admin panel ([admin.ubeac.io](https://github.com/ubeac/admin.ubeac.io)) |
| 2018 | A .NET Core rewrite on MongoDB, RabbitMQ and MessagePack (by April). The customer web app's first release (August). Core libraries audited and marked stable (October) |
| 2019 | Decoders for Minew, Ingics, Jaalee, April Brothers, BlueCats, Mist, Ruuvi Station and several phone apps. Mid-year refactor from two monoliths (user API, combined hub) into today's layout of 5 web services and 6 workers, on self-managed servers |
| 2020 | Web app 1.0 (May): new design, gateway wizard and phone onboarding |
| 2026 | Source code published under the MIT license |

Vocabulary changed along the way. Early code and the admin panel use *Organization* (now Team), *Endpoint* (now Gateway), *Gateway* and *GatewayFirmware* (now Product and Firmware), *Tag* (now Device) and *Permission* (now Access).

## 19. License

[MIT](LICENSE)
