using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using WebThuCung.Data;
using WebThuCung.Dto;
using WebThuCung.Models;

namespace WebThuCung.Controllers
{
    public class OrderController : Controller
    {
        private readonly PetContext _context; // Biến để truy cập cơ sở dữ liệu

        public OrderController(PetContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            // Lấy danh sách các đơn hàng từ cơ sở dữ liệu
            var orders = _context.Orders
                .Include(o => o.DetailOrders)
                .ThenInclude(p => p.Product)
                .ToList();

            // Tính toán tổng giá trị cho mỗi đơn hàng
            foreach (var order in orders)
            {
                order.CalculateTotalOrder(); // Calculate total order based on DetailOrders
            }

            // Truyền danh sách đơn hàng sang view
            return View(orders);
        }
        public IActionResult Accept()
        {
            // Lấy danh sách các đơn hàng từ cơ sở dữ liệu
            var orders = _context.Orders.Include(p => p.Customer).Include(o => o.DetailOrders)
                .ThenInclude(p => p.Product).ToList(); // Lấy tất cả các Order mà không cần Include Transactions

            // Tạo một danh sách để chứa các order kèm thông tin về transactions và payments
            var orderViewModels = new List<OrderViewDto>();

            foreach (var order in orders)
            {
                // Tính toán tổng giá trị cho mỗi đơn hàng
                order.CalculateTotalOrder(); // Tính tổng giá trị đơn hàng dựa trên DetailOrders

                // Tìm các Transaction liên quan đến order
                var transactions = _context.Transactions
                    .Where(t => t.idOrder == order.idOrder)
                    .ToList();

                // Kiểm tra nếu Transaction nào nằm trong Payment
                foreach (var transaction in transactions)
                {
                    var paymentExists = _context.Payments.Any(p => p.idTransaction == transaction.idTransaction);
                    orderViewModels.Add(new OrderViewDto
                    {
                        Order = order,
                        Transaction = transaction,
                        PaymentExists = paymentExists,
                        totalOrder = order.totalOrder // Thêm tổng giá trị đơn hàng vào model
                    });
                }
            }

            // Truyền danh sách orderViewModels sang view
            return View(orderViewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Accept(string idOrder)
        {
            // Kiểm tra xem order có tồn tại hay không
            var order = _context.Orders.Include(o => o.Customer).FirstOrDefault(o => o.idOrder == idOrder);
            if (order == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái của order và payment
            order.statusOrder = OrderStatus.Accepted; // Cập nhật trạng thái đơn hàng thành "Accept"
            order.statusPay = PaymentStatus.Paid; // Cập nhật trạng thái thanh toán thành "Paid"
            TempData["success"] = "Đã xác nhận thanh toán";
            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();

            // Gửi email xác nhận thanh toán
            SendOrderConfirmationEmail(order.Customer.Email, order.idOrder);

            // Chuyển hướng về danh sách đơn hàng
            return RedirectToAction("Accept");
        }
        private void SendOrderConfirmationEmail(string email, string orderId)
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
                    Subject = "Xác Nhận Thanh Toán Đơn Hàng",
                    Body = $"<h1>Xác Nhận Thanh Toán Đơn Hàng</h1>" +
                           $"<p>Đơn hàng của bạn với ID: <strong>{orderId}</strong> đã được xác nhận thanh toán.</p>",
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



        [Authorize(Roles = "Admin,StaffOrder")]
        // GET: Order/Edit/{id}
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var order = _context.Orders
                                .Include(o => o.Customer) // Kết hợp thông tin khách hàng
                                .Include(o => o.DetailOrders) // Kết hợp thông tin chi tiết đơn hàng
                                .FirstOrDefault(o => o.idOrder == id);

            if (order == null)
            {
                return NotFound();
            }

            var orderDto = new OrderDto
            {
                idOrder = order.idOrder,
                idCustomer = order.idCustomer,
                dateTo = order.dateTo,
                statusOrder = order.statusOrder,
                statusPay = order.statusPay,
                totalOrder = order.totalOrder
            };

            // Truyền danh sách khách hàng vào ViewBag
            ViewBag.Customers = _context.Customers.Select(c => new SelectListItem
            {
                Value = c.idCustomer.ToString(),
                Text = c.nameCustomer // Giả sử bạn có thuộc tính nameCustomer trong Customer
            }).ToList();

            return View(orderDto);
        }

        // POST: Order/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(OrderDto orderDto)
        {
            ViewBag.Customers = _context.Customers.Select(c => new SelectListItem
            {
                Value = c.idCustomer.ToString(),
                Text = c.nameCustomer
            }).ToList();

            if (ModelState.IsValid)
            {
                var order = _context.Orders.FirstOrDefault(o => o.idOrder == orderDto.idOrder);
                if (order == null)
                {
                    return NotFound();
                }

                // Cập nhật các giá trị từ DTO vào model
                order.idCustomer = orderDto.idCustomer;
                order.dateTo = orderDto.dateTo;
                order.statusOrder = orderDto.statusOrder;
                order.statusPay = orderDto.statusPay;

                // Tính toán tổng đơn hàng nếu có thay đổi trong chi tiết đơn hàng
                order.CalculateTotalOrder(); // Gọi phương thức để tính toán tổng giá trị của đơn hàng

                _context.SaveChanges();
                return RedirectToAction("Index"); // Quay lại trang danh sách đơn hàng sau khi cập nhật
            }

            return View(orderDto);
        }
        [Authorize(Roles = "Admin,StaffOrder")]
        // POST: Order/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Tìm đơn hàng và các giao dịch liên quan
            var order = _context.Orders
                .Include(o => o.Transactions) // Bao gồm các giao dịch liên quan
                .FirstOrDefault(o => o.idOrder == id);

            if (order == null)
            {
                return NotFound();
            }

            // Xóa tất cả giao dịch liên quan trước khi xóa đơn hàng
            foreach (var transaction in order.Transactions)
            {
                _context.Transactions.Remove(transaction);
            }

            // Xóa đơn hàng
            _context.Orders.Remove(order);
            _context.SaveChanges();

            return RedirectToAction("Index"); // Quay lại danh sách đơn hàng sau khi xóa
        }

        public IActionResult Detail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Lấy danh sách các DetailOrders cho một đơn hàng cụ thể và bao gồm thông tin sản phẩm liên quan
            var detailOrders = _context.DetailOrders
                .Include(d => d.Product)
                .Where(d => d.idOrder == id)
                .ToList();

            // Tính tổng giá cho mỗi DetailOrder
            foreach (var detailOrder in detailOrders)
            {
                detailOrder.totalPrice = detailOrder.CalculateTotalPrice();
            }

            // Truyền orderId vào view để liên kết trở lại đơn hàng
            ViewBag.OrderId = id;

            return View(detailOrders); // Trả về view detail cho DetailOrders
        }
        [HttpGet]
        public IActionResult CreateDetail(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return NotFound();
            }

            // Truyền orderId sang view
            ViewBag.OrderId = Id;

            // Truyền danh sách sản phẩm để chọn lựa
            ViewBag.Products = _context.Products.Select(p => new SelectListItem
            {
                Value = p.idProduct.ToString(),
                Text = p.nameProduct
            }).ToList();
            ViewBag.Sizes = _context.Sizes.Select(s => new SelectListItem
            {
                Value = s.nameSize, // Sử dụng nameSize làm giá trị
                Text = s.nameSize
            }).ToList();

            ViewBag.Colors = _context.Colors.Select(c => new SelectListItem
            {
                Value = c.nameColor, // Sử dụng nameColor làm giá trị
                Text = c.nameColor
            }).ToList();
            var model = new DetailOrderDto
            {
                idOrder = Id  // Hoặc gán giá trị hợp lệ cho idOrder
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateDetail(DetailOrderDto detailOrderDto)
        {
            ViewBag.Products = _context.Products.Select(p => new SelectListItem
            {
                Value = p.idProduct.ToString(),
                Text = p.nameProduct
            }).ToList();
            ViewBag.Sizes = _context.Sizes.Select(s => new SelectListItem
            {
                Value = s.nameSize, // Sử dụng nameSize làm giá trị
                Text = s.nameSize
            }).ToList();

            ViewBag.Colors = _context.Colors.Select(c => new SelectListItem
            {
                Value = c.nameColor, // Sử dụng nameColor làm giá trị
                Text = c.nameColor
            }).ToList();
            if (ModelState.IsValid)
            {
                // Kiểm tra xem sản phẩm đã tồn tại trong đơn hàng hay chưa
                var existingDetailOrder = _context.DetailOrders
                    .Include(d => d.Product)
                    .FirstOrDefault(d => d.idOrder == detailOrderDto.idOrder && d.idProduct == detailOrderDto.idProduct);

                if (existingDetailOrder != null)
                {
                    // Nếu sản phẩm đã tồn tại, thêm lỗi vào ModelState và trả về form
                    ModelState.AddModelError("idProduct", "Sản phẩm đã tồn tại trong đơn hàng này.");
                }
                else
                {
                    var product = _context.Products.FirstOrDefault(p => p.idProduct == detailOrderDto.idProduct);
                    // Chuyển đổi từ DTO sang model DetailOrder
                    var detailOrder = new DetailOrder
                    {
                        idOrder = detailOrderDto.idOrder,
                        idProduct = detailOrderDto.idProduct,
                        nameColor = detailOrderDto.nameColor,    
                        nameSize = detailOrderDto.nameSize,
                        Quantity = detailOrderDto.Quantity,
                        totalPrice = detailOrderDto.Quantity * product.sellPrice // Tính tổng giá
                    };

                    _context.DetailOrders.Add(detailOrder);
                    _context.SaveChanges();

                    // Chuyển hướng về danh sách DetailOrder của đơn hàng cụ thể
                    return RedirectToAction("Detail", new { id = detailOrder.idOrder });
                }
            }

            // Nếu không hợp lệ, tải lại form
            ViewBag.OrderId = detailOrderDto.idOrder;
        

            return View(detailOrderDto);
        }



        // GET: DetailOrder/Edit/{orderId}/{productId}
        [HttpGet]
        public IActionResult EditDetail(int id)
        {
            // Tìm chi tiết đơn hàng theo idDetailOrder
            var detailOrder = _context.DetailOrders
                .FirstOrDefault(d => d.IdDetailOrder == id);

            if (detailOrder == null)
            {
                return NotFound();
            }

            // Chuyển đổi dữ liệu từ model sang DTO
            var detailOrderDto = new DetailOrderDto
            {
                idDetailOrder = detailOrder.IdDetailOrder,
                idOrder = detailOrder.idOrder,
                idProduct = detailOrder.idProduct,
                nameColor = detailOrder.nameColor,
                nameSize = detailOrder.nameSize,
                Quantity = detailOrder.Quantity
            };

            // Truyền dữ liệu danh sách sản phẩm, màu sắc và kích thước vào ViewBag
            ViewBag.Products = _context.Products.Select(p => new SelectListItem
            {
                Value = p.idProduct.ToString(),
                Text = p.nameProduct
            }).ToList();

            ViewBag.Sizes = _context.Sizes.Select(s => new SelectListItem
            {
                Value = s.nameSize,
                Text = s.nameSize
            }).ToList();

            ViewBag.Colors = _context.Colors.Select(c => new SelectListItem
            {
                Value = c.nameColor,
                Text = c.nameColor
            }).ToList();

            return View(detailOrderDto); // Truyền DTO vào view
        }


        // POST: DetailOrder/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditDetail(DetailOrderDto detailOrderDto)
        {
            if (ModelState.IsValid)
            {
                // Sử dụng idDetailOrder thay vì idOrder để tìm chi tiết đơn hàng
                var detailOrder = _context.DetailOrders
                    .FirstOrDefault(d => d.IdDetailOrder == detailOrderDto.idDetailOrder);

                if (detailOrder == null)
                {
                    return NotFound();
                }

                // Tìm sản phẩm theo idProduct để tính tổng giá
                var product = _context.Products.FirstOrDefault(p => p.idProduct == detailOrderDto.idProduct);

                if (product == null)
                {
                    ModelState.AddModelError("", "Sản phẩm không tồn tại.");
                    return View(detailOrderDto);
                }

                // Cập nhật các thuộc tính của detailOrder từ detailOrderDto
                detailOrder.nameColor = detailOrderDto.nameColor;
                detailOrder.idProduct = detailOrderDto.idProduct;
                detailOrder.nameSize = detailOrderDto.nameSize;
                detailOrder.Quantity = detailOrderDto.Quantity;
                detailOrder.totalPrice = detailOrderDto.Quantity * product.sellPrice;

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.SaveChanges();

                // Điều hướng về trang chi tiết đơn hàng
                return RedirectToAction("Detail", new { id = detailOrder.idOrder });
            }

            // Nếu ModelState không hợp lệ, tải lại danh sách Products, Sizes và Colors vào ViewBag
            ViewBag.Products = _context.Products.Select(p => new SelectListItem
            {
                Value = p.idProduct.ToString(),
                Text = p.nameProduct
            }).ToList();

            ViewBag.Sizes = _context.Sizes.Select(s => new SelectListItem
            {
                Value = s.nameSize,
                Text = s.nameSize
            }).ToList();

            ViewBag.Colors = _context.Colors.Select(c => new SelectListItem
            {
                Value = c.nameColor,
                Text = c.nameColor
            }).ToList();

            // Trả về view với dữ liệu hiện tại để người dùng chỉnh sửa
            return View(detailOrderDto);
        }

        // POST: DetailOrder/Delete/{orderId}/{productId}
        // POST: DetailOrder/Delete/{idDetailOrder}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteDetail(int id)
        {
            // Kiểm tra xem idDetailOrder có hợp lệ không
            if (id == null)
            {
                return BadRequest("Detail Order ID is missing.");
            }

            // Lấy chi tiết đơn hàng cần xóa dựa trên idDetailOrder
            var detailOrder = _context.DetailOrders
                .FirstOrDefault(d => d.IdDetailOrder == id); // Sử dụng IdDetailOrder

            if (detailOrder == null)
            {
                return NotFound("Detail order not found.");
            }

            // Xóa chi tiết đơn hàng
            _context.DetailOrders.Remove(detailOrder);
            _context.SaveChanges();

            // Thông báo thành công (nếu cần thiết)
            TempData["SuccessMessage"] = "Detail order deleted successfully.";

            // Chuyển hướng về trang chi tiết đơn hàng, có thể truyền orderId nếu cần
            return RedirectToAction("Detail", new { Id = detailOrder.idOrder });
        }


    }
}
