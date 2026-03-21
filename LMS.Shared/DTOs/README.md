# LMS DTOs Documentation

## Overview
This directory contains all Data Transfer Objects (DTOs) for the Lexicon LMS project. DTOs are used to transfer data between the API layer and clients, ensuring clean separation of concerns and proper validation.

## Structure

```
DTOs/
├── Course/
│   ├── CourseDto.cs           # Full course with modules and activities
│   ├── CourseListDto.cs       # Lightweight course list item
│   ├── CourseCreateDto.cs     # Create new course
│   └── CourseUpdateDto.cs     # Update existing course
│
├── Module/
│   ├── ModuleDto.cs           # Module details (in Course namespace)
│   ├── ModuleCreateDto.cs     # Create new module
│   └── ModuleUpdateDto.cs     # Update existing module
│
├── Activity/
│   ├── ActivityDto.cs         # Activity details (in Course namespace)
│   ├── ActivityCreateDto.cs   # Create new activity
│   └── ActivityUpdateDto.cs   # Update existing activity
│
├── Document/
│   ├── DocumentDto.cs         # Document metadata
│   └── DocumentCreateDto.cs   # Upload new document
│
└── User/
    ├── UserDto.cs             # User information
    ├── UserCreateDto.cs       # Create new user (Teacher/Student)
    └── UserUpdateDto.cs       # Update existing user
```

## Validation Rules

### Course
- **Name**: 3-100 characters, required
- **Description**: 10-500 characters, required
- **Dates**: StartDate < EndDate

### Module
- **Name**: 3-100 characters, required
- **Description**: 10-500 characters, required
- **Dates**: Must be within parent course dates
- **Overlap**: Cannot overlap with other modules in same course

### Activity
- **Name**: 3-100 characters, required
- **Description**: 10-500 characters, required
- **Times**: Must be within parent module dates
- **Overlap**: Cannot overlap with other activities in same module
- **DueDate**: Optional, should be <= EndTime

### Document
- **Name**: 3-200 characters, required
- **Description**: Optional, max 500 characters
- **Parent**: Exactly one of CourseId, ModuleId, or ActivityId must be provided

### User
- **FirstName**: 2-50 characters, letters only
- **LastName**: 2-50 characters, letters only
- **Email**: Valid email format, unique
- **Password**: Min 6 characters, must contain uppercase, lowercase, and number
- **Role**: "Teacher" or "Student"
- **CourseId**: Required for Students, null for Teachers

## Business Rules

### Date Constraints (from Project Requirements)
1. **Modules**:
   - Must not overlap with each other
   - Must be within course date range
   - EndDate > StartDate

2. **Activities**:
   - Must not overlap with each other
   - Must be within module date range
   - EndTime > StartTime

3. **Students**:
   - Must belong to exactly one course
   - Cannot exist without a course assignment

## Usage Examples

### Creating a Course
```csharp
var courseDto = new CourseCreateDto
{
    Name = ".NET 2026",
    Description = "Full-stack .NET development course",
    StartDate = new DateTime(2026, 03, 16),
    EndDate = new DateTime(2026, 12, 20)
};
```

### Creating a Module
```csharp
var moduleDto = new ModuleCreateDto
{
    Name = "ASP.NET Core Fundamentals",
    Description = "Introduction to ASP.NET Core MVC and Web API",
    StartDate = new DateTime(2026, 04, 01),
    EndDate = new DateTime(2026, 04, 30),
    CourseId = 1
};
```

### Creating an Activity
```csharp
var activityDto = new ActivityCreateDto
{
    Name = "REST API Assignment",
    Description = "Build a RESTful API with authentication",
    StartTime = new DateTime(2026, 04, 15, 09, 00, 00),
    EndTime = new DateTime(2026, 04, 15, 17, 00, 00),
    DueDate = new DateTime(2026, 04, 22),
    ActivityTypeId = 4, // Assignment
    ModuleId = 1
};
```

### Creating a Student
```csharp
var userDto = new UserCreateDto
{
    FirstName = "Anna",
    LastName = "Andersson",
    Email = "anna.andersson@student.lexicon.se",
    Password = "SecureP@ss123",
    Role = "Student",
    CourseId = 1
};
```

## Notes
- All validation attributes are enforced at the API level
- Business rule validation (overlaps, date constraints) is handled in the service layer
- DTOs use DataAnnotations for basic validation
- Complex validations are documented in comments within each DTO file
