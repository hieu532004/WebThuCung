using WebThuCung.Models;

namespace WebThuCung.Dto
{
    public class OrderViewDto
    {
        public Order Order { get; set; }
        public Transaction Transaction { get; set; }
        public bool PaymentExists { get; set; }
        public decimal? totalOrder { get; set; }

    }
}
