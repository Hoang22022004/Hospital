using Hospital.Areas.Admin.Models;
using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Receptionist")] // Cho phép cả Admin và Lễ tân quản lý
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        // 1. DANH SÁCH TÀI KHOẢN
        public async Task<IActionResult> Index()
        {
            var userList = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = roles.FirstOrDefault() ?? "None",
                    IsLocked = user.LockoutEnd != null && user.LockoutEnd > DateTime.Now
                });
            }

            return View(userViewModels);
        }

        // 2. TẠO TÀI KHOẢN MỚI (GET)
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            }).ToListAsync();

            var model = new CreateUserViewModel
            {
                RoleList = roles
            };

            return View(model);
        }

        // 3. TẠO TÀI KHOẢN MỚI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // --- KIỂM TRA TRÙNG SỐ ĐIỆN THOẠI TRƯỚC KHI TẠO ---
                var phoneExists = await _userManager.Users.AnyAsync(u => u.PhoneNumber == model.PhoneNumber);
                if (phoneExists)
                {
                    ModelState.AddModelError("PhoneNumber", "Số điện thoại này đã được sử dụng cho một tài khoản khác.");
                }
                else
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FullName = model.FullName,
                        PhoneNumber = model.PhoneNumber, // Lưu SĐT vào Identity
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        // Gán vai trò
                        if (!string.IsNullOrEmpty(model.Role))
                        {
                            await _userManager.AddToRoleAsync(user, model.Role);
                        }

                        // --- TỰ ĐỘNG LIÊN KẾT VỚI BỆNH NHÂN CŨ ---
                        var patient = await _db.BenhNhan.FirstOrDefaultAsync(b =>
                            b.SoDienThoai == model.PhoneNumber || b.Email == model.Email);

                        if (patient != null)
                        {
                            patient.IdentityUserId = user.Id;
                            if (string.IsNullOrEmpty(patient.Email))
                            {
                                patient.Email = model.Email;
                            }
                            _db.BenhNhan.Update(patient);
                            await _db.SaveChangesAsync();
                        }

                        TempData["success"] = "Đã tạo tài khoản và đồng bộ hồ sơ cho " + model.FullName;
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // Load lại danh sách Role nếu có lỗi
            model.RoleList = await _roleManager.Roles.Select(x => new SelectListItem { Text = x.Name, Value = x.Name }).ToListAsync();
            return View(model);
        }

        // 4. SỬA TÀI KHOẢN (GET)
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = userRoles.FirstOrDefault(),
                RoleList = await _roleManager.Roles.Select(x => new SelectListItem { Text = x.Name, Value = x.Name }).ToListAsync()
            };

            return View(model);
        }

        // 5. SỬA TÀI KHOẢN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                // --- KIỂM TRA TRÙNG SỐ ĐIỆN THOẠI (Tránh trùng với người khác nhưng cho phép giữ số cũ của chính mình) ---
                var phoneExists = await _userManager.Users.AnyAsync(u => u.PhoneNumber == model.PhoneNumber && u.Id != model.Id);
                if (phoneExists)
                {
                    ModelState.AddModelError("PhoneNumber", "Số điện thoại này đã thuộc về một tài khoản khác.");
                }
                else
                {
                    user.FullName = model.FullName;
                    user.PhoneNumber = model.PhoneNumber;

                    // Cập nhật vai trò
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var currentRole = userRoles.FirstOrDefault();

                    if (currentRole != model.Role)
                    {
                        if (!string.IsNullOrEmpty(currentRole)) await _userManager.RemoveFromRoleAsync(user, currentRole);
                        if (!string.IsNullOrEmpty(model.Role)) await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    // Đổi mật khẩu nếu có
                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        await _userManager.RemovePasswordAsync(user);
                        var addPassResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
                        if (!addPassResult.Succeeded)
                        {
                            foreach (var error in addPassResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
                            model.RoleList = await _roleManager.Roles.Select(x => new SelectListItem { Text = x.Name, Value = x.Name }).ToListAsync();
                            return View(model);
                        }
                    }

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["success"] = "Cập nhật tài khoản thành công!";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.RoleList = await _roleManager.Roles.Select(x => new SelectListItem { Text = x.Name, Value = x.Name }).ToListAsync();
            return View(model);
        }

        // 6. KHÓA / MỞ KHÓA TÀI KHOẢN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUnlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                user.LockoutEnd = DateTime.Now;
                TempData["success"] = "Đã mở khóa tài khoản.";
            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddYears(100);
                TempData["success"] = "Đã khóa tài khoản.";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}