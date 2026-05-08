# Eagle Bank API - Practise REST API in .NET

A REST API for Eagle Bank built with .NET 10, implementing user management, bank account operations, and transaction handling with JWT authentication.

## Project Structure

This project follows **Clean Architecture** principles with clear separation of concerns across three layers:

**API → Core ← DAL** (Dependency flow: API and DAL depend on Core, but Core is independent)

```
openapi.yaml                # OpenAPI 3.1 Specification (includes auth endpoint)

EagleBankAPI/               # Presentation Layer (API)
├── Controllers/            # API Controllers (thin orchestration)
│   ├── AuthController.cs   # Authentication endpoints
│   ├── UsersController.cs  # User management endpoints
│   ├── AccountsController.cs   # Bank account endpoints
│   └── TransactionsController.cs   # Transaction endpoints
├── Models/                 # DTOs (Data Transfer Objects)
│   ├── Requests/           # Inbound API contracts
│   │   ├── CreateUserRequest.cs
│   │   ├── UpdateUserRequest.cs
│   │   ├── CreateAccountRequest.cs
│   │   ├── UpdateAccountRequest.cs
│   │   ├── CreateTransactionRequest.cs
│   │   └── LoginRequest.cs
│   ├── Responses/          # Outbound API contracts
│   │   ├── UserResponse.cs
│   │   ├── AuthResponse.cs
│   │   ├── AccountResponse.cs
│   │   ├── AccountsListResponse.cs
│   │   ├── TransactionResponse.cs
│   │   └── TransactionsListResponse.cs
│   ├── Address.cs          # Shared models
│   ├── ErrorResponse.cs
│   ├── ValidationError.cs
│   └── BadRequestErrorResponse.cs
├── Middleware/             # HTTP middleware components
│   └── GlobalExceptionHandlerMiddleware.cs   # Centralized exception handling
├── Program.cs              # Application configuration & DI container
└── appsettings.json        # Configuration settings

EagleBankAPI.Core/          # Business Logic Layer (Domain + Application)
├── Services/               # Application services (business logic)
│   ├── Interfaces/         # Service contracts
│   │   ├── IUserService.cs
│   │   ├── IBankAccountService.cs
│   │   ├── ITransactionService.cs
│   │   └── IAuthService.cs
│   ├── UserService.cs
│   ├── BankAccountService.cs
│   ├── TransactionService.cs
│   └── AuthService.cs
├── Exceptions/             # Custom domain exceptions
│   ├── NotFoundException.cs
│   ├── ForbiddenException.cs
│   ├── InsufficientFundsException.cs
│   ├── DuplicateEmailException.cs
│   ├── InvalidTransactionException.cs
│   └── UserHasAccountsException.cs
├── Entities/               # Domain entities
│   ├── User.cs
│   ├── BankAccount.cs
│   ├── Transaction.cs
│   └── Enums/
│       ├── Currency.cs
│       └── TransactionType.cs
└── Repositories/           # Repository interfaces (contracts)
    ├── IRepository.cs
    ├── IUnitOfWork.cs
    ├── IUserRepository.cs
    ├── IBankAccountRepository.cs
    └── ITransactionRepository.cs

EagleBankAPI.DAL/           # Data Access Layer (Infrastructure)
├── Data/                   # Database context
│   └── EagleBankDbContext.cs
└── Repositories/           # Repository implementations
    ├── Repository.cs       # Generic repository base
    ├── UnitOfWork.cs       # Unit of Work implementation
    ├── UserRepository.cs
    ├── BankAccountRepository.cs
    └── TransactionRepository.cs

EagleBankAPI.Tests/         # Unit Tests
└── Services/               # Service layer tests
    ├── UserServiceTests.cs
    ├── BankAccountServiceTests.cs
    ├── TransactionServiceTests.cs
    └── AuthServiceTests.cs
```

## Architecture

This project follows **Clean Architecture** principles with clear separation of concerns across three projects:

### **Core Layer (EagleBankAPI.Core)** - Business Logic + Domain
- **Services**: Application services containing business logic (e.g., `UserService`, `AuthService`)
- **Service Interfaces**: Contracts defining service operations (`IUserService`, `IAuthService`)
- **Entities**: Domain models representing core business objects (`User`, `BankAccount`, `Transaction`)
- **Enums**: Type-safe enumerations (`Currency`, `TransactionType`)
- **Repository Interfaces**: Contracts for data access (`IUserRepository`, `IUnitOfWork`)
- **Exceptions**: Custom domain exceptions (`NotFoundException`, `InsufficientFundsException`)
- **Responsibility**: Business rules, domain logic, contracts/interfaces
- **Dependencies**: None (Core doesn't reference API or DAL)

### **API Layer (EagleBankAPI)** - Presentation
- **Controllers**: Handle HTTP requests/responses, routing, authentication, DTO ↔ Entity mapping
- **Models (DTOs)**: Request/Response objects for API contracts (HTTP layer concerns)
- **Middleware**: Cross-cutting concerns (global exception handler)
- **Program.cs**: Application startup, DI container configuration
- **Responsibility**: API orchestration, HTTP concerns, endpoint routing, DTO mapping
- **Dependencies**: References Core (for services and entities)

### **Data Access Layer (EagleBankAPI.DAL)** - Infrastructure
- **Repository Implementations**: Concrete implementations of repository interfaces (`UserRepository`, `UnitOfWork`)
- **DbContext**: Entity Framework Core configuration (`EagleBankDbContext`)
- **Responsibility**: Data persistence, database operations only
- **Dependencies**: References Core (for entities and repository interfaces)

### **Dependency Flow**
```
       ┌─────────────────┐
       │  EagleBankAPI   │  (Presentation)
       │   (API Layer)   │
       └────────┬────────┘
                │ references
                ↓
       ┌─────────────────┐
       │ EagleBankAPI    │  (Business Logic + Domain)
       │   .Core         │  ← No external dependencies
       └────────▲────────┘
                │ references
                │
       ┌────────┴────────┐
       │ EagleBankAPI    │  (Infrastructure)
       │   .DAL          │
       └─────────────────┘

API → Core ← DAL
(Core defines interfaces; DAL implements them)
```

### **Key Design Decisions**
- ✅ **Three-project Clean Architecture** - Core, API, and DAL separated for maintainability and testability
- ✅ **Core is dependency-free** - Business logic doesn't depend on infrastructure or presentation
- ✅ **Dependency Inversion Principle** - Core defines interfaces; DAL/API implement/consume them
- ✅ **DTOs in API layer** - Proper separation: DTOs are HTTP concerns, not domain concerns
- ✅ **Services return Entities** - Business logic works with domain models, not DTOs
- ✅ **Controllers map DTO ↔ Entity** - Presentation layer handles translation between HTTP and domain
- ✅ **Entities use enums** - Type safety in domain code (`Currency.GBP`, `TransactionType.Deposit`)
- ✅ **DTOs use strings** - Language-agnostic REST API (e.g., "GBP", "Deposit")
- ✅ **Repository Pattern** - Abstracts data access for testability
- ✅ **Unit of Work** - Manages transactions across multiple repositories
- ✅ **Atomic operations** - UnitOfWork ensures all-or-nothing database transactions (e.g., creating account + initial transaction either both succeed or both fail)
- ✅ **One class per file** - Idiomatic C# structure for maintainability
- ✅ **Requests/Responses organization** - Makes data flow direction immediately obvious
- ✅ **Global exception handler middleware** - Centralized exception-to-HTTP mapping, keeps controllers focused on orchestration
- ✅ **Custom domain exceptions** - Typed exceptions with context (e.g., `InsufficientFundsException`, `NotFoundException`) for maintainable error handling

### **Exception Handling Architecture**

The API implements a comprehensive exception handling system with global middleware and typed domain exceptions:

#### Custom Domain Exceptions
- **`NotFoundException`** - Resource not found (404)
- **`ForbiddenException`** - Authorization failures (403)
- **`InsufficientFundsException`** - Withdrawal exceeds balance (422) - includes requested and available amounts
- **`DuplicateEmailException`** - Email already registered (409) - includes email
- **`InvalidTransactionException`** - Invalid transaction type (400)
- **`UserHasAccountsException`** - Cannot delete user with accounts (409) - includes user ID and account count

#### Global Exception Handler Middleware
Located at `/Middleware/GlobalExceptionHandlerMiddleware.cs`:
- Catches all unhandled exceptions from services
- Maps custom exceptions to appropriate HTTP status codes using pattern matching
- Logs all exceptions with stack traces for debugging
- Returns consistent `ErrorResponse` JSON format
- Eliminates repetitive try/catch blocks in controllers

**Benefits:**
- **DRY Principle** - Exception-to-HTTP mapping defined once, not in every controller method
- **Clean Controllers** - Controllers focus on orchestration, not error handling
- **Consistent Responses** - All errors return the same JSON structure
- **Centralized Logging** - All exceptions logged in one place
- **Type Safety** - Strongly-typed exceptions with contextual properties vs generic exceptions
- **Senior-Level Practice** - Demonstrates understanding of cross-cutting concerns and middleware patterns

## Features Implemented

### Core Functionality
- ✅ User Management (Create, Read, Update, Delete)
- ✅ Bank Account Management (CRUD operations)
- ✅ Transaction Processing (Deposits & Withdrawals)
- ✅ JWT Authentication
- ✅ In-Memory Database (configurable to SQL Server)
- ✅ Repository Pattern with Unit of Work
- ✅ Service Layer with Business Logic
- ✅ Comprehensive Error Handling

### API Endpoints

#### Authentication
- `POST /v1/auth/login` - Authenticate user and receive JWT token

#### Users
- `POST /v1/users` - Create new user (no auth required)
- `GET /v1/users/{userId}` - Get user details (auth required)
- `PATCH /v1/users/{userId}` - Update user details (auth required)
- `DELETE /v1/users/{userId}` - Delete user (auth required, only if no accounts)

#### Bank Accounts
- `POST /v1/accounts` - Create new bank account (auth required)
- `GET /v1/accounts` - List user's bank accounts (auth required)
- `GET /v1/accounts/{accountNumber}` - Get account details (auth required)
- `PATCH /v1/accounts/{accountNumber}` - Update account (auth required)
- `DELETE /v1/accounts/{accountNumber}` - Delete account (auth required)

#### Transactions
- `POST /v1/accounts/{accountNumber}/transactions` - Create transaction (auth required)
- `GET /v1/accounts/{accountNumber}/transactions` - List transactions (auth required)
- `GET /v1/accounts/{accountNumber}/transactions/{transactionId}` - Get transaction details (auth required)

## Technologies Used

- **.NET 10.0** - Latest framework
- **Entity Framework Core 10.0** - ORM for data access
- **JWT Bearer Authentication** - Secure API authentication
- **In-Memory Database** - For testing (configurable to SQL Server)
- **Repository Pattern** - Data access abstraction
- **Unit of Work Pattern** - Transaction management
- **Service Layer Pattern** - Business logic separation
- **Swagger/OpenAPI** - API documentation
- **Docker** - Containerization for easy deployment

## Docker Configuration

The application is fully containerized for easy deployment and portability.

### Dockerfile
- Multi-stage build for optimized image size
- Uses official Microsoft .NET SDK 10.0 for build
- Uses official Microsoft ASP.NET Core 10.0 runtime for final image
- Exposes ports 8080 (HTTP)
- Configured with development environment by default

### Docker Compose
- Single service configuration
- Port mapping: `5000:8080` (host:container)
- Environment variables for configuration
- Automatic restart policy
- Isolated network for security

### Benefits of Containerization
- ✅ Consistent environment across all machines
- ✅ No .NET SDK installation required (only Docker)
- ✅ Easy to deploy to cloud platforms (Azure, AWS, etc.)
- ✅ Simplified CI/CD pipeline integration
- ✅ Portable and reproducible builds

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EagleBankDB;...",
    "InMemoryConnection": "InMemory"
  },
  "JwtSettings": {
    "SecretKey": "REPLACE_WITH_SECURE_KEY_FROM_ENVIRONMENT",
    "Issuer": "EagleBankAPI",
    "Audience": "EagleBankAPIUsers",
    "ExpiryMinutes": "60"
  },
  "UseInMemoryDatabase": true
}
```

**Security Note:** The JWT secret key should **never** be committed to source control in production. For local development, the secret is stored in `appsettings.Development.json` (excluded from git). In production, set the secret via environment variable:
```bash
export JwtSettings__SecretKey="your-production-secret-key"
```

## Running the Application

### Option 1: Using Docker (Recommended) 🐳

**Prerequisites:**
- Docker Desktop installed

**Steps:**

1. **Navigate to the project directory:**
   ```bash
   cd "\API"
   ```

2. **Build and run with Docker Compose:**
   ```bash
   docker-compose up --build
   ```

3. **Access the API:**
   - Swagger UI: `http://localhost:5000`
   - API Base URL: `http://localhost:5000`

4. **Stop the container:**
   ```bash
   docker-compose down
   ```

**Or build and run manually:**
```bash
# Build the Docker image
docker build -t eaglebank-api .

# Run the container
docker run -d -p 5000:8080 --name eaglebank-api eaglebank-api

# View logs
docker logs eaglebank-api

# Stop the container
docker stop eaglebank-api
docker rm eaglebank-api
```

### Option 2: Using .NET CLI

**Prerequisites:**
- .NET 10 SDK installed
- (Optional) SQL Server for persistent storage

**Steps:**

1. **Navigate to the project directory:**
   ```bash
   cd API
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the solution:**
   ```bash
   dotnet build
   ```

4. **Run the API:**
   ```bash
   dotnet run --project EagleBankAPI
   ```

5. **Access Swagger UI:**
   - Open browser to `http://localhost:5227` (or check console for actual port)
   - Swagger UI will be displayed at the root

## API Usage Examples

### 1. Create a User
```bash
POST /v1/users
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john.doe@example.com",
  "password": "SecurePassword123!",
  "phoneNumber": "+441234567890",
  "address": {
    "line1": "123 Main Street",
    "town": "London",
    "county": "Greater London",
    "postcode": "SW1A 1AA"
  }
}
```

### 2. Login
```bash
POST /v1/auth/login
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "SecurePassword123!"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "usr-abc123def456"
}
```

### 3. Create Bank Account
```bash
POST /v1/accounts
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "name": "My Savings Account",
  "accountType": "personal"
}
```

### 4. Deposit Money
```bash
POST /v1/accounts/{accountNumber}/transactions
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "amount": 1000.00,
  "currency": "GBP",
  "type": "deposit",
  "reference": "Initial deposit"
}
```

### 5. Withdraw Money
```bash
POST /v1/accounts/{accountNumber}/transactions
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "amount": 50.00,
  "currency": "GBP",
  "type": "withdrawal",
  "reference": "ATM withdrawal"
}
```

## Security Features

- **Password Hashing**: PBKDF2 with SHA256 (10,000 iterations)
- **JWT Authentication**: Secure token-based authentication
- **Authorization Checks**: Users can only access their own resources
- **Input Validation**: Comprehensive validation on all endpoints
- **Transaction Safety**: Unit of Work pattern ensures atomicity

## Business Rules Implemented

1. **User Management**
   - Users can only view/modify their own details
   - Users cannot be deleted if they have bank accounts
   - Email must be unique

2. **Bank Accounts**
   - Account numbers are auto-generated (01XXXXXX format)
   - Sort code is fixed: 10-10-10
   - Currency is GBP
   - Users can only manage their own accounts

3. **Transactions**
   - Withdrawals require sufficient funds (422 if insufficient)
   - Transactions are immutable (cannot be updated or deleted)
   - Balance is updated atomically with transaction creation
   - Users can only create transactions for their own accounts

## Architecture Highlights

### Repository Pattern
- Generic repository for common CRUD operations
- Specific repositories for specialized queries
- Unit of Work for transaction management

### Unit of Work Pattern
- Coordinates multiple repository operations into a single database transaction
- Ensures atomic operations - all changes succeed or all fail together
- Example: Creating a transaction and updating account balance either both succeed or both rollback
- Provides `BeginTransactionAsync()`, `CommitTransactionAsync()`, and `RollbackTransactionAsync()`
- Single `SaveChangesAsync()` call for all repositories prevents partial updates

### Service Layer
- Business logic separated from controllers
- Services coordinate between repositories
- Authorization logic centralized in services

### DTOs
- Request/Response models separate from entities
- Clean API contract
- Entity-to-DTO mapping in services

### Error Handling
- Consistent error responses
- Appropriate HTTP status codes
- Detailed validation errors

## Testing

### Unit Tests ✅ Implemented (29 tests passing)

Comprehensive unit tests for the service layer using xUnit, Moq, and FluentAssertions:

- **AuthServiceTests** - Login authentication, password verification, token generation, exception handling
- **UserServiceTests** - User CRUD operations, authorization, duplicate email validation, cascade delete restrictions
- **BankAccountServiceTests** - Account creation, ownership validation, account management
- **TransactionServiceTests** - Deposits, withdrawals, insufficient funds handling, transaction retrieval

**Run tests:**
```bash
dotnet test
```

### Future Testing Enhancements
- Integration tests for end-to-end API flows
- Controller tests with mocked services
- Database integration testing
- Authentication flow testing

## Future Enhancements

- [ ] Add pagination for list endpoints
- [ ] Implement account types beyond "personal"
- [ ] Add transaction filtering and search
- [ ] Implement transfer between accounts
- [ ] Add audit logging
- [ ] Implement rate limiting
- [ ] Add caching layer
- [ ] Integration tests for end-to-end API flows
- [ ] API versioning strategy
- [ ] HATEOAS links in responses

## Notes 
This implementation demonstrates:
- **Clean Architecture**: Three-layer separation with Core (business logic + domain), API (presentation), and DAL (infrastructure) - proper dependency flow with Core as the dependency-free center
- **Best Practices**: Repository pattern, Unit of Work, Dependency Injection, Dependency Inversion Principle
- **Security**: JWT authentication, password hashing, authorization
- **RESTful Design**: Proper HTTP methods, status codes, and resource naming
- **Error Handling**: Comprehensive exception handling with global middleware and custom domain exceptions
- **Scalability**: Designed for easy extension and maintenance
- **Documentation**: Well-structured code with clear naming conventions

## License

This is a practise REST API project in .NET demonstrating clean architecture principles.
