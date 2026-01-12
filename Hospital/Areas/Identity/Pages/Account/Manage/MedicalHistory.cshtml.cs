using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Hospital.Models;
using Hospital.Data;

namespace Hospital.Areas.Identity.Pages.Account.Manage
{
    public class MedicalHistoryModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MedicalHistoryModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public List<HoSoBenhAn> MedicalHistories { get; set; } = new List<HoSoBenhAn>();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // Lấy hồ sơ thông qua bảng BenhNhan
                MedicalHistories = await _context.HoSoBenhAn
                    .Include(h => h.BacSi)
                    .Include(h => h.BenhNhan)
                    .Where(h => h.BenhNhan.Email == user.Email) // Lọc đúng Email của bệnh nhân
                    .OrderByDescending(h => h.NgayKham)
                    .ToListAsync();
            }
        }
    }
}