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

        public DbInitializer(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task Initialize()
        {
            // 1. Tự động cập nhật Database (Migration) nếu có thay đổi
            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi Migration: " + ex.Message);
            }

            // 2. TẠO DANH SÁCH VAI TRÒ (ROLES)
            // Đảm bảo tạo đủ 4 vai trò quan trọng nhất của hệ thống
            string[] roleNames = { "Admin", "Doctor", "Receptionist", "Customer" };

            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 3. TẠO TÀI KHOẢN QUẢN TRỊ VIÊN MẶC ĐỊNH (ADMIN)
            var adminEmail = "admin@hospital.com";
            var userAdmin = await _userManager.FindByEmailAsync(adminEmail);

            if (userAdmin == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Hệ Thống Quản Trị",
                    Address = "Huế, Việt Nam",
                    DateOfBirth = new DateTime(1990, 1, 1)
                };

                var result = await _userManager.CreateAsync(newAdmin, "Admin@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // 4. TẠO TÀI KHOẢN BỆNH NHÂN MẪU (Để bạn Test thanh toán)
            var patientEmail = "benhnhan@gmail.com";
            var userPatient = await _userManager.FindByEmailAsync(patientEmail);

            if (userPatient == null)
            {
                var newPatient = new ApplicationUser
                {
                    UserName = patientEmail,
                    Email = patientEmail,
                    EmailConfirmed = true,
                    FullName = "Bệnh Nhân Test",
                    Address = "52 Chế Lan Viên, Huế"
                };

                var result = await _userManager.CreateAsync(newPatient, "Benhnhan@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newPatient, "Customer");
                }
            }
        }
    }
}