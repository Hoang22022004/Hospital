using Hospital.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // Constructor để inject các service cần thiết
        public DbInitializer(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Phương thức chính để khởi tạo
        public async Task Initialize()
        {
            // 1. CHẠY MIGRATION NẾU CẦN
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu có
            }

            // 2. TẠO ROLES (Vai trò)
            // Nếu các Role chưa tồn tại, tạo chúng
            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                // Tạo Role Admin
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                // Tạo Role Doctor
                await _roleManager.CreateAsync(new IdentityRole("Doctor"));
                // Tạo Role Customer (Bệnh nhân)
                await _roleManager.CreateAsync(new IdentityRole("Customer"));

                // 3. TẠO TÀI KHOẢN ADMIN ĐẦU TIÊN
                // Tạo một tài khoản ApplicationUser mới
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@hospital.com",
                    Email = "admin@hospital.com",
                    EmailConfirmed = true,
                    FullName = "Super Administrator",
                    Address = "Ha Noi, Viet Nam",
                    DateOfBirth = DateTime.Now.AddYears(-30)
                };

                // Tạo User và gán mật khẩu
                await _userManager.CreateAsync(adminUser, "Admin@123");

                // Gán Role Admin cho tài khoản này
                await _userManager.AddToRoleAsync(adminUser, "Admin");

                // Ghi log để xác nhận
                Console.WriteLine("Data Seeding: Created Admin role and default Admin user.");
            }
            // Nếu Role đã tồn tại, không làm gì cả
        }
    }
}