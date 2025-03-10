# simplerpc_grpc_restapi

## Project Overview

This project implements a classroom simulation system with multiple server and client components communicating through different protocols: SimpleRPC, gRPC, and REST API. The system models a virtual classroom where teachers and doors interact with a central classroom service to manage class sessions.

## System Architecture

The system consists of the following main components:

### Core Components

1. **Classroom Logic** - The central business logic that manages:
   - Student tracking
   - Class session state
   - Teacher voting system for starting/ending classes
   - Unique ID generation

2. **Service Interfaces**
   - `IClassroomService` - Core service interface defining operations for classroom management

### Servers

1. **SimpleRPC Server** 
   - Primary server implementing the `IClassroomService` interface
   - Handles core classroom logic operations
   - Listens on port 5000

2. **REST API Server (ASP.NET Core)**
   - Provides REST endpoints for classroom operations
   - Acts as a gateway/adapter to the SimpleRPC service
   - Handles HTTP requests from Teacher and Door clients
   - Listens on port 5001

3. **gRPC Server**
   - Alternative interface for Door clients
   - Defined using protobuf schema
   - Provides more efficient communication for Door operations

### Clients

1. **Teacher Client**
   - Available in two variants:
     - SimpleRPC client (connects directly to SimpleRPC server)
     - REST API client (connects to the REST API server)
   - Manages teacher interactions with the classroom
   - Teachers can vote to start or end classes when conditions are met

2. **Door Client**
   - Available in three variants:
     - SimpleRPC client (connects directly to SimpleRPC server)
     - REST API client (connects to the REST API server)
     - gRPC client (connects to the gRPC server)
   - Simulates doors where students enter the classroom
   - Generates random student counts and sends them to the server

## Domain Model

### Core Classes

1. **ClassroomState**
   - Maintains the current state of the classroom
   - Tracks whether class is in session
   - Counts students present
   - Defines threshold for starting class
   - Evaluates if votes are sufficient to start/end class

2. **ClassroomLogic**
   - Thread-safe implementation of classroom business rules
   - Manages voting processes for starting and ending class sessions
   - Tracks teacher IDs and their votes
   - Generates unique IDs for system entities

3. **Data Transfer Objects**
   - **Teacher** - Represents a teacher entity with:
     - Unique ID
     - Name
     - Voting state (start/end)
   
   - **Door** - Represents a door entity with:
     - Unique ID
     - Description
     - Open/closed state
     - Student count

## Workflow

1. **System Initialization**
   - All servers start and begin listening on their respective ports
   - Logging is configured using NLog

2. **Class Session Lifecycle**
   - Door clients generate random students and send counts to the server
   - Once enough students are present (threshold reached), voting can begin
   - Teachers connect and receive unique IDs
   - Teachers vote to start class (requires majority approval)
   - If enough votes are received, class session begins
   - While class is in session, teachers can vote to end it
   - When sufficient votes to end are received, class session ends

3. **Communication Flow**
   - Teacher clients communicate with servers to:
     - Get unique IDs
     - Check class status
     - Submit votes
   
   - Door clients communicate with servers to:
     - Generate students
     - Check class status

## Implementation Details

### Concurrency Management
- Thread-safe operations using locks
- Atomic voting operations

### Fault Tolerance
- Clients automatically reconnect on connection failures
- Exception handling at multiple levels

### Protocols

1. **SimpleRPC**
   - Uses Hyperion serialization
   - HTTP transport
   - Direct method invocation semantics

2. **REST API**
   - Standard HTTP methods (GET, POST)
   - JSON request/response format
   - Swagger documentation available

3. **gRPC**
   - Protocol Buffers serialization
   - High-performance binary protocol
   - Streaming capabilities

## Project Structure

```
simplerpc_grpc_restapi/
│
├── Servers/
│   ├── ClassroomLogic.cs         # Core classroom business logic
│   ├── ClassroomService.cs       # SimpleRPC service implementation
│   ├── ClassroomState.cs         # Classroom state management
│   ├── Server.cs                 # SimpleRPC server initialization
│   └── Program.cs                # gRPC server initialization
│
├── Services/
│   ├── IClassroomService.cs      # Service contract interface
│   ├── Door.cs                   # Door DTO
│   └── Teacher.cs                # Teacher DTO
│
├── ClassroomServer/
│   ├── Controllers/
│   │   └── ClassroomController.cs # REST API controller
│   └── Program.cs                # REST API server initialization
│
├── TeacherClient/
│   ├── TeacherClient.cs          # SimpleRPC teacher client
│   └── TeacherServiceClient.cs   # REST API teacher client (generated)
│
├── DoorClient/
│   ├── DoorClient.cs             # SimpleRPC door client
│   └── DoorAdaptorClient.cs      # gRPC door client
│
└── Proto/
    └── classroom.proto           # gRPC service definition
```

## Configuration

- All servers are configured to listen on localhost
  - SimpleRPC server: port 5000
  - REST API server: port 5001
  - gRPC server: port 5000 (shared with SimpleRPC server)

## Logging

- NLog is used throughout the application for consistent logging
- Console output with timestamp, level, and message

## Running the Project

To run the complete system:

1. Start the SimpleRPC server first
   ```
   dotnet run --project Servers
   ```

2. Start the REST API server
   ```
   dotnet run --project ClassroomServer
   ```

3. Start one or more Teacher clients
   ```
   dotnet run --project TeacherClient
   ```

4. Start one or more Door clients
   ```
   dotnet run --project DoorClient
   ```

## Development Notes

- The system uses NLog for logging across all components
- The REST API client code is generated using NSwag from the Swagger definition
- Thread safety is implemented throughout the system using locks
- The system demonstrates interoperability between different communication protocols
