using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using WebThuCung.Data;
using WebThuCung.Dto;
using WebThuCung.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.WebRequestMethods;

namespace WebThuCung.Controllers
{
    public class UserController : Controller
    {
        private readonly PetContext _context;
        public UserController(PetContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            // Truy vấn lấy các sản phẩm cùng các thuộc tính liên quan sử dụng Include
            var allProducts = _context.Products
                                 .Include(sp => sp.Branch)  // Bao gồm thông tin thương hiệu
                                 .Include(sp => sp.Category)        // Bao gồm thông tin loại
                                 .Include(sp => sp.Color)      // Bao gồm thông tin màu sắc
                                 .Select(sp => new ProductViewDto
                                 {
                                     idProduct = sp.idProduct,
                                     nameProduct = sp.nameProduct,
                                     sellPrice = sp.sellPrice,
                                     Image = sp.Image,
                                     idBranch = sp.idBranch,
                                     idCategori = sp.idCategory,
                                     nameBranch = sp.Branch.namBranch,   // Lấy tên thương hiệu từ đối tượng liên quan
                                     nameCategory = sp.Category.namCategory,     // Lấy tên loại từ đối tượng liên quan
                                     Quantity = sp.Quantity,
                                     Description = sp.Description,
                                     nameColor = sp.Color.nameColor, // Lấy tên màu sắc từ đối tượng liên quan
                                     Logo = sp.Branch.Logo     // Lấy logo từ đối tượng liên quan
                                 })
                                 .OrderBy(p => p.idProduct)
                                 .Take(6)  // Lấy ra 6 sản phẩm
                                 .ToList();

            return View(allProducts);
        }
    
        #region Kiểm tra tên đăng nhập đã tồn tại
        public bool KiemTraTenDn(string tendn)
        {
            return _context.Customers.Any(x => x.userCustomer == tendn);
        }
        #endregion

        public bool KiemTraEmail(string email)
        {
            return _context.Customers.Any(x => x.Email == email);
        }
        public bool KiemTraEmailAdmin(string email)
        {
            return _context.Admins.Any(x => x.Email == email);
        }

        // Hàm sử dụng BCrypt để băm mật khẩu
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        //#region Đăng ký tài khoản người dùng
        [HttpPost]
        public IActionResult Register(RegisterDto model)
        {
            // Kiểm tra xem model có hợp lệ không
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên đăng nhập đã tồn tại chưa
                if (KiemTraTenDn(model.userCustomer))
                {
                    ModelState.AddModelError("userCustomer", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Kiểm tra xem email đã tồn tại chưa
                if (KiemTraEmail(model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(model);
                }
                if (KiemTraEmailAdmin(model.Email))
                {
                    ModelState.AddModelError("Email", "Email này là của Admin");
                    return View(model);
                }

                // Nếu không có lỗi xác thực nào, tiến hành tạo khách hàng
                if (ModelState.IsValid)
                {
                    var otp = new Random().Next(100000, 999999).ToString();
                    // Tạo đối tượng KHACHHANG và gán thông tin từ form đăng ký
                    var khachHang = new Customer
                    {
                        userCustomer = model.userCustomer,
                        nameCustomer = model.Name,
                        Email = model.Email,
                        Phone = model.Phone,
                        OtpCode = otp, // Lưu OTP trong cơ sở dữ liệu
                        OtpExpiryTime = DateTime.Now.AddMinutes(5),
                        Image = "avatar_user.png", // Có thể thay đổi nếu bạn xử lý hình ảnh upload
                    };

                    // Sử dụng BCrypt để băm mật khẩu
                    khachHang.passwordCustomer = HashPassword(model.paswordCusstomer);

                    // Thêm khách hàng mới vào cơ sở dữ liệu
                    _context.Customers.Add(khachHang);
                    _context.SaveChanges();
                    SendOtpEmail(model.Email, otp);

                    // Chuyển hướng tới trang nhập mã OTP
                    return RedirectToAction("ConfirmOtp", new { email = model.Email });
                }
            }

            // Trả về view với dữ liệu của model khi có lỗi
            return View(model);
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginDto model)
        {
            if (ModelState.IsValid)
            {
                // Tìm kiếm khách hàng theo tên đăng nhập hoặc email
                var customer = _context.Customers
                                       .SingleOrDefault(c => c.userCustomer == model.userName || c.Email == model.userName);

                // Nếu tài khoản không tồn tại
                if (customer == null)
                {
                    // Thêm thông báo lỗi vào ModelState
                    ModelState.AddModelError(string.Empty, "Tài khoản không tồn tại. Vui lòng kiểm tra lại email hoặc tên đăng nhập.");
                    return View(model); // Trả về view cùng thông báo lỗi
                }

                // Kiểm tra xem email đã được xác nhận chưa
                if (!customer.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Email của bạn chưa được xác nhận.");
                    return View(model); // Trả về view cùng thông báo lỗi
                }

                // Kiểm tra mật khẩu
                if (!BCrypt.Net.BCrypt.Verify(model.password, customer.passwordCustomer))
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu không đúng. Vui lòng thử lại.");
                    return View(model); // Trả về view cùng thông báo lỗi
                }

                // Nếu vượt qua tất cả các kiểm tra, đăng nhập thành công
                ViewBag.Thongbao = "Đăng nhập thành công";

                // Serialize thông tin khách hàng thành JSON và lưu vào Session
                var customerJson = Newtonsoft.Json.JsonConvert.SerializeObject(customer);
                HttpContext.Session.SetString("Taikhoan", customerJson);

                // Chuyển hướng về trang chính sau khi đăng nhập thành công
                return RedirectToAction("Index", "User");
            }

            // Nếu model không hợp lệ, trả về view với thông báo lỗi ModelState
            return View(model);
        }



        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Taikhoan");

            ViewBag.Thongbao = "Bạn đã đăng xuất thành công.";

            return RedirectToAction("Index", "User");
        }

        public IActionResult ConfirmOtp(string email)
        {
            var model = new ConfirmOtpDto { Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmOtp(ConfirmOtpDto model)
        {

            var customer = _context.Customers.SingleOrDefault(c => c.Email == model.Email && c.OtpCode == model.Otp);

            if (customer != null && customer.OtpExpiryTime > DateTime.Now)
            {
                // Xử lý cho customer
                customer.EmailConfirmed = true;
                customer.OtpCode = null; // Xóa OTP sau khi xác nhận thành công
                customer.OtpExpiryTime = null;
                _context.SaveChanges();

                return RedirectToAction("Login");
            }      
            else
            {
                ModelState.AddModelError("Otp", "Mã OTP không đúng hoặc đã hết hạn.");
            }

            // Nếu có lỗi, trả về view với thông báo lỗi và các giá trị nhập
            return View(model); // Gửi lại model về view
        }
        private void SendOtpEmail(string email, string otp)
        {
            using (var client = new SmtpClient("smtp.gmail.com"))
            {
                client.Port = 587; // Port cho TLS
                client.EnableSsl = true; // Bật SSL
                client.Credentials = new NetworkCredential(
                    Environment.GetEnvironmentVariable("EMAIL_USERNAME"),
                    Environment.GetEnvironmentVariable("EMAIL_PASSWORD")
                );

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(Environment.GetEnvironmentVariable("EMAIL_USERNAME")),
                    Subject = "Mã OTP Xác Nhận Địa Chỉ Email",
                    Body = $"<h1>Xác Nhận Địa Chỉ Email</h1>" +
                           $"<p>Đây là mã OTP của bạn: <strong>{otp}</strong></p>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);

                try
                {
                    client.Send(mailMessage);
                }
                catch (SmtpException ex)
                {
                    Console.WriteLine($"Error sending email: {ex.Message}");
                }
            }
        }
        public IActionResult ConfirmOtpReset(string email)
        {
            var model = new ConfirmOtpDto { Email = email };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmOtpReset(ConfirmOtpDto model)
        {
            var customer = _context.Customers.SingleOrDefault(c => c.Email == model.Email && c.OtpCode == model.Otp);
           

            if (customer != null && customer.OtpExpiryTime > DateTime.Now)
            {
                // Xử lý cho customer
                customer.OtpCode = null; // Xóa OTP sau khi xác nhận thành công
                customer.OtpExpiryTime = null; // Xóa thông tin OTP
                _context.SaveChanges();

                // Chuyển hướng đến trang nhập lại mật khẩu
                return RedirectToAction("ResetPassword", new { email = model.Email });
            }        
            else
            {
                ModelState.AddModelError("Otp", "Mã OTP không đúng hoặc đã hết hạn.");
            }

            // Nếu có lỗi, trả về view với thông báo lỗi và các giá trị nhập
            return View(model); // Gửi lại model về view
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordDto model)
        {
            if (ModelState.IsValid)
            {

                // Kiểm tra trong bảng customer
                var customer = _context.Customers.SingleOrDefault(c => c.Email == model.Email);
                if (customer != null)
                {
                    // Tạo mã OTP ngẫu nhiên
                    var otp = new Random().Next(100000, 999999).ToString();
                    customer.OtpCode = otp; // Gán mã OTP
                    customer.OtpExpiryTime = DateTime.Now.AddMinutes(10); // Đặt thời gian hết hạn
                    _context.SaveChanges();

                    // Gửi email chứa mã OTP
                    SendOtpEmail(model.Email, otp);
                    return RedirectToAction("ConfirmOtpReset", new { email = model.Email });
                }            

                // Nếu không tìm thấy người dùng, thêm thông báo lỗi
                ModelState.AddModelError(string.Empty, "Email không tồn tại.");
            }

            // Trả lại view với thông báo lỗi
            return View(model);
        }


        public IActionResult ResetPassword(string email)
        {
            var model = new ResetPasswordDto { Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordDto model)
        {
            if (ModelState.IsValid)
            {
                // Tìm người dùng theo email
                var customer = _context.Customers.SingleOrDefault(c => c.Email == model.Email);

                if (customer != null)
                {
                    // Cập nhật mật khẩu cho customer
                    customer.passwordCustomer = HashPassword(model.Password); // Băm mật khẩu
                    _context.SaveChanges();
                    return RedirectToAction("Login");
                }
                else
                {
                    // Nếu không tìm thấy người dùng, có thể gửi thông báo thành công giả để bảo mật
                    return RedirectToAction("ResetPasswordConfirmation");
                }
            }
            return View(model);
        }
    }
}
