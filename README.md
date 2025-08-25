# Discount Code Service

A TCP-based server application for generating and managing unique discount codes with in-memory storage for testing purposes.

## 🚀 Features

- **TCP Socket Communication** - Custom binary protocol (no REST API)
- **In-Memory Storage** - Fast and efficient for testing (no persistence)
- **Unique Code Generation** - 7-8 character alphanumeric codes
- **One-Time Use** - Codes can only be used once
- **Thread-Safe Processing** - Handles concurrent requests safely
- **Bulk Generation** - Up to 2000 codes per request
- **Docker Ready** - Container support with proper networking
- **Comprehensive Testing** - Built-in client tester

## 🏗️ Architecture

| Component | Technology |
|-----------|------------|
| **Protocol** | Custom binary TCP protocol |
| **Storage** | In-memory database (testing only) |
| **Framework** | .NET 9.0 Worker Service |
| **Communication** | TCP sockets on port 8080 |
| **Container** | Docker with bridge networking |

## 📦 Project Structure

DiscountApp/
├── src/
│ └── DiscountApp.Service/ # dotnet worker
│   ├── Services/ # Business logic services
│   ├── Interfaces/
│   ├── Interfaces/ # domain helpers
│   ├── Models/ # Data models
│   ├── DependencyInjection.cs # Dependency Injection registration
│   ├── Program.cs # Application entry point
│   ├── Worker.cs # Background service handler
│   ├── appsettings.json # Configuration
│   └── DiscountApp.Service.csproj
│ └── DiscountApp.Domain/ # Application Domain
│   └── Entities/ # Domain entities
│   └── DiscountApp.Persistence.csproj
│ └── DiscountApp.Persistence/ # Persistence layer
│   └── Configurations/ # Entity configurations
│   └── Interfaces/ # Persistence interfaces
│   └── DependencyInjection.cs # DI for persistence
│   └── DiscountAppDbContext # EF Core DbContext    
│   └── DiscountApp.Persistence.csproj
└── tester/
    └── DiscountApp.Tester/ # Client tester
    ├── Program.cs # Tester entry point
    └── DiscountApp.Tester.csproj

## 🛠️ Installation & Setup

### Prerequisites

- .NET 9.0 SDK
- Docker (optional, for containerization)

### Clone and Build

```bash
# Clone the repository
git clone <repository-url>
cd DiscountApp

# Build both projects
dotnet build
```

## 🚀 Running the Application

### Server Side

# Navigate to server directory

```bash
cd src/DiscountApp.Service
`# Run the server (default port: 8080)
dotnet run



# Run with specific port

dotnet run -- --urls "http://*:8080"

# Expected output:

# TCP Server started on port 8080

# DiscountCodeService initialized

```


## 🧪 Using the Client Tester

### Interactive Mode

```bash
# Available commands:

> generate <count> <length>   # Generate discount codes
> use <code>                  # Use a discount code
> exit                        # Exit the tester

# Examples:

> generate 5 8               # Generate 5 codes of length 8
> use ABCDEFGH               # Use a specific code
> exit                       # Quit the tester
```

### Command-Line Mode

```bash
# Generate codes directly
dotnet run -- generate 10 7

# Use a specific code
dotnet run -- use TESTCODE123

# Connect to custom server and generate codes
dotnet run -- 192.168.1.100 8080 generate 5 8
```

