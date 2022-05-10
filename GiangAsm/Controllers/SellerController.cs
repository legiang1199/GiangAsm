using GiangAsm.Areas.Identity.Data;
using GiangAsm.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BookShop.Controllers
{
    public class SellerController : Controller
    {
        private readonly ILogger<SellerController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private UserContext _context;
        private readonly int _recordsPerPage = 5;

        public SellerController(ILogger<SellerController> logger, UserManager<AppUser> userManager, UserContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        [Authorize(Roles = "Seller")]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Book");
        }
        [Authorize(Roles = "Seller")]

        public IActionResult Register()
        {
            return View("Views/Store/Register.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult addStore([Bind("UserId, Address, Slogan, Name")] Store store)
        {
            _context.Store.Add(store);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Seller")]
        public IActionResult ForSellerOnly()
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            Console.WriteLine(thisUserId);
            Store User = _context.Store.FirstOrDefault(s => s.UserId == thisUserId);
            /* ViewBag.message = "This is for Customer only! Hi " + _userManager.GetUserName(HttpContext.User);*/
            Console.WriteLine("Store");
            if (User == null)
            {

                return View("Views/Store/Register.cshtml");
            }
            else
            {
                return View("Views/Book/Index.cshtml");
            }

        }
        [Authorize(Roles = "Seller")]

        public async Task<IActionResult> Profile(int id = 0)
        {
            var userid = _userManager.GetUserId(HttpContext.User);

            var ordered = from b in _context.Order select b;

            ordered = ordered.Include(u => u.User).Include(r => r.OrderDetails).ThenInclude(d => d.Book).ThenInclude(s => s.Store)
                .Where(f => f.OrderDetails.Where(w => w.Book.Store.UserId == userid).Any());
            List<Order> ordersList = await ordered.Skip(id * _recordsPerPage)
                .Take(_recordsPerPage).ToListAsync();

            return RedirectToAction("Index", "Orders", ordersList);
        }
    }
}