using System.ComponentModel.DataAnnotations;

namespace WebThuCung.Dto
{
    public class RegisterDto
    {
        [Display(Name = "Tên đăng nhập:")]
        [Required(ErrorMessage = "Tên đang nhập không được để trống.")]
        public string userCustomer { get; set; } // Viết hoa chữ cái đầu

        [Display(Name = "Mật khẩu:")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Độ dài mật khẩu ít nhất 6 - 20 ký tự.")]
        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public string paswordCusstomer { get; set; } // Viết hoa chữ cái đầu

        [Display(Name = "Xác nhận mật khẩu:")]
        [Compare("paswordCusstomer", ErrorMessage = "Xác nhận mật khẩu không đúng.")]
        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public string confirmPassword { get; set; } // Viết hoa chữ cái đầu

        [Display(Name = "Họ và tên:")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Họ tên không hợp lệ")]
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        public string Name { get; set; } // Viết hoa chữ cái đầu

        [Display(Name = "Số điện thoại:")]
        [StringLength(11, MinimumLength = 9, ErrorMessage = "Số điện thoại không hợp lệ")]
        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        public string Phone { get; set; } // Viết hoa chữ cái đầu

        [Required(ErrorMessage = "Email không được trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } // Viết hoa chữ cái đầu

        [Display(Name = "Hình ảnh:")]
        public string? Image { get; set; } // Viết hoa chữ cái đầu

        [Display(Name = "Ngày sinh:")]
        public DateTime DateBirth { get; set; } // Viết hoa chữ cái đầu

        [Display(Name = "Địa chỉ:")]
        public string? Address { get; set; } // Viết hoa chữ cái đầu
    }
}
