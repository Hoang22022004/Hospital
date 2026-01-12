using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;
namespace Hospital
{
    using Hospital.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Identity;
    using Hospital.Models;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Lấy chuỗi kết nối từ appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // 2. Đăng ký ApplicationDbContext với SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 3. Cấu hình Identity (Bao gồm hỗ trợ Roles)
            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>() // Quan trọng: Thêm hỗ trợ Role
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";

                // CẤU HÌNH QUAN TRỌNG CHO "REMEMBER ME"
                options.ExpireTimeSpan = TimeSpan.FromDays(30); // Cookie sống trong 30 ngày
                options.SlidingExpiration = true; // Tự động gia hạn khi người dùng còn hoạt động
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;

                // Nếu bạn đang chạy localhost không có HTTPS, hãy thêm dòng này:
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            // ***************************************************************
            // BỔ SUNG QUAN TRỌNG CHO DATA SEEDING (Bước 6)
            // ***************************************************************
            // 3b. Đăng ký DbInitializer (Service khởi tạo dữ liệu)
            builder.Services.AddScoped<DbInitializer>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // ***************************************************************
            // BỔ SUNG QUAN TRỌNG CHO DATA SEEDING (Gọi hàm khởi tạo)
            // ***************************************************************
            // Khai báo và gọi hàm để thực hiện Seeding khi ứng dụng chạy
            SeedDatabase();

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

            // *************************************************************
            // KHẮC PHỤC LỖI KHÔNG TÌM THẤY TRANG IDENTITY (Đăng nhập, Đăng ký)
            // *************************************************************
            app.MapRazorPages(); // Dòng này là bắt buộc để mapping các trang Identity

            // 5. Định tuyến cho Area (Phải đặt TRƯỚC định tuyến mặc định)
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            // Định tuyến mặc định (cho giao diện Khách hàng)
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();

            // Hàm thực hiện Seeding Database
            void SeedDatabase()
            {
                using (var scope = app.Services.CreateScope())
                {
                    // Lấy service DbInitializer đã đăng ký
                    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
                    // Gọi hàm khởi tạo
                    dbInitializer.Initialize().Wait();
                }
            }
        }
    }
}