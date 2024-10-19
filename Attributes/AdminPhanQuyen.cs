using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebThuCung.Data;

namespace WebThuCung.Attributes
{
    public class AdminPhanQuyen : AuthorizeAttribute
    {
        private readonly PetContext _context;

        public AdminPhanQuyen(PetContext context)
        {
            _context = context; // Sử dụng Dependency Injection để cung cấp ngữ cảnh
        }

        public int MACN { get; set; }
        public string MACHUCNANG { get; set; }

        //public override void OnAuthorization(AuthorizationContext context)
        //{
        //    // Lấy thông tin quản trị viên từ session
        //    var quantri = context.HttpContext.Session["Taikhoanadmin"] as ADMIN;

        //    if (quantri != null)
        //    {
        //        // Kiểm tra quyền truy cập
        //        bool hasPermission = _context.PhanQuyens
        //            .Any(m => m.MAADMIN == quantri.MAADMIN && m.MACHUCNANG == MACHUCNANG);

        //        if (!hasPermission)
        //        {
        //            // Người dùng không có quyền
        //            RedirectToNoPermission(context);
        //        }
        //    }
        //    else
        //    {
        //        // Chưa đăng nhập
        //        RedirectToLogin(context);
        //    }
        //}

        //private void RedirectToNoPermission(AuthorizationContext context)
        //{
        //    var returnUrl = context.HttpContext.Request.RawUrl;
        //    context.Result = new RedirectToRouteResult(new RouteValueDictionary
        //{
        //    { "controller", "BaoLoi" },
        //    { "action", "KhongCoQuyen" },
        //    { "area", "Admin" },
        //    { "returnUrl", returnUrl }
        //});
        //}

        //private void RedirectToLogin(AuthorizationContext context)
        //{
        //    var returnUrl = context.HttpContext.Request.RawUrl;
        //    context.Result = new RedirectToRouteResult(new RouteValueDictionary
        //{
        //    { "controller", "Admin" },
        //    { "action", "Index" },
        //    { "area", "Admin" },
        //    { "returnUrl", returnUrl }
        //});
        //}
    }
}
