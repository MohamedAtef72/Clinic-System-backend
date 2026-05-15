# 🏥 Clinic System - Backend

This is the **backend** service for the Clinic System project.  
It provides all API endpoints for managing doctors, patients, appointments, and ratings.  
Built with **ASP.NET Core Web API**, it follows **Clean Architecture** for scalability and maintainability.

---

## 🚀 Tech Stack

- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **SQL Server**
- **Identity (JWT Authentication)**
- **Cloudinary (store Images)**
- **MailKit & MimeKit (send Emails)**
- **Repository & Unit of Work Pattern**
- **Pagination, Filtering & Rate Limiting**
- **Docker**
- **AutoMapper**

---

## 🏗️ Architecture Overview

```
Clinic_System/
│
├── Application/        # DTOs, Interfaces, and Business Logic
├── Domain/             # Core Entities and Enums
├── Infrastructure/     # Repositories and Services Implementation
├── API/                # Controllers and API Configuration
```

---

## ⚙️ Setup Instructions

### 1️⃣ Clone the Repository
```bash
git clone https://github.com/MohamedAtef72/Clinic-System-backend.git
cd clinic-system-backend
```

### 2️⃣ Configure Environment
Create a `.env` file in the root directory and set your own values:

```env
# Database Connection
ConnectionStrings__DefaultConnection=<YourDatabaseConnectionString>

# ASP.NET Environment
ASPNETCORE_ENVIRONMENT=Development

# JWT Settings
JWT__Key=<YourJWTKey>
JWT__Issuer=<YourJWTIssuer>
JWT__Audience=<YourJWTAudience>
JWT__SecretKey=<YourJWTSecretKey>
JWT__AccessTokenExpirationMinutes=<AccessTokenMinutes>
JWT__RefreshTokenExpirationDays=<RefreshTokenDays>

# Cloudinary Settings
CloudinarySettings__CloudName=<YourCloudName>
CloudinarySettings__ApiKey=<YourCloudinaryApiKey>
CloudinarySettings__ApiSecret=<YourCloudinaryApiSecret>

# Admin Settings
AdminSettings__DefaultAdminPassword=<YourDefaultAdminPassword>
AdminSettings__Admins__0__UserName=<Admin1UserName>
AdminSettings__Admins__0__Email=<Admin1Email>
AdminSettings__Admins__0__PhoneNumber=<Admin1PhoneNumber>
AdminSettings__Admins__0__Country=<Admin1Country>
AdminSettings__Admins__0__Gender=<Admin1Gender>
AdminSettings__Admins__0__DateOfBirth=<Admin1DOB>
AdminSettings__Admins__0__RegisterDate=<Admin1RegisterDate>
AdminSettings__Admins__0__ImagePath=<Admin1ImageURL>

# Repeat for other admins as needed

# Mail Settings
MailSettings__Mail=<YourEmail>
MailSettings__DisplayName=<YourDisplayName>
MailSettings__Password=<YourEmailPassword>
MailSettings__Host=<SMTPHost>
MailSettings__Port=<SMTPPort>

# Frontend Base URL
Frontend__BaseUrl=<YourFrontendURL>

# CORS
AllowedCorsOrigin=<YourFrontendURL>
```

> ⚠️ Note: Make sure the **database exists** and **migration files** are applied before running the backend.

### 3️⃣ Run with Docker
```bash
docker-compose up --build
```

This will start:
- ASP.NET API container

---

## 📡 API Modules

| Module | Description |
|--------|--------------|
| **Auth** | Register, Login, Roles |
| **Doctors** | CRUD operations for doctors with Pagination, Filtering & Rate Limiting |
| **Patients** | Manage patients with Pagination, Filtering & Rate Limiting |
| **Appointments** | Create and manage appointments |
| **Ratings** | Add, view, and update ratings for doctors |
| **Admin Dashboard** | Aggregated statistics for admin panel |
| **Emails** | Send emails using MailKit & MimeKit |

---

## 🐳 Docker Setup

### Dockerfile Overview
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Clinic-System.API/Clinic-System.API.csproj", "Clinic-System.API/"]
COPY ["Clinic-System.Application/Clinic-System.Application.csproj", "Clinic-System.Application/"]
COPY ["Clinic-System.Domain/Clinic-System.Domain.csproj", "Clinic-System.Domain/"]
COPY ["Clinic-System.Infrastructure/Clinic-System.Infrastructure.csproj", "Clinic-System.Infrastructure/"]
RUN dotnet restore "Clinic-System.API/Clinic-System.API.csproj"
COPY . .
WORKDIR "/src/Clinic-System.API"
RUN dotnet publish "Clinic-System.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY ["Clinic-System.API/.env", "."]
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Clinic-System.API.dll"]
```

> ⚠️ Note: Ensure that the **database is running** and all **EF Core migrations are applied** for the API to function correctly.


---

## 👨‍💻 Developed By
- Mohamed Atef

