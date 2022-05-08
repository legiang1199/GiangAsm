using GiangAsm.Areas.Identity.Data;
using GiangAsm.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GiangAsm.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<AppUser> _userManager;
        private UserContext _context;

        private readonly int maxofpage = 10;

        private readonly int rowsonepage = 4;
        public HomeController(ILogger<HomeController> logger, IEmailSender emailSender, UserManager<AppUser> userManager, UserContext context)
        {
            _logger = logger;
            _emailSender = emailSender;
            _userManager = userManager;

            _context = context;
        }
        public async Task<IActionResult> Index(int id = 0)
        {
            
            var books = from s in _context.Book
                        select s;
           
            int numOfFilteredBook = books.Count();
            ViewBag.NumberOfPages = (int)Math.Ceiling((double)numOfFilteredBook / rowsonepage);
            ViewBag.CurrentPage = id;
            List<Book> booklist = await books.Skip(id * rowsonepage)
                .Take(rowsonepage).ToListAsync();
            if (id > 0)
            {
                ViewBag.idpagprev = id - 1;
            }
            ViewBag.idpagenext = id + 1;
            ViewBag.currentPage = id;
            return View(booklist);


        }

        public async Task<IActionResult> Privacy()
        {
            await _emailSender.SendEmailAsync("legiang1199@gmail.com", "test send mail", "just test");
            return View();
        }

        [Authorize(Roles = "Customer")]
        public IActionResult ForCustomerOnly()
        {
            ViewBag.message = "This is for Customer only! Hi " + _userManager.GetUserName(HttpContext.User);
            return View("Views/Home/Index.cshtml");
        }

        [Authorize(Roles = "Seller")]
        public IActionResult ForSellerOnly()
        {
            ViewBag.message = "This is for Store Owner only!";
            return View("Views/Home/Index.cshtml");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> SearchBook(int id = 0, string searchString = "")
        {
            var userid = _userManager.GetUserId(HttpContext.User);
            var storeid = _context.Store.FirstOrDefault(s => s.UserId == userid);
            if (storeid == null)
            {
                TempData["msg"] = "<script>alert('You are seller. Can't get in here.');</script>";
                return RedirectToAction("Create", "Stores");
            }
            ViewData["CurrentFilter"] = searchString;
            var books = from s in _context.Book
                        select s;
            books = books.Include(s => s.Store).ThenInclude(u => u.User)
                .Where(u => u.Store.User.Id == userid);
            if (searchString != null)
            {
                books = books.Include(s => s.Store).ThenInclude(u => u.User)
                .Where(u => u.Store.User.Id == userid)
                .Where(s => s.Title.Contains(searchString) || s.Category.Contains(searchString));
            }
            int numOfFilteredStudent = books.Count();
            ViewBag.NumberOfPages = (int)Math.Ceiling((double)numOfFilteredStudent / rowsonepage);
            ViewBag.CurrentPage = id;
            List<Book> bookList = await books.Skip(id * rowsonepage)
                .Take(rowsonepage).ToListAsync();
            if (id > 0)
            {
                ViewBag.idpagprev = id - 1;
            }
            ViewBag.idpagenext = id + 1;
            ViewBag.currentPage = id;
            return View("Views/Home/Search.cshtml",bookList);
        }

    }
}