using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;
using Hospital.Models;
// 1. Thêm directive này


namespace Hospital
{
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
                .AddRoles<IdentityRole>() // Cần thiết để phân quyền Admin/Bác sĩ
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            // 3b. Đăng ký DbInitializer (Service khởi tạo dữ liệu mẫu)
            builder.Services.AddScoped<DbInitializer>();

            // *****************************************************************************
            // 3c. CẤU HÌNH DỊCH VỤ THANH TOÁN (ĐÃ SỬA LỖI XUNG ĐỘT)
            // *****************************************************************************

            // Đổi tên biến thành payOSClient để không trùng với tên namespace Net.payOS


            // Đăng ký HttpContextAccessor: Bắt buộc để VNPAY lấy địa chỉ IP khách hàng
            builder.Services.AddHttpContextAccessor();

            // Thêm các dịch vụ cho Controller và View
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Thực hiện Seeding Database (Tạo tài khoản Admin mặc định, v.v.)
            SeedDatabase(app);

            // Cấu hình Pipeline cho yêu cầu HTTP
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // 4. Kích hoạt Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Mapping các trang Identity (Razor Pages)
            app.MapRazorPages();

            // 5. Định tuyến cho Area (Admin) - Phải đặt trước định tuyến mặc định
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            // Định tuyến mặc định cho giao diện khách hàng
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();

            // Hàm thực hiện Seeding Database
            void SeedDatabase(WebApplication appInstance)
            {
                using (var scope = appInstance.Services.CreateScope())
                {
                    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
                    dbInitializer.Initialize().Wait();
                }
            }
        }
    }
}