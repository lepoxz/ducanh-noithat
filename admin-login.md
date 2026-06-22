# Admin Login Implementation Plan

## Goal
Implement a secure admin login mechanism using ASP.NET Core Cookie Authentication and SQLite DB database storage, with a premium minimalist UI design.

## Tasks
- [ ] Task 1: Create `Models/Admin.cs` → Verify: File exists and matches SQLite requirements
- [ ] Task 2: Update `Data/ApplicationDbContext.cs` → Verify: DbSet<Admin> registered
- [ ] Task 3: Create EF migration `AddAdminsTable` and run `dotnet ef database update` → Verify: Bảng `Admins` được tạo trong `app.db`
- [ ] Task 4: Seed default Admin user in `Data/DbInitializer.cs` → Verify: Default admin user seeded with securely hashed password
- [ ] Task 5: Configure Cookie Authentication in `Program.cs` and secure `/Pages/Admin` routes → Verify: Unauthenticated visits to `/Admin/*` are redirected to `/Admin/Login`
- [ ] Task 6: Create `Pages/Admin/Login.cshtml` and `Pages/Admin/Login.cshtml.cs` with custom premium design → Verify: Admin can log in successfully with valid credentials, error shown for invalid credentials
- [ ] Task 7: Update Admin Navigation/Layout to support Logout → Verify: Admin can log out successfully

## Done When
- [ ] Direct access to `/Admin` pages requires authentication and redirects to `/Admin/Login`
- [ ] Logged-in admin can access all management pages and log out successfully
- [ ] Database stores hashed password securely

## Notes
- Use `Microsoft.AspNetCore.Identity.PasswordHasher<Admin>` or similar built-in utility for secure hashing.
- Ensure the login UI does not use violet/purple colors (Purple Ban). Keep it minimal and elegant using the existing theme.
