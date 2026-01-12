using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Hospital.Models;
using Hospital.Data;

namespace Hospital.Areas.Identity.Pages.Account.Manage
{
    public class MedicalRecordDetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MedicalRecordDetailsModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public HoSoBenhAn HoSo { get; set; }
        public decimal TongDichVu { get; set; }
        public decimal TongThuoc { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            HoSo = await _context.HoSoBenhAn
                .Include(h => h.BacSi)
                .Include(h => h.BenhNhan)
                .Include(h => h.HinhAnhBenhAns)
                .Include(h => h.ChiTietDonThuocs).ThenInclude(d => d.Thuoc)
                .Include(h => h.ChiTietDichVus).ThenInclude(s => s.DichVu)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (HoSo == null || HoSo.BenhNhan.Email != user.Email) return RedirectToPage("./MedicalHistory");

            // Tính toán tổng tiền ngay trong Model
            TongDichVu = HoSo.ChiTietDichVus?.Sum(x => x.DichVu.Gia) ?? 0;
            TongThuoc = HoSo.ChiTietDonThuocs?.Sum(x => x.SoLuong * x.Thuoc.GiaBan) ?? 0;

            return Page();
        }
    }
}