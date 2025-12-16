using Hospital.Areas.Admin.Models;
using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using static Hospital.Areas.Admin.Models.CreateUserViewModel;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")] // Bỏ comment sau khi có tài khoản Admin
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

        // 1. Danh sách Tài khoản
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
                    IsLocked = user.LockoutEnd != null && user.LockoutEnd > DateTime.Now,
                    LockoutEnd = user.LockoutEnd // Hiển thị ngày mở khóa
                });
            }

            return View(userViewModels);
        }

        // 2. Tạo Tài khoản mới (GET)
        public IActionResult Create()
        {
            var roles = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            {
                Text = i,
                Value = i
            });

            var model = new CreateUserViewModel
            {
                RoleList = roles
            };

            return View(model);
        }

        // 3. Tạo Tài khoản mới (POST)
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
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }
                    TempData["success"] = "Tạo tài khoản thành công!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            {
                Text = i,
                Value = i
            });

            return View(model);
        }

        // ---------------------------------------------------------
        // 4. SỬA TÀI KHOẢN (EDIT) - ĐÃ BỔ SUNG
        // ---------------------------------------------------------

        // GET: Hiển thị form sửa
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Lấy Role hiện tại
            var userRoles = await _userManager.GetRolesAsync(user);
            var currentRole = userRoles.FirstOrDefault();

            // Lấy danh sách Role cho Dropdown
            var roles = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            {
                Text = i,
                Value = i
            });

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = currentRole,
                RoleList = roles
            };

            return View(model);
        }

        // POST: Xử lý cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                // 1. Cập nhật Họ tên
                user.FullName = model.FullName;

                // 2. Cập nhật Role
                var userRoles = await _userManager.GetRolesAsync(user);
                var currentRole = userRoles.FirstOrDefault();

                if (currentRole != model.Role)
                {
                    // Xóa Role cũ nếu có
                    if (!string.IsNullOrEmpty(currentRole))
                    {
                        await _userManager.RemoveFromRoleAsync(user, currentRole);
                    }
                    // Thêm Role mới
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }
                }

                // 3. Xử lý Reset Mật khẩu (Chỉ khi Admin nhập mật khẩu mới)
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    var removePassResult = await _userManager.RemovePasswordAsync(user);
                    if (removePassResult.Succeeded)
                    {
                        await _userManager.AddPasswordAsync(user, model.NewPassword);
                    }
                }

                // Lưu thay đổi vào DB
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

            // Load lại RoleList nếu lỗi
            model.RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            {
                Text = i,
                Value = i
            });

            return View(model);
        }

        // ---------------------------------------------------------

        // 5. Khóa / Mở khóa tài khoản
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