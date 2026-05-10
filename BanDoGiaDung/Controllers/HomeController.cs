using BanDoGiaDung.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BanDoGiaDung.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        private readonly GiaDungDbContext db = new GiaDungDbContext();
        public ActionResult Index()
        {
            ViewBag.Genres = db.Genres.OrderBy(g => g.genre_name).ToList();
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult Chat()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> SendMessageToGemini(string userMessage)
        {
            string apiKey = "YOUR_GEMINI_API_KEY";
            string modelName = "gemini-2.5-flash";
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";
            var products = db.Products
                             .Where(p => p.status == "1")
                             .Select(p => new { p.product_name, p.price })
                             .Take(100)
                             .ToList();
            var formattedList = products.Select(p => $"{p.product_name} - Giá: {p.price:N0} đ");
            string productContext = string.Join("; ", formattedList);
            string systemPrompt = $@"
                Bạn là nhân viên tư vấn của cửa hàng Elmich.
                Dưới đây là DANH SÁCH SẢN PHẨM VÀ GIÁ BÁN hiện tại của shop: [{productContext}]
                YÊU CẦU TRẢ LỜI:
                1. Nếu khách hỏi về sản phẩm CÓ trong danh sách: Hãy báo giá chính xác như trong dữ liệu trên và mời khách mua.
                2. Nếu khách hỏi giá: Tuyệt đối chỉ lấy giá từ danh sách trên, không được tự bịa giá.
                3. Nếu khách hỏi sản phẩm KHÔNG có: Báo là shop chưa kinh doanh.
                4. Trả lời ngắn gọn, lịch sự, dùng kính ngữ.
                Câu hỏi của khách: ""{userMessage}""
            ";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Asp.Net-Mvc-App");

                var requestData = new
                {
                    contents = new[] { new { parts = new[] { new { text = systemPrompt }}
                }
            }
                };
                var jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                try
                {
                    var response = await client.PostAsync(apiUrl, content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);

                    if (jsonResponse.error != null)
                    {
                        return Json(new { success = false, reply = "Lỗi API: " + jsonResponse.error.message });
                    }

                    if (jsonResponse.candidates != null && jsonResponse.candidates.Count > 0)
                    {
                        string aiResponse = jsonResponse.candidates[0].content.parts[0].text;
                        return Json(new { success = true, reply = aiResponse });
                    }

                    return Json(new { success = false, reply = "AI không phản hồi." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, reply = "Lỗi hệ thống: " + ex.Message });
                }
            }
        }
        public ActionResult Banner()
        {
            return View();
        }

        public ActionResult Benefits()
        {
            return View();
        }

        public ActionResult NewProducts()
        {
            var list = db.Products
                 .OrderByDescending(x => x.create_at)
                 .Take(10)
                 .ToList();

            return PartialView(list);
        }

        public ActionResult Categories()
        {
            return PartialView();
        }


        public ActionResult BestSeller()
        {
            var list = db.Products
                         .OrderByDescending(p => p.buyturn)
                         .Take(10)
                         .ToList();

            return PartialView(list);
        }

    }
}