using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Hospital.Models;
using Hospital.Data;
using Microsoft.AspNetCore.Mvc;

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
                MedicalHistories = await _context.HoSoBenhAn
                    .Include(h => h.BacSi)
                    .Include(h => h.BenhNhan)
                    .Where(h => h.BenhNhan.Email == user.Email)
                    .OrderByDescending(h => h.NgayKham)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostHuyHoSoAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var hoSo = await _context.HoSoBenhAn
                .Include(h => h.LichHen)
                .Include(h => h.BenhNhan)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Kiểm tra bảo mật và trạng thái trước khi hủy
            if (hoSo == null || hoSo.BenhNhan.Email != user.Email) return NotFound();

            if (hoSo.TrangThai == TrangThaiHoSo.ChoKham)
            {
                // Sử dụng giá trị Enum DaHuy bạn đã định nghĩa
                hoSo.TrangThai = TrangThaiHoSo.DaHuy;

                if (hoSo.LichHen != null)
                {
                    hoSo.LichHen.TrangThai = TrangThaiLichHen.DaHuy;
                }

                await _context.SaveChangesAsync();
                TempData["Status"] = "Lịch khám của bạn đã được hủy thành công.";
            }

            return RedirectToPage();
        }
    }
}