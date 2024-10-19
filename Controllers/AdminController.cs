using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebThuCung.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WebThuCung.Models;
using WebThuCung.Dto;

namespace WebThuCung.Controllers
{
    public class AdminController : Controller
    {
        private readonly PetContext _context; // Biến để truy cập cơ sở dữ liệu

        public AdminController(PetContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var sessionAdmin = HttpContext.Session.GetString("Taikhoanadmin");

            if (string.IsNullOrEmpty(sessionAdmin))
            {
                return RedirectToAction("Login", "Admin");
            }

            return View();
        }

        // GET: Hiển thị trang đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Xử lý đăng nhập
        [HttpPost]
        public IActionResult Login(LoginDto model)
        {
            if (ModelState.IsValid)
            {
                // Tìm kiếm admin trong cơ sở dữ liệu
                Admin ad = _context.Admins.SingleOrDefault(n => n.userAdmin == model.userName && n.passwordAdmin == model.password);

                if (ad != null)
                {
                    ViewBag.Thongbao = "Đăng nhập thành công";

                    // Lưu thông tin admin vào session dưới dạng chuỗi JSON
                    HttpContext.Session.SetString("Taikhoanadmin", JsonConvert.SerializeObject(ad));

                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ViewBag.Thongbao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }

            return View(model);
        }
    }
}
