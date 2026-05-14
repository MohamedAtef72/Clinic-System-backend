# Clinic System — Backend

## Project Overview

A clinic management REST API built with **.NET 8 / ASP.NET Core Web API**, following **Clean Architecture** across 4 projects. Manages doctors, patients, receptionists, appointments, visits, ratings, and real-time notifications.

---

## Solution Structure

```
Clinic-System/
├── Clinic-System.Domain/          # Entities, enums, constants — zero external dependencies
├── Clinic-System.Application/     # DTOs, interfaces (contracts) — depends only on Domain
├── Clinic-System.Infrastructure/  # EF Core, repositories, services, SignalR hub
└── Clinic-System.API/             # Controllers, middleware, DI wiring, Program.cs
```

**Dependency rule:** `API → Infrastructure → Application → Domain`  
Never reference Infrastructure directly from Domain or Application.

---

## Tech Stack

| Concern | Technology |
|---|---|
| Framework | .NET 8, ASP.NET Core Web API |
| ORM | Entity Framework Core 8 (SQL Server) |
| Identity | ASP.NET Core Identity |
| Auth | JWT Bearer — HttpOnly cookies |
| Caching | Redis via StackExchange.Redis |
| Background Jobs | Hangfire (SQL Server storage) |
| Real-time | SignalR (`ClinicHub` at `/clinicHub`) |
| Object Mapping | AutoMapper 15 (`MappingProfile`) |
| Image Storage | Cloudinary (signed upload) |
| Email | MailKit + MimeKit |
| API Docs | Swagger / Swashbuckle |
| Containerization | Docker + docker-compose |
| Config | DotNetEnv (`.env` file) |

---

## Domain Models

All models live in `Clinic-System.Domain/Models/`.

| Model | Key Fields |
|---|---|
| `ApplicationUser` | Extends `IdentityUser`; `ImagePath`, `Country`, `Gender`, `DateOfBirth`, `RegisterDate`, `IsDeleted`, `DeletedAt`, `RefreshTokens` |
| `Doctor` | `Id (Guid)`, `UserId`, `SpecialityId`, `ConsultationPrice` → nav: `Availabilities`, `Appointments` |
| `Patient` | `Id (Guid)`, `UserId`, `BloodType`, `MedicalHistory`, `CreatedAt` → nav: `Appointments` |
| `Receptionist` | `Id (Guid)`, `UserId`, `ShiftStart (TimeOnly)`, `ShiftEnd (TimeOnly)` |
| `Appointment` | `Id`, `AvailabilityId`, `PatientId`, `Date`, `AppointmentStatus`, `IsDeleted`, `DeletedAt` |
| `Visit` | `Id`, `AppointmentId`, `Price`, `DoctorNotes`, `Medicine`, `VisitStatus` |
| `DoctorAvailability` | `Id`, `DoctorId`, `StartTime`, `EndTime`, `RecurrencePattern`, `RecurrenceEndDate`, `SeriesId (Guid?)`, `IsBooked`, `IsActive` |
| `Rating` | `Id`, `AppointmentId`, `DoctorId`, `PatientId`, `Rate (int)`, `Comment?` |
| `Notification` | `Id`, `Title`, `Message`, `IsGlobal`, `CreatedAt` → nav: `UserNotifications` |
| `UserNotification` | `Id`, `NotificationId`, `UserId`, `IsRead` |
| `Speciality` | `Id`, `Name` (unique index) |
| `RefreshToken` | `UserId`, `Token` (SHA-256 hashed), `ExpiryDate`, `IsRevoked`, `CreatedDate`, `CreatedByIp` |
| `BaseEntity` | Abstract base: `IsDeleted (bool)`, `DeletedAt (DateTime?)` |

**Enums** (`Clinic-System.Domain/Enums/`):
- `RecurrencePattern`: `None`, `Weekly`, `BiWeekly`

**Constants** (`Clinic-System.Domain/Constant/`):
- `Role.Admin`, `Role.Doctor`, `Role.Patient`, `Role.Receptionist`

---

## Roles

Four roles seeded automatically on startup via `RoleSeederService`:
- `Admin` — full access, sees soft-deleted records, bypasses cache
- `Doctor` — manages own availability, visits
- `Patient` — books appointments, submits ratings
- `Receptionist` — manages appointments and visits

---

## API Endpoints

All controllers are under `api/[controller]`. All require `[Authorize]` unless noted.

### AuthController — `api/Auth`
| Method | Route | Auth | Notes |
|---|---|---|---|
| POST | `DoctorRegister` | Admin only | Creates user + doctor, sends credentials email via Hangfire |
| POST | `PatientRegister` | Public | Rate limited (AuthPolicy) |
| POST | `ReceptionRegister` | Public | Rate limited (AuthPolicy), validates ShiftStart < ShiftEnd |
| POST | `Login` | Public | Rate limited (AuthPolicy), sets `t` + `rt` HttpOnly cookies |
| POST | `Refresh` | Public | Rate limited (AuthPolicy), rotates refresh token |
| GET | `Me` | Authenticated | Returns userId, role-specific id, email |
| POST | `ForgotPassword` | Public | Sends reset link to email |
| POST | `ResetPassword` | Public | Validates token, resets password |
| GET | `GetUploadSignature` | Authenticated | Returns Cloudinary signed upload params |
| GET | `Logout` | Public | Clears `t` + `rt` cookies |

### DoctorController — `api/Doctor`
| Method | Route | Notes |
|---|---|---|
| GET | `AllDoctors` | Paginated, filterable by `searchName`, `gender`, `speciality`. Admin sees deleted; others use Redis cache |
| GET | `{id}` | Redis cached per doctor |
| PATCH | `SetPrice/{id}` | Updates `ConsultationPrice`, bumps cache |

### PatientController — `api/Patient`
| Method | Route | Notes |
|---|---|---|
| GET | `GetAll` | Paginated, filterable by `searchName`, `gender`. Admin sees deleted; others use Redis cache |
| GET | `{id}` | Redis cached per patient |

### AppointmentController — `api/Appointment`
| Method | Route | Notes |
|---|---|---|
| GET | `GetAll` | Paginated, filterable by `status` |
| GET | `{id}` | |
| POST | `Create` | Rate limited (WritePolicy) |
| PUT | `Update/{id}` | Rate limited (WritePolicy) |
| DELETE | `{id}` | Rate limited (WritePolicy) |
| GET | `doctor/{doctorId}` | Filterable by `status`, `startDate`, `endDate` |
| GET | `patient/{patientId}` | Filterable by `status` |

### VisitController — `api/Visit`
| Method | Route |
|---|---|
| GET | `GetAll` |
| GET | `{id}` |
| GET | `doctor/{doctorId}` |
| GET | `patient/{patientId}` |
| POST | `Create` |
| PUT | `Update/{id}` |
| DELETE | `Delete/{id}` |

### DoctorAvailabilityController — `api/DoctorAvailability`
| Method | Route | Notes |
|---|---|---|
| GET | `GetAll` | Redis cached |
| GET | `doctor/{doctorId}` | Redis cached per doctor |
| GET | `{id}` | Redis cached |
| POST | `Add` | Validates StartTime < EndTime, supports recurrence |
| PUT | `Update/{id}` | Validates hours 6AM–11PM |
| DELETE | `Delete/{id}` | |

### RatingController — `api/Rating`
| Method | Route |
|---|---|
| GET | `{appointmentId}` |
| GET | `doctor/{doctorId}` — returns average + total count |
| POST | `Create` |
| PUT | `Update/{id}` |

### NotificationController — `api/Notification`
| Method | Route | Notes |
|---|---|---|
| GET | `User` | Paginated (pageSize=6), reads userId from JWT |
| POST | `MarkAllAsRead` | |
| POST | `MarkAsRead/{notificationId}` | |

### AdminController — `api/Admin` — Admin role only
| Method | Route |
|---|---|
| GET | `Dashboard` |
| GET | `RecentData` |

### SpecialityController — `api/Speciality`
| Method | Route | Auth |
|---|---|---|
| GET | `AllSpecialities` | Anonymous |
| GET | `{id}` | Authenticated |
| POST | `/` | Admin only |
| PUT | `{id}` | Admin only |
| DELETE | `{id}` | Admin only |

### UserController — `api/User`
| Method | Route | Auth | Notes |
|---|---|---|---|
| GET | `UserProfile` | Authenticated | Role-specific DTO, Redis cached |
| PUT | `UpdateProfile` | Authenticated | Updates common fields + role-specific fields |
| DELETE | `DeleteProfile/{id}` | Admin only | Soft delete, bumps cache |
| GET | `AllUsers` | Admin only | Paginated, Redis cached |

---

## Key Patterns & Conventions

### Repository + Service Pattern
Every entity has a matching `IXxxRepository` + `IXxxService` interface in `Application/Interfaces/`, implemented in `Infrastructure/Repositories/` and `Infrastructure/Services/`. Always inject interfaces — never concrete types.

### DTO Layer
All API input/output uses DTOs from `Application/DTO/`. Never expose raw entities. AutoMapper (`MappingProfile`) handles entity ↔ DTO mapping.

### Soft Delete
- `BaseEntity` provides `IsDeleted` + `DeletedAt` for most entities
- `ApplicationUser` and `Appointment` have inline soft-delete fields
- EF Core global query filters in `AppDbContext.OnModelCreating` (e.g., `!u.IsDeleted`, `a.IsActive && !a.Doctor.User.IsDeleted`)
- `SoftDeleteExtensions` helpers on `DbSet<T>`: `SoftDeleteAsync`, `SoftDeleteRangeAsync`, `RestoreAsync`, `HardDeleteAsync`, `IncludingSoftDeleted`, `OnlySoftDeleted`
- Deleted doctors/patients can be restored via `EnsureDoctorExistsOrRestoreAsync` / `EnsurePatientExistsOrRestoreAsync` — preserves historical medical data

### JWT Authentication (Cookie-based)
- Access token → `t` cookie (HttpOnly, Secure, SameSite=None)
- Refresh token → `rt` cookie (HttpOnly, Secure, SameSite=None)
- Refresh tokens are **SHA-256 hashed** before DB storage
- Token rotation: old tokens revoked on each refresh cycle
- Soft-deleted users are blocked at `GenerateTokenAsync`
- JWT read from cookie in `OnMessageReceived` event (not Authorization header)

### Redis Caching (Versioned)
- Cache keys follow pattern: `prefix:version_guid:params`
- Version GUID stored at `prefix:version` key in Redis
- On any mutation, call `_cache.BumpVersionAsync("prefix")` — stale entries expire naturally, no explicit deletion
- Admins always bypass cache to see soft-deleted records
- Cache TTLs: lists = 5–10 min, detail = 10 min, user profile = 10 min

### Rate Limiting
Three named policies applied via `[EnableRateLimiting("PolicyName")]`:
- `AuthPolicy` — sliding window, 10 req/min per IP (login, register, refresh)
- `ReadPolicy` — sliding window, 200 req/10s, queue 20 (GET endpoints)
- `WritePolicy` — token bucket, 50 tokens, 10/sec replenishment (POST/PUT/DELETE)
- Global token bucket: 100 tokens, 20/sec (safety net on all requests)

### Doctor Availability & Scheduling
- Recurrence: `None`, `Weekly`, `BiWeekly` (stored as string in DB)
- Series grouped by `SeriesId (Guid)` for bulk management
- `IsBooked = true` prevents double-booking
- `IsActive = false` soft-deactivates a slot
- Global query filter: `IsActive && !Doctor.User.IsDeleted`

### Background Jobs (Hangfire)
- Fire-and-forget via `BackgroundJob.Enqueue()`
- Used for: welcome/credential emails after Doctor and Receptionist registration
- Dashboard at `/hangfire`
- Storage: SQL Server (same connection string as app DB)

### Real-time Notifications (SignalR)
- Hub: `ClinicHub` at `/clinicHub`
- `INotificationService` — sends to specific user by `userId` or broadcasts globally
- `INotificationQueryService` — reads/marks notifications (used by controller)
- Notifications persisted in DB with per-user read state via `UserNotification` join table

### Image Uploads (Cloudinary)
- Frontend uploads **directly** to Cloudinary using a signed signature from `GET api/Auth/GetUploadSignature`
- Never proxy image bytes through the API
- Config: `CloudinarySettings:CloudName`, `ApiKey`, `ApiSecret`

### Email (MailKit)
- `IMailingServices` / `MailingService` wraps MailKit SMTP
- Used for: welcome emails with credentials (Doctor, Receptionist), password reset links
- Config: `MailSettings:Mail`, `Password`, `Host`, `Port`, `DisplayName`

### Global Exception Middleware
`GlobalExceptionHandlingMiddleware` catches all unhandled exceptions, logs them, and returns a consistent JSON response with `message`, `traceId`, and `timestamp`. Never leaks stack traces to clients.

---

## Infrastructure Layer Details

### Repositories (`Infrastructure/Repositories/`)
`AdminRepository`, `AppointmentRepository`, `DoctorAvailabilityRepository`, `DoctorRepository`, `NotificationRepository`, `PatientRepository`, `RatingRepository`, `ReceptionistRepository`, `SpecialityRepository`, `UserRepository`, `VisitRepository`

### Services (`Infrastructure/Services/`)
`AdminService`, `AppointmentService`, `AuthService`, `DoctorAvailabilityService`, `DoctorService`, `MailingService`, `MappingProfile`, `NotificationQueryService`, `NotificationService`, `PatientService`, `PhotoService`, `RatingService`, `ReceptionistService`, `RedisCacheService`, `RegisterService`, `RoleSeederService`, `SpecialityService`, `UserService`, `VisitService`

### Data (`Infrastructure/Data/`)
`AppDbContext` — extends `IdentityDbContext<ApplicationUser>`. All relationships use `DeleteBehavior.Restrict` to prevent cascade deletes.

### Hubs (`Infrastructure/Hubs/`)
`ClinicHub` — SignalR hub for real-time push notifications.

### Extensions (`Infrastructure/Extensions/`)
`SoftDeleteExtensions` — extension methods on `DbSet<T where T : BaseEntity>`.

---

## Application Layer Details

### DTOs (`Application/DTO/`)
`AppointmentDTO`, `AuthResultDTO`, `CloudinarySignatureDto`, `CreateAppointmentDTO`, `DashboardDTO`, `DoctorAvailabilityCreateDTO`, `DoctorAvailabilityDTO`, `DoctorInfoDTO`, `DoctorPriceDTO`, `DoctorRegisterDTO`, `DoctorsListDto`, `MailRequestDTO`, `NotificationDto`, `PatientInfoDTO`, `PatientRegisterDTO`, `PatientsListDTO`, `RatingCreateDTO`, `RatingReadDTO`, `RatingUpdateDTO`, `RecentActivityDataDTO`, `ReceptionistInfoDTO`, `ReceptionistRegisterDTO`, `ResetPasswordRequest`, `SpecialityInfo`, `UpdateAppointmentDTO`, `UserEditProfile`, `UserInfo`, `UserLogin`, `UserRegisterBase`, `UserWithDetails`, `VisitCreateDTO`, `VisitReadDTO`

---

## Environment Configuration

All secrets loaded from `.env` at startup via DotNetEnv. The app checks three candidate paths: `AppContext.BaseDirectory`, `Directory.GetCurrentDirectory()`, `AppDomain.CurrentDomain.BaseDirectory`.

```env
# Database
ConnectionStrings__DefaultConnection=

# JWT
JWT__SecretKey=
JWT__Issuer=
JWT__Audience=
JWT__AccessTokenExpirationMinutes=
JWT__RefreshTokenExpirationDays=

# Redis
Redis__BaseUrl=

# Cloudinary
CloudinarySettings__CloudName=
CloudinarySettings__ApiKey=
CloudinarySettings__ApiSecret=

# Email
MailSettings__Mail=
MailSettings__DisplayName=
MailSettings__Password=
MailSettings__Host=
MailSettings__Port=

# Admin seeding
AdminSettings__DefaultAdminPassword=
AdminSettings__Admins__0__UserName=
AdminSettings__Admins__0__Email=
AdminSettings__Admins__0__PhoneNumber=
AdminSettings__Admins__0__Country=
AdminSettings__Admins__0__Gender=
AdminSettings__Admins__0__DateOfBirth=
AdminSettings__Admins__0__RegisterDate=
AdminSettings__Admins__0__ImagePath=

# CORS & Frontend
AllowedCorsOrigin=
Frontend__BaseUrl=

# Deployment
PORT=
ASPNETCORE_ENVIRONMENT=
```

---

## Startup Behavior (Program.cs)

On every startup the app automatically:
1. Loads `.env` file
2. Binds dynamic `PORT` env var for Railway
3. Applies pending EF Core migrations
4. Seeds Identity roles (`Admin`, `Doctor`, `Patient`, `Receptionist`)
5. Seeds admin user accounts from `AdminSettings` config

---

## CORS

Allowed origins (configured in `Program.cs`):
- Value of `AllowedCorsOrigin` env var (production frontend)
- `http://localhost:3000`, `http://localhost:5173`, `http://localhost:4200`, `http://localhost:8080`

Policy name: `AllowReactApp` — allows any header, any method, with credentials.

---

## Build & Run

```bash
# Run locally
cd Clinic-System.API
dotnet run

# Build
dotnet build

# Run with Docker
docker-compose up --build

# Apply migrations manually
dotnet ef database update \
  --project Clinic-System.Infrastructure \
  --startup-project Clinic-System.API

# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project Clinic-System.Infrastructure \
  --startup-project Clinic-System.API
```

---

## Deployment Notes

- Targets **Railway** — reads `PORT` env var, configures `ForwardedHeaders` (XForwardedFor + XForwardedProto)
- Docker image: `mcr.microsoft.com/dotnet/aspnet:8.0`, exposes port `8080`
- `.env` file is copied into the Docker image at build time
- Swagger UI is always enabled (including production)
- Hangfire dashboard exposed at `/hangfire` (no auth guard — consider restricting in production)
