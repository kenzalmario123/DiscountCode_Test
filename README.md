A TCP-based server application for generating and managing unique discount codes with persistent storage.

Features
✅ TCP socket communication (no REST API)

✅ Persistent storage using SQLite

✅ Unique 7-8 character code generation

✅ One-time use codes

✅ Thread-safe concurrent processing

✅ Maximum 2000 codes per request

✅ Docker container support (needed to work on network configuration since it is a container based)

✅ Comprehensive client tester

Architecture
Protocol: Custom binary TCP protocol

Storage: In memory database (for testing purposes only)

Framework: .NET 9.0 Worker Service

Communication: TCP sockets on port 8080