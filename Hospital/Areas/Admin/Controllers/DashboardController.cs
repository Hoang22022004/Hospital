using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;
using Hospital.Models;
using System.Linq;
using Newtonsoft.Json;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AdminDashboard()
        {
            var today = DateTime.Today;
            var currentYear = today.Year;
            var currentMonth = today.Month;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));

            // 1. THỐNG KÊ 4 THẺ STAT CARDS
            ViewBag.TotalPatients = await _context.BenhNhan.CountAsync();
            ViewBag.AppointmentsToday = await _context.LichHen.CountAsync(l => l.LichLamViec.NgayLamViec.Date == today);
            ViewBag.CompletedToday = await _context.HoSoBenhAn.CountAsync(h => h.NgayKham.Date == today && h.TrangThai == TrangThaiHoSo.HoanThanh);

            var allRecords = await _context.HoSoBenhAn
                .Where(h => h.NgayKham.Year == currentYear && h.TrangThai == TrangThaiHoSo.HoanThanh)
                .Include(h => h.ChiTietDichVus).ThenInclude(d => d.DichVu)
                .Include(h => h.ChiTietDonThuocs).ThenInclude(t => t.Thuoc)
                .Include(h => h.BacSi)
                .ToListAsync();

            decimal GetTotal(HoSoBenhAn h) =>
                (h.ChiTietDichVus?.Sum(d => d.DichVu.Gia) ?? 0) +
               h.ChiTietDonThuocs?.Sum(t => (decimal)(t.SoLuong) * (t.Thuoc?.GiaBan ?? 0)) ?? 0;

            // Doanh thu hôm nay
            ViewBag.RevDay = allRecords.Where(h => h.NgayKham.Date == today).Sum(GetTotal);

            // 2. TỔNG DOANH THU GIAI ĐOẠN
            ViewBag.RevWeekTotal = allRecords.Where(h => h.NgayKham.Date >= startOfWeek).Sum(GetTotal);
            ViewBag.RevMonthTotal = allRecords.Where(h => h.NgayKham.Month == currentMonth).Sum(GetTotal);
            ViewBag.RevYearTotal = allRecords.Sum(GetTotal);

            // 3. DỮ LIỆU BIỂU ĐỒ
            var yearData = Enumerable.Range(1, 12).Select(m => allRecords.Where(h => h.NgayKham.Month == m).Sum(GetTotal)).ToArray();
            ViewBag.YearLabels = JsonConvert.SerializeObject(Enumerable.Range(1, 12).Select(m => "T" + m));
            ViewBag.YearData = JsonConvert.SerializeObject(yearData);

            int days = DateTime.DaysInMonth(currentYear, currentMonth);
            var monthData = Enumerable.Range(1, days).Select(d => allRecords.Where(h => h.NgayKham.Month == currentMonth && h.NgayKham.Day == d).Sum(GetTotal)).ToArray();
            ViewBag.MonthLabels = JsonConvert.SerializeObject(Enumerable.Range(1, days).Select(d => d.ToString()));
            ViewBag.MonthData = JsonConvert.SerializeObject(monthData);

            var weekData = Enumerable.Range(0, 7).Select(i => allRecords.Where(h => h.NgayKham.Date == startOfWeek.AddDays(i)).Sum(GetTotal)).ToArray();
            ViewBag.WeekLabels = JsonConvert.SerializeObject(new string[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" });
            ViewBag.WeekData = JsonConvert.SerializeObject(weekData);

            // 4. PHÂN TÍCH BỆNH LÝ & BÁC SĨ
            var pathology = allRecords.Where(h => !string.IsNullOrEmpty(h.ChanDoan)).GroupBy(h => h.ChanDoan)
                .Select(g => new { Label = g.Key, Count = g.Count() }).OrderByDescending(g => g.Count).Take(5).ToList();
            ViewBag.PathLabels = JsonConvert.SerializeObject(pathology.Select(s => s.Label));
            ViewBag.PathCounts = JsonConvert.SerializeObject(pathology.Select(s => s.Count));

            var drRanking = allRecords.Where(h => h.NgayKham.Month == currentMonth).GroupBy(h => h.BacSi.HoTen)
                .Select(g => new { Name = g.Key ?? "N/A", Count = g.Count() }).OrderByDescending(g => g.Count).Take(5).ToList();
            ViewBag.DrLabels = JsonConvert.SerializeObject(drRanking.Select(d => d.Name));
            ViewBag.DrCounts = JsonConvert.SerializeObject(drRanking.Select(d => d.Count));

            return View();
        }
    }
}