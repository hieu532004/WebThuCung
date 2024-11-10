using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebThuCung.Models
{
    [Table("DetailOrder")]
    public class DetailOrder
    {
        [Key] // Đặt một khóa chính cho bảng này
        public int IdDetailOrder { get; set; }
        [ForeignKey("Order")]
        public string idOrder { get; set; }

        [ForeignKey("Product")]
        public string idProduct { get; set; }

        [Required]
        public int Quantity { get; set; }
        public string nameSize { get; set; }
        public string nameColor { get; set; }
        public decimal? totalPrice { get; set; }

        // Navigation Properties
        public Order Order { get; set; }
        public Product Product { get; set; }
        public decimal CalculateTotalPrice()
        {
            // Kiểm tra xem Product có khác null không trước khi truy cập Price
            if (Product != null)
            {
                return Quantity * Product.sellPrice; // Sử dụng giá từ Product
            }

            return 0; // Nếu Product là null, trả về 0
        }
    }

}
