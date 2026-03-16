# Lexicon LMS — Complete Project Plan & Scrum Task Board

> **Team:** 5 Developers · **Duration:** 4 Weeks (2 Sprints × 2 Weeks) · **Start:** 2026-03-16  
> **Stack:** .NET · EF Core (Code First) · Blazor Web App · Bootstrap 5 · Clean Architecture

---

## Workspace Architecture Overview

The current boilerplate follows **Clean Architecture** with these projects:

| Project | Role | Key Existing Content |
|---|---|---|
| `Domain.Models` | Entities & configurations | [ApplicationUser](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/Domain.Models/Entities/ApplicationUser.cs#5-10) (IdentityUser), [JwtSettings](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/Domain.Models/Configurations/JwtSettings.cs#4-20) |
| `Domain.Contracts` | Repository interfaces | `IRepositoryBase<T>`, `IInternalRepositoryBase<T>`, [IUnitOfWork](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/Domain.Contracts/Repositories/IUnitOfWork.cs#3-7) |
| `LMS.Services` | Business logic | [AuthService](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.Services/AuthService.cs#25-37) (JWT), [ServiceManager](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.Services/ServiceManager.cs#10-14) |
| `Service.Contracts` | Service interfaces | [IAuthService](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/Service.Contracts/IAuthService.cs#5-12), [IServiceManager](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/Service.Contracts/IServiceManager.cs#2-6) |
| `LMS.Infractructure` | Data access & EF Core | [ApplicationDbContext](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.Infractructure/Data/ApplicationDbContext.cs#8-16), `RepositoryBase<T>`, [UnitOfWork](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.Infractructure/Repositories/UnitOfWork.cs#9-13), [MapperProfile](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.Infractructure/Data/MapperProfile.cs#9-13) |
| `LMS.API` | REST API host | [Program.cs](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.API/Program.cs), extensions (CORS, Swagger, Auth, Identity), [DataSeedHostingService](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.API/Services/DataSeedHostingService.cs#17-123) |
| `LMS.Presentation` | API Controllers | [AuthController](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.Presentation/Controllers/AuthController.cs#17-21), [TokenController](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.Presentation/Controllers/TokenController.cs#9-24) |
| `LMS.Shared` | DTOs shared across layers | Auth DTOs (`TokenDto`, `UserAuthDto`, `UserRegistrationDto`) |
| `LMS.Blazor` | Frontend (Server + Client WASM) | Identity pages (Login etc.), BFF proxy, [ClientApiService](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.Blazor/LMS.Blazor.Client/Services/ClientApiService.cs#12-23), Layout/Nav |

---

## Step 1: ER Diagram

### Entity-Relationship Diagram (Mermaid)

```mermaid
erDiagram
    ApplicationUser ||--o| Course : "belongs to (Student)"
    Course ||--|{ Module : "has"
    Module ||--|{ Activity : "has"
    Course ||--o{ Document : "has"
    Module ||--o{ Document : "has"
    Activity ||--o{ Document : "has"
    ApplicationUser ||--o{ Document : "uploads"

    ApplicationUser {
        string Id PK "from IdentityUser"
        string Name "required"
        string Email "required, unique"
        string UserName "from IdentityUser"
        int CourseId FK "nullable, Students only"
        string RefreshToken "existing"
        DateTime RefreshTokenExpireTime "existing"
    }

    Course {
        int Id PK
        string Name "required"
        string Description "required"
        DateTime StartDate "required"
    }

    Module {
        int Id PK
        int CourseId FK "required"
        string Name "required"
        string Description "required"
        DateTime StartDate "required"
        DateTime EndDate "required"
    }

    Activity {
        int Id PK
        int ModuleId FK "required"
        int ActivityTypeId FK "required"
        string Name "required"
        string Description "required"
        DateTime StartDate "required"
        DateTime EndDate "required"
    }

    ActivityType {
        int Id PK
        string Name "E-Learning, Lecture, Exercise, Assignment, Other"
    }

    Document {
        int Id PK
        string Name "required"
        string Description
        DateTime UploadTimestamp "auto-set"
        string UploadedByUserId FK "required"
        string FilePath "stored file path"
        int CourseId FK "nullable"
        int ModuleId FK "nullable"
        int ActivityId FK "nullable"
    }
```

### Entity Details & Mapping to Workspace

#### [ApplicationUser](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/Domain.Models/Entities/ApplicationUser.cs#5-10) → [ApplicationUser.cs](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/Domain.Models/Entities/ApplicationUser.cs)

**Existing** — Extends `IdentityUser`. Must **add**:
- `string Name` — full name (required)
- `int? CourseId` — FK to Course (nullable, only for Students)
- Navigation: `Course? Course`

> [!IMPORTANT]
> Roles ("Teacher" / "Student") are already handled via ASP.NET Identity Roles — no need for a custom role column. The [DataSeedHostingService](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.API/Services/DataSeedHostingService.cs#17-123) already seeds both roles.

#### `Course` → **[NEW]** `Domain.Models/Entities/Course.cs`

| Attribute | Type | Notes |
|---|---|---|
| [Id](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.API/Extensions/AuthServiceExtension.cs#52-69) | `int` | PK, auto-increment |
| `Name` | `string` | Required. E.g. ".NET 2026" |
| `Description` | `string` | Required |
| `StartDate` | `DateTime` | Required |
| Navigation | `ICollection<Module>` | One-to-many |
| Navigation | `ICollection<ApplicationUser>` | Students in this course |
| Navigation | `ICollection<Document>` | Course-level documents |

#### `Module` → **[NEW]** `Domain.Models/Entities/Module.cs`

| Attribute | Type | Notes |
|---|---|---|
| [Id](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.API/Extensions/AuthServiceExtension.cs#52-69) | `int` | PK |
| `CourseId` | `int` | FK → Course (required) |
| `Name` | `string` | Required |
| `Description` | `string` | Required |
| `StartDate` | `DateTime` | Must be ≥ Course.StartDate |
| `EndDate` | `DateTime` | Must not overlap other modules in same course |
| Navigation | `Course` | Required |
| Navigation | `ICollection<Activity>` | One-to-many |
| Navigation | `ICollection<Document>` | Module-level documents |

#### `Activity` → **[NEW]** `Domain.Models/Entities/Activity.cs`

| Attribute | Type | Notes |
|---|---|---|
| [Id](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.API/Extensions/AuthServiceExtension.cs#52-69) | `int` | PK |
| `ModuleId` | `int` | FK → Module (required) |
| `ActivityTypeId` | `int` | FK → ActivityType |
| `Name` | `string` | Required |
| `Description` | `string` | Required |
| `StartDate` | `DateTime` | Must be within module date range |
| `EndDate` | `DateTime` | Must not overlap within module |
| Navigation | `Module` | Required |
| Navigation | `ActivityType` | Required |
| Navigation | `ICollection<Document>` | Activity-level documents |

#### `ActivityType` → **[NEW]** `Domain.Models/Entities/ActivityType.cs`

| Attribute | Type | Notes |
|---|---|---|
| [Id](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.API/Extensions/AuthServiceExtension.cs#52-69) | `int` | PK |
| `Name` | `string` | E-Learning, Lecture, Exercise, Assignment, Other |

> Seeded via [DataSeedHostingService](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.API/Services/DataSeedHostingService.cs#17-123). Keeps activity type data-driven.

#### `Document` → **[NEW]** `Domain.Models/Entities/Document.cs`

| Attribute | Type | Notes |
|---|---|---|
| [Id](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.API/Extensions/AuthServiceExtension.cs#52-69) | `int` | PK |
| `Name` | `string` | Required |
| `Description` | `string?` | Optional |
| `UploadTimestamp` | `DateTime` | Auto-set on creation |
| `UploadedByUserId` | `string` | FK → ApplicationUser |
| `FilePath` | `string` | Server file storage path |
| `CourseId` | `int?` | Nullable FK → Course |
| `ModuleId` | `int?` | Nullable FK → Module |
| `ActivityId` | `int?` | Nullable FK → Activity |
| Navigation | [ApplicationUser](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/Domain.Models/Entities/ApplicationUser.cs#5-10) | Uploader |
| Navigation | `Course?`, `Module?`, `Activity?` | Parent entity |

> [!NOTE]
> A Document belongs to **exactly one** parent (Course OR Module OR Activity). This is enforced at the application level — at least one FK must be non-null.

### Validation Rules (from PDF)
1. **Modules** must not overlap each other within a course and must not extend outside the course dates
2. **Activities** must not overlap within a module and must be within the module's date range
3. All students belong to **exactly one** course

---

## Step 2: 4-Week Task Breakdown

### Developers
| Code | Name |
|---|---|
| **D1** | Developer 1 — Backend Lead |
| **D2** | Developer 2 — Backend/Infra |
| **D3** | Developer 3 — Full-Stack |
| **D4** | Developer 4 — Frontend Lead |
| **D5** | Developer 5 — Frontend/UX |

---

### 🏃 Sprint 1 — Week 1 (Mar 16–20): Foundation & Database

> **Goal:** ER diagram approved → Domain models → Database → Basic auth working

| # | Task | Assigned | Effort | File/Folder | Depends On |
|---|---|---|---|---|---|
| 1.1 | **Finalize & submit ER diagram** for teacher approval | D1 | 0.5d | This document | — |
| 1.2 | **Create `Course` entity** with data annotations | D1 | 0.5d | `Domain.Models/Entities/Course.cs` | 1.1 |
| 1.3 | **Create `Module` entity** with FK to Course | D1 | 0.5d | `Domain.Models/Entities/Module.cs` | 1.1 |
| 1.4 | **Create `Activity` entity** with FK to Module | D2 | 0.5d | `Domain.Models/Entities/Activity.cs` | 1.1 |
| 1.5 | **Create `ActivityType` entity** (lookup table) | D2 | 0.5d | `Domain.Models/Entities/ActivityType.cs` | 1.1 |
| 1.6 | **Create `Document` entity** with polymorphic FKs | D2 | 0.5d | `Domain.Models/Entities/Document.cs` | 1.1 |
| 1.7 | **Extend [ApplicationUser](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/Domain.Models/Entities/ApplicationUser.cs#5-10)** — add `Name`, `CourseId` FK | D1 | 0.5d | [Domain.Models/Entities/ApplicationUser.cs](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/Domain.Models/Entities/ApplicationUser.cs) | 1.1 |
| 1.8 | **Configure EF relationships** (Fluent API in `OnModelCreating`) | D2 | 1d | [LMS.Infractructure/Data/ApplicationDbContext.cs](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.Infractructure/Data/ApplicationDbContext.cs) | 1.2–1.7 |
| 1.9 | **Add DbSets** for all new entities to [ApplicationDbContext](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.Infractructure/Data/ApplicationDbContext.cs#8-16) | D2 | 0.5d | [LMS.Infractructure/Data/ApplicationDbContext.cs](file:///c:/Users/mouni/OneDrive/Desktop/LMS-Gr1/LMS.Infractructure/Data/ApplicationDbContext.cs) | 1.8 |
| 1.10 | **Run initial EF migration** & verify database schema | D2 | 0.5d | `LMS.Infractructure/Migrations/` | 1.9 |
| 1.11 | **Create repository interfaces** (`ICourseRepository`, `IModuleRepository`, `IActivityRepository`, `IDocumentRepository`) | D3 | 1d | `Domain.Contracts/Repositories/` | 1.2–1.6 |
| 1.12 | **Implement concrete repositories** | D3 | 1d | `LMS.Infractructure/Repositories/` | 1.11, 1.9 |
| 1.13 | **Register repositories** in DI and update `IUnitOfWork` | D3 | 0.5d | `LMS.API/Extensions/ServiceExtensions.cs`, `Domain.Contracts/Repositories/IUnitOfWork.cs` | 1.12 |
| 1.14 | **Expand `DataSeedHostingService`** — seed ActivityTypes, demo Course, Modules, Activities | D3 | 1d | `LMS.API/Services/DataSeedHostingService.cs` | 1.10 |
| 1.15 | **Verify Login flow works** end-to-end with existing auth boilerplate | D4 | 1d | `LMS.Blazor/LMS.Blazor/Components/Account/Pages/Login.razor` | — |
| 1.16 | **Update `UserRegistrationDto`** — add `Name` field | D4 | 0.5d | `LMS.Shared/DTOs/AuthDtos/UserRegistrationDto.cs` | 1.7 |
| 1.17 | **Update `MapperProfile`** for new user fields | D4 | 0.5d | `LMS.Infractructure/Data/MapperProfile.cs` | 1.16 |
| 1.18 | **Setup project README** with build/run instructions & Git branching strategy (main → development → feature branches) | D5 | 0.5d | `README.md` | — |
| 1.19 | **Design UI wireframes/mockups** for Teacher and Student dashboards (layout planning) | D5 | 1.5d | — | — |
| 1.20 | **Setup Bootstrap 5 theme** — branding, color palette, common component styles | D5 | 1d | `LMS.Blazor/LMS.Blazor.Client/wwwroot/` | — |

---

### 🏃 Sprint 1 — Week 2 (Mar 23–27): Core Services & API + Frontend Scaffolding

> **Goal:** Full CRUD API for all entities → Initial Blazor pages scaffolded

| # | Task | Assigned | Effort | File/Folder | Depends On |
|---|---|---|---|---|---|
| 2.1 | **Create DTOs** for Course (`CourseDto`, `CourseCreateDto`, `CourseUpdateDto`) | D1 | 0.5d | `LMS.Shared/DTOs/` | 1.2 |
| 2.2 | **Create DTOs** for Module (`ModuleDto`, `ModuleCreateDto`, `ModuleUpdateDto`) | D1 | 0.5d | `LMS.Shared/DTOs/` | 1.3 |
| 2.3 | **Create DTOs** for Activity (`ActivityDto`, `ActivityCreateDto`, `ActivityUpdateDto`) | D1 | 0.5d | `LMS.Shared/DTOs/` | 1.4 |
| 2.4 | **Create service interfaces** (`ICourseService`, `IModuleService`, `IActivityService`) | D1 | 0.5d | `Service.Contracts/` | 2.1–2.3 |
| 2.5 | **Implement `CourseService`** — CRUD + validation (date constraints) | D1 | 1d | `LMS.Services/CourseService.cs` | 2.4, 1.12 |
| 2.6 | **Implement `ModuleService`** — CRUD + overlap validation | D2 | 1d | `LMS.Services/ModuleService.cs` | 2.4, 1.12 |
| 2.7 | **Implement `ActivityService`** — CRUD + overlap validation | D2 | 1d | `LMS.Services/ActivityService.cs` | 2.4, 1.12 |
| 2.8 | **Update `ServiceManager`** — register new services | D2 | 0.5d | `LMS.Services/ServiceManager.cs`, `Service.Contracts/IServiceManager.cs` | 2.5–2.7 |
| 2.9 | **Update `MapperProfile`** — add all DTO↔Entity mappings | D2 | 0.5d | `LMS.Infractructure/Data/MapperProfile.cs` | 2.1–2.3 |
| 2.10 | **Create `CoursesController`** — CRUD endpoints with `[Authorize]` | D3 | 1d | `LMS.Presentation/Controllers/CoursesController.cs` | 2.5 |
| 2.11 | **Create `ModulesController`** — CRUD endpoints nested under course | D3 | 1d | `LMS.Presentation/Controllers/ModulesController.cs` | 2.6 |
| 2.12 | **Create `ActivitiesController`** — CRUD endpoints nested under module | D3 | 1d | `LMS.Presentation/Controllers/ActivitiesController.cs` | 2.7 |
| 2.13 | **Extend `IApiService`** — add `PostAsync`, `PutAsync`, `DeleteAsync` methods | D4 | 0.5d | `LMS.Blazor/LMS.Blazor.Client/Services/IApiService.cs` | — |
| 2.14 | **Update `ClientApiService`** — implement all HTTP methods | D4 | 0.5d | `LMS.Blazor/LMS.Blazor.Client/Services/ClientApiService.cs` | 2.13 |
| 2.15 | **Scaffold Teacher Dashboard page** — list all courses | D4 | 1d | `LMS.Blazor/LMS.Blazor.Client/Pages/Teacher/Dashboard.razor` | 2.13 |
| 2.16 | **Scaffold Student Dashboard page** — show enrolled course & modules | D5 | 1d | `LMS.Blazor/LMS.Blazor.Client/Pages/Student/Dashboard.razor` | 2.13 |
| 2.17 | **Update `NavMenu.razor`** — role-based navigation (Teacher vs Student) | D5 | 0.5d | `LMS.Blazor/LMS.Blazor.Client/Layout/NavMenu.razor` | — |
| 2.18 | **Scaffold Course Detail page** — show modules within a course | D4 | 1d | `LMS.Blazor/LMS.Blazor.Client/Pages/Teacher/CourseDetail.razor` | 2.15 |
| 2.19 | **Scaffold Course Create/Edit form** (Bootstrap modal or page) | D5 | 1d | `LMS.Blazor/LMS.Blazor.Client/Pages/Teacher/CourseForm.razor` | 2.15 |
| 2.20 | **Create `UserService`** for teacher to manage users (list, create, edit) | D3 | 0.5d | `LMS.Services/UserService.cs`, `Service.Contracts/IUserService.cs` | 1.16 |

---

### 🏃 Sprint 2 — Week 3 (Mar 30–Apr 3): Frontend-Backend Integration & Documents

> **Goal:** Full UI for all use cases → Document upload/download → Student-specific views

| # | Task | Assigned | Effort | File/Folder | Depends On |
|---|---|---|---|---|---|
| 3.1 | **Teacher: Module Create/Edit form** with date validation | D1 | 1d | `LMS.Blazor/LMS.Blazor.Client/Pages/Teacher/ModuleForm.razor` | 2.11 |
| 3.2 | **Teacher: Activity Create/Edit form** with type dropdown & date validation | D1 | 1d | `LMS.Blazor/LMS.Blazor.Client/Pages/Teacher/ActivityForm.razor` | 2.12 |
| 3.3 | **Teacher: User management page** — list, create, assign to course | D2 | 1d | `LMS.Blazor/LMS.Blazor.Client/Pages/Teacher/UserManagement.razor` | 2.20 |
| 3.4 | **Create `DocumentService`** — upload, download, list, delete logic | D2 | 1d | `LMS.Services/DocumentService.cs`, `Service.Contracts/IDocumentService.cs` | 1.6 |
| 3.5 | **Create `DocumentsController`** — upload endpoint (multipart/form-data), download, list | D2 | 1d | `LMS.Presentation/Controllers/DocumentsController.cs` | 3.4 |
| 3.6 | **Configure file storage** — `wwwroot/uploads/` or a configurable path, add to `appsettings.json` | D3 | 0.5d | `LMS.API/appsettings.json`, `LMS.Services/DocumentService.cs` | 3.4 |
| 3.7 | **Document upload component** — reusable Blazor component with drag & drop | D3 | 1d | `LMS.Blazor/LMS.Blazor.Client/Components/DocumentUpload.razor` | 3.5 |
| 3.8 | **Document list/download component** — show documents for any entity | D3 | 1d | `LMS.Blazor/LMS.Blazor.Client/Components/DocumentList.razor` | 3.5 |
| 3.9 | **Student: Course overview page** — see course info & classmates | D4 | 1d | `LMS.Blazor/LMS.Blazor.Client/Pages/Student/CourseOverview.razor` | 2.16 |
| 3.10 | **Student: Module schedule view** — see activities per module (module schema) | D4 | 1d | `LMS.Blazor/LMS.Blazor.Client/Pages/Student/ModuleSchedule.razor` | 2.16 |
| 3.11 | **Student: Assignment submissions** — show assignments, upload submissions, see status (submitted/late/pending) | D4 | 1.5d | `LMS.Blazor/LMS.Blazor.Client/Pages/Student/Assignments.razor` | 3.7, 3.8 |
| 3.12 | **Teacher: Document management** — upload/view docs at Course, Module, Activity level | D5 | 1d | `LMS.Blazor/LMS.Blazor.Client/Pages/Teacher/DocumentManagement.razor` | 3.7, 3.8 |
| 3.13 | **Teacher: Receive submissions** — view submitted assignments per activity | D5 | 1d | `LMS.Blazor/LMS.Blazor.Client/Pages/Teacher/Submissions.razor` | 3.11 |
| 3.14 | **Update BFF proxy controller** — add proxy routes for Document endpoints | D1 | 0.5d | `LMS.Blazor/LMS.Blazor/Controllers/` | 3.5 |
| 3.15 | **Add API authorization policies** — Teacher-only endpoints vs Student-accessible | D1 | 0.5d | `LMS.Presentation/Controllers/`, `LMS.API/Extensions/` | — |

---

### 🏃 Sprint 2 — Week 4 (Apr 6–10): Testing, Bug Fixing, Polish & Demo Prep

> **Goal:** Stable, polished application → Sprint demo ready

| # | Task | Assigned | Effort | File/Folder | Depends On |
|---|---|---|---|---|---|
| 4.1 | **End-to-end testing** — Teacher flow: Login → Create Course → Add Modules → Add Activities → Upload Documents | D1 | 1d | — | All Week 3 |
| 4.2 | **End-to-end testing** — Student flow: Login → See Course → See Schedule → View/Download Documents → Submit Assignment | D2 | 1d | — | All Week 3 |
| 4.3 | **Fix bugs** from E2E testing (backend) | D1 | 1.5d | Various | 4.1, 4.2 |
| 4.4 | **Fix bugs** from E2E testing (frontend) | D4 | 1.5d | Various | 4.1, 4.2 |
| 4.5 | **Date overlap validation** — ensure module/activity overlap rules are enforced in both UI and API | D2 | 1d | `LMS.Services/ModuleService.cs`, `LMS.Services/ActivityService.cs` | 4.1 |
| 4.6 | **UI polish: Responsive layout** — test and fix on mobile/tablet viewports | D5 | 1d | `LMS.Blazor/LMS.Blazor.Client/` | — |
| 4.7 | **UI polish: Loading states & error handling** — spinners, toasts, empty states | D4 | 1d | `LMS.Blazor/LMS.Blazor.Client/` | — |
| 4.8 | **UI polish: Consistent Bootstrap styling** — "Less is more" review pass | D5 | 1d | `LMS.Blazor/LMS.Blazor.Client/` | — |
| 4.9 | **Seed realistic demo data** — courses, modules, activities, documents, users for demo | D3 | 1d | `LMS.API/Services/DataSeedHostingService.cs` | — |
| 4.10 | **Write unit tests** (optional/desirable) — service layer validation logic | D3 | 1.5d | New `LMS.Tests/` project | — |
| 4.11 | **Code cleanup** — remove demo pages (`Counter.razor`, `Weather.razor`, `DemoAuth.razor`), unused code | D3 | 0.5d | `LMS.Blazor/LMS.Blazor.Client/Pages/` | — |
| 4.12 | **Prepare Sprint Demo** — talking points, demo script, ensure clean database state | D1, D5 | 0.5d | — | All |
| 4.13 | **Final merge & deploy** — merge all feature branches → development → main | D1 | 0.5d | Git | All |

---

## Step 3: Developer Assignments Summary

### Week 1 — Foundation & Database

| Developer | Tasks | Focus Area |
|---|---|---|
| **D1** | 1.1, 1.2, 1.3, 1.7 | ER diagram approval, Course/Module/User entities |
| **D2** | 1.4, 1.5, 1.6, 1.8, 1.9, 1.10 | Activity/Document entities, EF config, migration |
| **D3** | 1.11, 1.12, 1.13, 1.14 | Repository layer, DI registration, seeding |
| **D4** | 1.15, 1.16, 1.17 | Verify login flow, update auth DTOs/mapper |
| **D5** | 1.18, 1.19, 1.20 | README, wireframes, Bootstrap theme |

> **Dependency flow:** D1 does ER (1.1) first → unblocks D1/D2 entity work → D2 configures EF → D3 builds repositories → D3 seeds data. D4/D5 work independently on auth & design.

### Week 2 — Core Services & API + Frontend Scaffolding

| Developer | Tasks | Focus Area |
|---|---|---|
| **D1** | 2.1, 2.2, 2.3, 2.4, 2.5 | DTOs, service interfaces, CourseService |
| **D2** | 2.6, 2.7, 2.8, 2.9 | ModuleService, ActivityService, mapper |
| **D3** | 2.10, 2.11, 2.12, 2.20 | API controllers, UserService |
| **D4** | 2.13, 2.14, 2.15, 2.18 | Blazor API client, Teacher dashboard & course detail |
| **D5** | 2.16, 2.17, 2.19 | Student dashboard, nav menu, course form |

> **Dependency flow:** D1/D2 build services (Mon–Wed) → D3 builds controllers (Tue–Fri, waits for services) → D4/D5 can scaffold UI pages while controllers are being built using mock data, then connect by Thu–Fri.

### Week 3 — Frontend-Backend Integration & Documents

| Developer | Tasks | Focus Area |
|---|---|---|
| **D1** | 3.1, 3.2, 3.14, 3.15 | Teacher forms, BFF proxy updates, auth policies |
| **D2** | 3.3, 3.4, 3.5 | User management, Document service & controller |
| **D3** | 3.6, 3.7, 3.8 | File storage config, document Blazor components |
| **D4** | 3.9, 3.10, 3.11 | Student pages: course overview, module schedule, assignments |
| **D5** | 3.12, 3.13 | Teacher document management, submission viewing |

> **Dependency flow:** D2 builds DocumentService/Controller (Mon–Tue) → D3 builds upload/download components (Tue–Wed) → D4/D5 integrate document components in pages (Wed–Fri).

### Week 4 — Testing, Bug Fixing, Polish & Demo

| Developer | Tasks | Focus Area |
|---|---|---|
| **D1** | 4.1, 4.3, 4.12, 4.13 | E2E testing (teacher), bug fixes, demo prep, final merge |
| **D2** | 4.2, 4.5 | E2E testing (student), date validation hardening |
| **D3** | 4.9, 4.10, 4.11 | Demo data seeding, unit tests, code cleanup |
| **D4** | 4.4, 4.7 | Frontend bug fixes, loading/error states |
| **D5** | 4.6, 4.8, 4.12 | Responsive design, Bootstrap polish, demo prep |

> **Dependency flow:** D1/D2 do E2E testing (Mon–Tue) → generate bug list → D1/D3 fix backend, D4 fixes frontend (Tue–Thu). D5 polishes UI throughout. Everyone converges on demo prep Friday.

---

## Sprint Deliverables Checklist

### Sprint 1 Demo (End of Week 2)
- [ ] ER diagram approved ✅
- [ ] Database created with all entities and relationships
- [ ] Login/Logout working for Teacher and Student roles
- [ ] API: Full CRUD for Courses, Modules, Activities via Swagger
- [ ] Blazor: Teacher dashboard showing courses
- [ ] Blazor: Student dashboard showing enrolled course
- [ ] Blazor: Course detail page with modules
- [ ] Blazor: Course create/edit form

### Sprint 2 Demo (End of Week 4)
- [ ] Teacher can manage users (create, edit, assign to course)
- [ ] Teacher can create/edit modules and activities with date validation
- [ ] Teacher can upload documents to courses/modules/activities
- [ ] Teacher can view student submissions
- [ ] Student can see course, classmates, module schedule
- [ ] Student can view/download documents
- [ ] Student can submit assignments and see submission status
- [ ] Responsive, polished UI with Bootstrap 5
- [ ] Clean demo data seeded
- [ ] All feature branches merged to main

---

## Git Branching Strategy

```
main ← development ← feature/[task-number]-[short-description]
```

| Branch | Purpose |
|---|---|
| `main` | Production-ready, only merged after sprint demo |
| `development` | Integration branch, all features merge here |
| `feature/1.2-course-entity` | Example feature branch for task 1.2 |

> [!TIP]
> PRs should target `development`. At least one team member reviews before merge. Merge `development` → `main` only after sprint demo.
