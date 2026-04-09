using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hospital.Data;
using Hospital.Models;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            // 1. THIẾT LẬP THỜI GIAN CHUẨN
            var today = DateTime.Today; // 00:00:00 ngày hôm nay
            var tomorrow = today.AddDays(1);
            var currentYear = today.Year;
            var currentMonth = today.Month;

            // Tính ngày đầu tuần (Thứ 2)
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));

            // 2. LẤY TẬP DỮ LIỆU GỐC HỒ SƠ (Lấy toàn bộ trong năm để vẽ biểu đồ và tính toán)
            var allRecords = await _context.HoSoBenhAn
                .Where(h => h.NgayKham.Year == currentYear)
                .Include(h => h.ChiTietDichVus).ThenInclude(d => d.DichVu)
                .Include(h => h.ChiTietDonThuocs).ThenInclude(t => t.Thuoc)
                .Include(h => h.BacSi)
                .ToListAsync();

            // Hàm helper tính tổng tiền một hồ sơ (Dịch vụ + Thuốc)
            decimal GetTotal(HoSoBenhAn h)
            {
                decimal dv = h.ChiTietDichVus?.Sum(d => (decimal)(d.DichVu?.Gia ?? 0)) ?? 0;
                decimal thuoc = h.ChiTietDonThuocs?.Sum(t => (decimal)(t.SoLuong) * (decimal)(t.Thuoc?.GiaBan ?? 0)) ?? 0;
                return dv + thuoc;
            }

            // --- 3. CẬP NHẬT 4 THẺ THỐNG KÊ (STAT CARDS) ---

            // Thẻ 1: Tổng bệnh nhân (Tất cả thời gian)
            ViewBag.TotalPatients = await _context.BenhNhan.CountAsync();

            // Thẻ 2: Lịch hẹn hôm nay (Lọc theo ngày làm việc của bác sĩ trong ca trực)
            ViewBag.AppointmentsToday = await _context.LichHen
                .Include(l => l.LichLamViec)
                .CountAsync(l => l.LichLamViec.NgayLamViec.Date == today && l.TrangThai != TrangThaiLichHen.DaHuy);

            // LOGIC ĐỒNG BỘ: Lọc danh sách hồ sơ khớp với ngày hôm nay (bao gồm cả giờ lẻ như 19:51)
            var recordsToday = allRecords
                .Where(h => h.NgayKham >= today && h.NgayKham < tomorrow && h.TrangThai == TrangThaiHoSo.HoanThanh)
                .ToList();

            // Thẻ 3: Hoàn tất hôm nay
            ViewBag.CompletedToday = recordsToday.Count;

            // Thẻ 4: Doanh thu hôm nay
            ViewBag.RevDay = recordsToday.Sum(GetTotal);


            // --- 4. DỮ LIỆU BIỂU ĐỒ DOANH THU CHÍNH (Năm/Tháng/Tuần) ---

            ViewBag.RevWeekTotal = allRecords.Where(h => h.NgayKham.Date >= startOfWeek && h.TrangThai == TrangThaiHoSo.HoanThanh).Sum(GetTotal);
            ViewBag.RevMonthTotal = allRecords.Where(h => h.NgayKham.Month == currentMonth && h.TrangThai == TrangThaiHoSo.HoanThanh).Sum(GetTotal);
            ViewBag.RevYearTotal = allRecords.Where(h => h.TrangThai == TrangThaiHoSo.HoanThanh).Sum(GetTotal);

            // Dữ liệu biểu đồ Năm (12 tháng)
            var yearData = Enumerable.Range(1, 12).Select(m => allRecords.Where(h => h.NgayKham.Month == m && h.TrangThai == TrangThaiHoSo.HoanThanh).Sum(GetTotal)).ToArray();
            ViewBag.YearLabels = JsonConvert.SerializeObject(Enumerable.Range(1, 12).Select(m => "T" + m));
            ViewBag.YearData = JsonConvert.SerializeObject(yearData);

            // Dữ liệu biểu đồ Tháng (theo từng ngày)
            int dMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            var monthData = Enumerable.Range(1, dMonth).Select(d => allRecords.Where(h => h.NgayKham.Month == currentMonth && h.NgayKham.Day == d && h.TrangThai == TrangThaiHoSo.HoanThanh).Sum(GetTotal)).ToArray();
            ViewBag.MonthLabels = JsonConvert.SerializeObject(Enumerable.Range(1, dMonth).Select(d => d.ToString()));
            ViewBag.MonthData = JsonConvert.SerializeObject(monthData);

            // Dữ liệu biểu đồ Tuần (Thứ 2 -> CN)
            var weekData = Enumerable.Range(0, 7).Select(i => allRecords.Where(h => h.NgayKham.Date == startOfWeek.AddDays(i) && h.TrangThai == TrangThaiHoSo.HoanThanh).Sum(GetTotal)).ToArray();
            ViewBag.WeekLabels = JsonConvert.SerializeObject(new string[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" });
            ViewBag.WeekData = JsonConvert.SerializeObject(weekData);


            // --- 5. PHÂN TÍCH BIỂU ĐỒ PHỤ: BỆNH LÝ & BÁC SĨ ---

            var benhLyDict = await _context.BenhLy.AsNoTracking().ToDictionaryAsync(b => b.BenhLyId, b => b.TenBenhLy);

            // Cơ cấu bệnh lý (Top 5 loại bệnh chẩn đoán nhiều nhất)
            var pathology = allRecords
                .Where(h => !string.IsNullOrEmpty(h.ChanDoan) && h.TrangThai == TrangThaiHoSo.HoanThanh)
                .GroupBy(h => h.ChanDoan)
                .Select(g => new {
                    Label = benhLyDict.ContainsKey(g.Key) ? benhLyDict[g.Key] : g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count).Take(5).ToList();

            ViewBag.PathLabels = JsonConvert.SerializeObject(pathology.Select(s => s.Label));
            ViewBag.PathCounts = JsonConvert.SerializeObject(pathology.Select(s => s.Count));

            // Hiệu suất Bác sĩ (Top 5 bác sĩ khám nhiều nhất)
            var drRanking = allRecords
                .Where(h => h.TrangThai == TrangThaiHoSo.HoanThanh)
                .GroupBy(h => h.BacSi?.HoTen)
                .Select(g => new { Name = g.Key ?? "N/A", Count = g.Count() })
                .OrderByDescending(g => g.Count).Take(5).ToList();

            ViewBag.DrLabels = JsonConvert.SerializeObject(drRanking.Select(d => d.Name));
            ViewBag.DrCounts = JsonConvert.SerializeObject(drRanking.Select(d => d.Count));

            return View();
        }

        // API xử lý lọc khoảng ngày tùy chỉnh cho biểu đồ (AJAX gọi từ View)
        [HttpGet]
        public async Task<IActionResult> GetDataByRange(DateTime start, DateTime end)
        {
            var endNext = end.Date.AddDays(1);
            var records = await _context.HoSoBenhAn
                .Where(h => h.NgayKham >= start.Date && h.NgayKham < endNext && h.TrangThai == TrangThaiHoSo.HoanThanh)
                .Include(h => h.ChiTietDichVus).ThenInclude(d => d.DichVu)
                .Include(h => h.ChiTietDonThuocs).ThenInclude(t => t.Thuoc)
                .ToListAsync();

            decimal Calc(HoSoBenhAn h) =>
                (h.ChiTietDichVus?.Sum(d => (decimal)(d.DichVu?.Gia ?? 0)) ?? 0) +
                (h.ChiTietDonThuocs?.Sum(t => (decimal)(t.SoLuong) * (decimal)(t.Thuoc?.GiaBan ?? 0)) ?? 0);

            var groupedData = records.GroupBy(h => h.NgayKham.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(Calc) })
                .OrderBy(x => x.Date).ToList();

            return Json(new
            {
                labels = groupedData.Select(d => d.Date.ToString("dd/MM")),
                values = groupedData.Select(d => d.Total),
                total = groupedData.Sum(d => d.Total),
                count = (end.Date - start.Date).Days + 1
            });
        }
    }
}