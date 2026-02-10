using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    public class DichVuController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DichVuController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- HÀM HỖ TRỢ UPLOAD ẢNH CHO TINYMCE ---
        [HttpPost]
        public async Task<IActionResult> UploadImageTiny(IFormFile file)
        {
            if (file == null) return Json(new { location = "" });

            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "dichvu");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(folderPath, fileName);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            // Trả về đường dẫn đúng định dạng JSON mà TinyMCE yêu cầu
            return Json(new { location = "/images/dichvu/" + fileName });
        }

        // Action phụ trợ: Lấy danh sách Chuyên khoa
        private void PrepareChuyenKhoaForView(int? dichVuId = null)
        {
            var allChuyenKhoas = _db.ChuyenKhoa
                .OrderBy(c => c.TenChuyenKhoa)
                .Select(c => new SelectListItem
                {
                    Value = c.ChuyenKhoaId.ToString(),
                    Text = c.TenChuyenKhoa
                }).ToList();

            if (dichVuId.HasValue)
            {
                var selectedIds = _db.ChuyenKhoaDichVus
                    .Where(cd => cd.DichVuId == dichVuId.Value)
                    .Select(cd => cd.ChuyenKhoaId.ToString())
                    .ToHashSet();

                foreach (var item in allChuyenKhoas)
                {
                    if (selectedIds.Contains(item.Value))
                    {
                        item.Selected = true;
                    }
                }
            }
            ViewData["ChuyenKhoaList"] = allChuyenKhoas;
        }

        public async Task<IActionResult> Index()
        {
            var danhSachDichVu = await _db.DichVu
               .Include(d => d.ChuyenKhoaDichVus)
                   .ThenInclude(cd => cd.ChuyenKhoa)
               .ToListAsync();
            return View(danhSachDichVu);
        }

        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Create()
        {
            PrepareChuyenKhoaForView();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create(DichVu dichVu, IFormFile? file, int[] selectedChuyenKhoaIds)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\dichvu");
                    var extension = Path.GetExtension(file.FileName);
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStreams);
                    }
                    dichVu.AnhDichVuUrl = @"\images\dichvu\" + fileName + extension;
                }

                _db.DichVu.Add(dichVu);
                await _db.SaveChangesAsync();

                if (selectedChuyenKhoaIds != null && selectedChuyenKhoaIds.Length > 0)
                {
                    var newLinks = selectedChuyenKhoaIds.Select(id => new ChuyenKhoaDichVu
                    {
                        DichVuId = dichVu.DichVuId,
                        ChuyenKhoaId = id
                    }).ToList();
                    _db.ChuyenKhoaDichVus.AddRange(newLinks);
                    await _db.SaveChangesAsync();
                }

                TempData["success"] = "Thêm dịch vụ thành công.";
                return RedirectToAction(nameof(Index));
            }
            PrepareChuyenKhoaForView();
            return View(dichVu);
        }

        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();
            var dichVuFromDb = await _db.DichVu.FindAsync(id);
            if (dichVuFromDb == null) return NotFound();

            PrepareChuyenKhoaForView(id);
            return View(dichVuFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(DichVu dichVu, IFormFile? file, int[] selectedChuyenKhoaIds)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    if (dichVu.AnhDichVuUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, dichVu.AnhDichVuUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                    }

                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\dichvu");
                    var extension = Path.GetExtension(file.FileName);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStreams);
                    }
                    dichVu.AnhDichVuUrl = @"\images\dichvu\" + fileName + extension;
                }

                _db.DichVu.Update(dichVu);

                var oldLinks = _db.ChuyenKhoaDichVus.Where(cd => cd.DichVuId == dichVu.DichVuId);
                _db.ChuyenKhoaDichVus.RemoveRange(oldLinks);

                if (selectedChuyenKhoaIds != null)
                {
                    var newLinks = selectedChuyenKhoaIds.Select(id => new ChuyenKhoaDichVu
                    {
                        DichVuId = dichVu.DichVuId,
                        ChuyenKhoaId = id
                    }).ToList();
                    _db.ChuyenKhoaDichVus.AddRange(newLinks);
                }

                await _db.SaveChangesAsync();
                TempData["success"] = "Cập nhật dịch vụ thành công.";
                return RedirectToAction(nameof(Index));
            }
            PrepareChuyenKhoaForView(dichVu.DichVuId);
            return View(dichVu);
        }

        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();
            var dichVuFromDb = await _db.DichVu
                .Include(d => d.ChuyenKhoaDichVus)
                    .ThenInclude(cd => cd.ChuyenKhoa)
                .FirstOrDefaultAsync(d => d.DichVuId == id);

            if (dichVuFromDb == null) return NotFound();
            return View(dichVuFromDb);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            var obj = await _db.DichVu.FindAsync(id);
            if (obj == null) return NotFound();

            var linksToDelete = _db.ChuyenKhoaDichVus.Where(cd => cd.DichVuId == id);
            _db.ChuyenKhoaDichVus.RemoveRange(linksToDelete);

            if (obj.AnhDichVuUrl != null)
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, obj.AnhDichVuUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
            }

            _db.DichVu.Remove(obj);
            await _db.SaveChangesAsync();
            TempData["success"] = "Xóa dịch vụ thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}