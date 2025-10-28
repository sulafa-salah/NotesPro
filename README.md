# 📝 NotesPro
> A clean, professional **ASP.NET Core 8 Web API** using **MongoDB** for note management — built to demonstrate best practices like validation, slug generation, optimistic concurrency, and soft deletion with TTL.

## 🚀 Overview

**NotesPro** is a lightweight note-management API designed for scalability and clarity.  
It supports creating, searching, updating, and soft-deleting notes with automatic **unique slug generation**, strong **validation**, and **collection-level JSON schema enforcement** in MongoDB.

This project is ideal as:
- A **portfolio project** to demonstrate backend and MongoDB expertise.
- A **template** for building real-world document-oriented APIs.

## 🧰 Tech Stack

| Category | Technology |
|-----------|-------------|
| Language | C# (.NET 8) |
| Framework | ASP.NET Core Web API |
| Database | MongoDB 8.0 |
| Validation | FluentValidation |
| Logging | Microsoft.Extensions.Logging |
| Architecture | Repository Pattern + Services |
| Containerization | Docker + Docker Compose |
| Tools | mongo-express (Mongo GUI) |


## 🧪 Features

- ✅ **CRUD Operations**
- ✅ **Search by text & tags**
- ✅ **Soft delete** + automatic **purge via TTL index**
- ✅ **Slug Service** – unique, SEO-friendly slugs with retry logic
- ✅ **MongoDB JSON Schema validator**
- ✅ **Optimistic concurrency** using `Version`
- ✅ **FluentValidation**
- ✅ **Dockerized MongoDB** + mongo-express GUI

## ⚙️ Local Setup

### Configure App

`appsettings.Development.json`:
```json
{
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "notespro"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Run 

```bash
docker compose up -d
```

Swagger UI → https://localhost:5000/swagger  
Mongo GUI → http://localhost:8081  

## 🧱 MongoDB Setup

When the app starts, `NoteCollectionInitializer`:
- Creates the `notes` collection (if not exists)
- Applies a **strict JSON schema**
- Builds these **indexes**:
  - `UX_slug` → unique index on `Slug`
  - `IX_tags_createdAt` → compound (Tags + CreatedAtUtc)
  - `TTL_purgeAt` → TTL index on `PurgeAtUtc`
  - `TXT_title_content` → text index for search

## 📘 Endpoints Summary

| Method | Endpoint | Description |
|--------|-----------|-------------|
| **POST** | `/api/notes` | Create a new note |
| **GET** | `/api/notes/{id}` | Get note by ID |
| **GET** | `/api/notes?q=word&tags=dev&page=1&pageSize=10` | Search notes |
| **PUT** | `/api/notes/{id}` | Update note (version checked) |
| **DELETE** | `/api/notes/{id}?purgeDays=7` | Soft delete note |
| **POST** | `/api/notes/{id}/restore` | Restore deleted note |
| **GET** | `/api/notes/deleted` | List deleted notes |



## 📈 Future Enhancements

- User authentication (JWT)
- Role-based access control
- Caching (Redis)
- Event-driven updates with RabbitMQ
- Frontend dashboard (Angular)
- Dockerized CI/CD via GitHub Actions


