using BanDoGiaDung.Models.Cart; // Để dùng CartItem
using BanDoGiaDung.Services;    // Để dùng MomoService
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BanDoGiaDung.Controllers
{
    public class PaymentController : Controller
    {
        private readonly MomoService momo = new MomoService();

        // 1. TẠO GIAO DỊCH
        public async Task<ActionResult> Create()
        {
            long amount = GetOrderAmount();
            if (amount == 0)
            {
                return Content("Giỏ hàng trống!");
            }

            string orderId = DateTime.Now.Ticks.ToString(); // Tạo mã đơn hàng ngẫu nhiên
            string redirectUrl = Url.Action("ReturnUrl", "Payment", null, Request.Url.Scheme);
            string ipnUrl = "https://momo.vn";
            try
            {
                string payUrl = await momo.CreatePaymentAsync(amount, orderId, redirectUrl);
                return Redirect(payUrl); // Chuyển hướng sang MoMo
            }
            catch (Exception ex)
            {
                return Content("Lỗi tạo thanh toán: " + ex.Message);
            }
        }

        // 2. KẾT QUẢ TRẢ VỀ (ReturnUrl)
        public ActionResult ReturnUrl()
        {
            string resultCode = Request.QueryString["resultCode"];
            string orderId = Request.QueryString["orderId"];
            string amount = Request.QueryString["amount"];

            if (resultCode == "0")
            {
                // THANH TOÁN THÀNH CÔNG
                ViewBag.Message = "Giao dịch thành công!";
                ViewBag.OrderId = orderId;
                ViewBag.Amount = string.Format("{0:N0} ₫", long.Parse(amount));

                // Xóa giỏ hàng sau khi thanh toán xong
                Session["Cart"] = null;
            }
            else
            {
                // THANH TOÁN THẤT BẠI
                ViewBag.Message = "Giao dịch thất bại (Hoặc bạn đã hủy).";
            }

            return View();
        }

        // Hàm lấy tổng tiền từ Session Giỏ Hàng
        private long GetOrderAmount()
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart != null && cart.Count > 0)
            {
                return (long)cart.Sum(x => x.TotalPrice);
            }
            return 0;
        }
    }
}