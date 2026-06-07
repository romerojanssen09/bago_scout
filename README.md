# BagoScout Web Application

This repository contains the ASP.NET Core MVC web application and Web API endpoints serving as the backend for the **BagoScout** mobile application.

## Table of Contents
- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [API Documentation for Mobile (Android)](#api-documentation-for-mobile-android)
  - [Authentication](#authentication)
  - [Dashboard](#dashboard)
- [Connecting the Android App](#connecting-the-android-app)
- [Android App Hosting and Distribution](#android-app-hosting-and-distribution)


---

## Overview

BagoScout is a job-scouting application designed for Seeker / Employer matching in Bago City. 
- **Backend / Web Portal**: Built with ASP.NET Core MVC 8.0, Entity Framework Core, SQL Server (LocalDB), and Bootstrap.
- **Mobile Client**: A companion .NET MAUI application targeting Android and iOS which interacts with this backend via REST APIs.

## Prerequisites

Ensure you have the following installed on your machine:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (comes standard with Visual Studio)
- [Entity Framework Core Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) (`dotnet-ef` CLI tool)

To install EF Core CLI tools globally:
```bash
dotnet tool install --global dotnet-ef
```

## Configuration

The configuration parameters are defined in `appsettings.json`.

1. **Database Connection**: Uses SQL Server LocalDB by default.
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BagoScoutDb;Trusted_Connection=true;MultipleActiveResultSets=true"
   }
   ```
2. **Email SMTP Settings**: Used for email verification.
   ```json
   "EmailSettings": {
     "SmtpServer": "smtp.gmail.com",
     "SmtpPort": 587,
     "SenderName": "BagoScout",
     "SenderEmail": "bagoscout@gmail.com",
     "Username": "bagoscout@gmail.com",
     "Password": "YOUR_GMAIL_APP_PASSWORD"
   }
   ```
3. **MapBox API Token**: Configured for geographical maps.
   ```json
   "MapBox": {
     "Token": "pk.eyJ1... (Token)"
   }
   ```

## Database Setup

1. Open your terminal in the inner webapp directory (`BagoScout/BagoScout/` where `BagoScout.csproj` is located).
2. Run migrations to create the database schema:
   ```bash
   dotnet ef database update
   ```
   *Note: Default lookup skills are seeded automatically upon database creation.*

## Running the Application

To run the web app locally, execute:
```bash
dotnet run
```

By default, the server is configured to listen on:
- **HTTP**: `http://localhost:5180` (or local IP)
- **HTTPS**: `https://localhost:7030` (or local IP)

---

## API Documentation for Mobile (Android)

The backend provides several controllers specifically for the mobile app, primarily located in [MobileApiController.cs](file:///c:/Users/User/source/repos/BagoScout/BagoScout/BagoScout/Controllers/MobileApiController.cs) and [AuthController.cs](file:///c:/Users/User/source/repos/BagoScout/BagoScout/BagoScout/Controllers/AuthController.cs).

### Authentication

* **Check Email Availability** (`GET /api/auth/check-email/{email}`)
* **Send Verification Code** (`POST /api/auth/send-verification-code`)
  * Request Body: `{ "Email": "user@example.com", "Name": "Full Name" }`
* **Verify Email** (`POST /api/auth/verify-email`)
  * Request Body: `{ "Email": "user@example.com", "Code": "123456" }`
* **Register Account** (`POST /api/auth/register`)
  * Request Body: `{ "FirstName": "John", "LastName": "Doe", "Email": "john@example.com", "Password": "...", "UserType": "seeker" }`
* **Mobile Login** (`POST /api/mobile/login`)
  * Request Body: `{ "Email": "user@example.com", "Password": "..." }`
  * Response: `{ "success": true, "token": "GUID-TOKEN", "userId": 1, "userType": "seeker", "name": "John Doe" }`

### Dashboard

* **Get Seeker Dashboard Data** (`GET /api/mobile/dashboard/seeker?token={token}`)

---

## Connecting the Android App

When running the **BagoScoutApp** (.NET MAUI) Android Emulator, it cannot access `localhost` directly. You must link it to the host computer's IP address.

1. Find your machine's local IP address (e.g., `192.168.1.4`) by running `ipconfig` in CMD/PowerShell.
2. In the MAUI App project, locate the file [ApiClient.cs](file:///c:/Users/User/source/repos/BagoScout/BagoScoutApp/BagoScoutApp/Services/ApiClient.cs).
3. Update the `GetBaseUrl()` return value to point to your development machine's local IP address:
   ```csharp
   static string GetBaseUrl()
   {
       return "https://<YOUR_LOCAL_IP>:7030";
   }
   ```
4. Keep the ASP.NET Core webapp running on the host machine while compiling and running the Android application.

---

## Android App Hosting and Distribution

The WebApp is configured to host the compiled Android application package (`.apk`) so clients can download it directly from the site.

1. **Build & Copy APK**: Run the build script `build-and-deploy-apk.ps1` from the repository root. This compiles the MAUI Android app and copies the output to `wwwroot/downloads/BagoScout.apk`.
2. **MIME Type Configuration**: Serving `.apk` files is handled in [Program.cs](file:///c:/Users/User/source/repos/BagoScout/BagoScout/BagoScout/Program.cs) via custom `StaticFileOptions` mapping the `.apk` extension to the MIME type `application/vnd.android.package-archive`.
3. **Download URL**: Once the backend is running, the app can be downloaded directly at:
   ```text
   http://<YOUR_SERVER_IP>:5180/downloads/BagoScout.apk
   ```

"# bago_scout" 
