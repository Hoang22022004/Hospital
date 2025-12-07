namespace Hospital
{
    // Cần thêm các lệnh 'using' này ở đầu file hoặc sử dụng 'global using' nếu đã cấu hình
    using Hospital.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Identity;
    using Hospital.Models;

    // using Microsoft.AspNetCore.Identity.UI; // Nếu bạn dùng Identity UI mặc định

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Lấy chuỗi kết nối từ appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // 2. Đăng ký ApplicationDbContext với SQL Server
            // (Bạn sẽ tạo lớp ApplicationDbContext sau)
            IServiceCollection serviceCollection = builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 3. Cấu hình Identity (Bao gồm hỗ trợ Roles)
            // (Chúng ta đang dùng IdentityUser mặc định, sẽ thay bằng ApplicationUser sau)
            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>() // Quan trọng để dùng Role-Based Authorization
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // 4. Kích hoạt Authentication (Phải đặt trước UseAuthorization)
            app.UseAuthentication();

            app.UseAuthorization();

            // 5. Định tuyến cho Area (Phải đặt TRƯỚC định tuyến mặc định)
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            // Định tuyến mặc định (cho giao diện Khách hàng)
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}