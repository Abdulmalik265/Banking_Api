# Banking API

A comprehensive and secure banking REST API built with .NET 9 and MySQL. Supports user registration, JWT authentication, account management, fund transfers, and transaction history.

## Tech Stack

- **.NET 9** (ASP.NET Core Web API)
- **MySQL 8** (via Pomelo.EntityFrameworkCore.MySql)
- **Entity Framework Core 9** (ORM with Code-First Migrations)
- **JWT Bearer Authentication** (HMAC-SHA256)
- **BCrypt** (Password hashing)
- **Swagger / OpenAPI** (API documentation)
- **xUnit** (Unit testing with EF Core InMemory provider)

## Project Structure

```
BankingApi/
├── Controllers/
│   ├── AuthController.cs          # Registration & Login
│   ├── AccountController.cs       # Account info & updates
│   └── TransactionController.cs   # Fund transfers & history
├── Services/
│   ├── AuthService.cs             # Auth business logic
│   ├── AccountService.cs          # Account business logic
│   └── TransactionService.cs      # Transfer business logic
├── Models/
│   ├── User.cs
│   ├── Account.cs
│   └── Transaction.cs
├── DTOs/
│   ├── AuthDtos.cs                # Register/Login request & response
│   ├── AccountDtos.cs             # Account request & response
│   └── TransactionDtos.cs         # Transfer request, response & ApiResponse<T>
├── Data/
│   └── BankingDbContext.cs         # EF Core DbContext configuration
├── Migrations/                    # EF Core migrations (auto-generated)
├── BankingApi.Tests/              # Unit tests
│   ├── AuthServiceTests.cs
│   ├── AccountServiceTests.cs
│   ├── TransactionServiceTests.cs
│   └── Helpers/TestDbHelper.cs
├── DatabaseSetup.sql              # SQL script for manual setup (optional)
├── Program.cs                     # Application entry point & DI configuration
├── appsettings.json               # Configuration
└── BankingApi.sln                 # Solution file
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [MySQL 8](https://dev.mysql.com/downloads/mysql/) (or Docker: `docker run -d -p 3306:3306 -e MYSQL_ROOT_PASSWORD=yourpassword mysql:8`)

## Database Setup

### Option 1: Automatic (Recommended)

1. Create the database in MySQL:

```sql
CREATE DATABASE IF NOT EXISTS banking_api;
```

2. Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=banking_api;User=root;Password=YOUR_PASSWORD;"
}
```

3. Generate and apply the migration:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

The tables (users, accounts, transactions) are created automatically.

### Option 2: Manual SQL Script

Run the provided `DatabaseSetup.sql` script in MySQL Workbench or CLI:

```bash
mysql -u root -p < DatabaseSetup.sql
```

## Running the Application

```bash
dotnet run
```

The API will start at:
- **HTTPS:** https://localhost:5001
- **HTTP:** http://localhost:5000

Swagger UI is available at: **https://localhost:5001/swagger**

## API Endpoints

### Authentication (Public)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and receive JWT token |

### Account Management (Requires JWT)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/account` | Get account details and balance |
| PUT | `/api/account` | Update contact information |

### Transactions (Requires JWT)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/transaction/transfer` | Transfer funds to another account |
| GET | `/api/transaction/history` | Get paginated transaction history |

## Usage Examples

### 1. Register a User

```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Abdulmalik Sulaiman",
    "email": "abdulmalik@bank.com",
    "password": "Password123",
    "phone": "08012345678"
  }'
```

**Response (201):**
```json
{
  "code": 201,
  "message": "Registration successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "expires": "2026-03-31T10:00:00Z",
    "user": {
      "id": 1,
      "fullName": "Abdulmalik Sulaiman",
      "email": "abdulmalik@bank.com",
      "accountNumber": "1847362950",
      "balance": 0
    }
  }
}
```

### 2. Login

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "abdulmalik@bank.com",
    "password": "Password123"
  }'
```

### 3. Transfer Funds

```bash
curl -X POST https://localhost:5001/api/transaction/transfer \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "recipientAccountNumber": "2222222222",
    "amount": 5000,
    "narration": "Payment for services"
  }'
```

### 4. View Transaction History

```bash
curl https://localhost:5001/api/transaction/history?page=1&pageSize=10 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Using Swagger UI

1. Run the application (`dotnet run`)
2. Open https://localhost:5001/swagger in your browser
3. Register or login to get a JWT token
4. Click the **Authorize** button (lock icon)
5. Enter: `Bearer YOUR_JWT_TOKEN`
6. Test any protected endpoint directly from the browser

## Running Tests

```bash
dotnet test
```

The test suite includes 25 unit tests covering:

- **Auth:** Registration, login, duplicate emails, wrong password, non-existent user
- **Account:** Retrieve account info, update name/phone, invalid user, empty field handling
- **Transactions:** Valid transfer, insufficient balance, self-transfer prevention, inactive accounts, exact balance transfer, invalid recipient, pagination, recipient history view

## Key Design Decisions

- **ApiResponse\<T\> Wrapper:** All endpoints return a consistent `{ code, message, data }` format
- **Database Transactions:** Fund transfers use `BeginTransactionAsync`/`CommitAsync` with rollback to ensure atomicity
- **Balance Snapshots:** Each transaction records sender/recipient balances before and after for audit trail
- **BCrypt Hashing:** Passwords are securely hashed with automatic salting
- **Stateless Auth:** JWT tokens contain user claims; no server-side session storage
- **Auto-Migration:** Application applies pending EF Core migrations on startup

## Security Features

- JWT Bearer token authentication with 24-hour expiry
- BCrypt password hashing
- Input validation via DataAnnotations
- Parameterized queries (EF Core) preventing SQL injection
- Foreign key constraints with DELETE RESTRICT on transactions
- Unique indexes on email, account number, and transaction reference

## License

This project was built as a technical assessment.
