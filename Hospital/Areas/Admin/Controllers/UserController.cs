using Hospital.Areas.Admin.Models;
using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")] // Mở khóa dòng này khi Huy đã có tài khoản Admin để bảo mật
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
            // Lấy danh sách Role động từ SQL (Bao gồm Admin, Doctor, Receptionist...)
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
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Gán vai trò đã chọn cho tài khoản mới
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    TempData["success"] = "Đã cấp quyền truy cập cho " + model.FullName;
                    return RedirectToAction(nameof(Index));
                }

                // Xử lý lỗi từ Identity (Ví dụ: Password yếu, Email đã tồn tại)
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Load lại RoleList nếu có lỗi để Dropdown không bị trống
            model.RoleList = await _roleManager.Roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            }).ToListAsync();

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
                Role = userRoles.FirstOrDefault(),
                RoleList = await _roleManager.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }).ToListAsync()
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

                user.FullName = model.FullName;

                // 1. CẬP NHẬT VAI TRÒ (ROLE)
                var userRoles = await _userManager.GetRolesAsync(user);
                var currentRole = userRoles.FirstOrDefault();

                if (currentRole != model.Role)
                {
                    if (!string.IsNullOrEmpty(currentRole))
                    {
                        await _userManager.RemoveFromRoleAsync(user, currentRole);
                    }
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }
                }

                // 2. XỬ LÝ ĐỔI MẬT KHẨU (RESET PASSWORD)
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    // Bước A: Xóa mật khẩu cũ
                    var removePassResult = await _userManager.RemovePasswordAsync(user);
                    if (removePassResult.Succeeded)
                    {
                        // Bước B: Thêm mật khẩu mới
                        var addPassResult = await _userManager.AddPasswordAsync(user, model.NewPassword);

                        // QUAN TRỌNG: Nếu mật khẩu mới không thỏa mãn (ví dụ quá ngắn, thiếu ký tự...), phải báo lỗi
                        if (!addPassResult.Succeeded)
                        {
                            foreach (var error in addPassResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, "Lỗi mật khẩu: " + error.Description);
                            }
                            // Load lại danh sách Role để trả về View nếu có lỗi
                            model.RoleList = await _roleManager.Roles.Select(x => new SelectListItem
                            {
                                Text = x.Name,
                                Value = x.Name
                            }).ToListAsync();
                            return View(model);
                        }
                    }
                }

                // 3. LƯU THAY ĐỔI TỔNG THỂ
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["success"] = "Cập nhật tài khoản thành công!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Nếu có lỗi ModelState, phải load lại danh sách Role trước khi return View
            model.RoleList = await _roleManager.Roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            }).ToListAsync();

            return View(model);
        }

        // 6. KHÓA / MỞ KHÓA TÀI KHOẢN
        [HttpPost]
        public async Task<IActionResult> LockUnlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                // Đang khóa -> Mở khóa
                user.LockoutEnd = DateTime.Now;
                TempData["success"] = "Đã mở khóa tài khoản.";
            }
            else
            {
                // Đang hoạt động -> Khóa 100 năm
                user.LockoutEnd = DateTime.Now.AddYears(100);
                TempData["success"] = "Đã khóa quyền truy cập của tài khoản này.";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}