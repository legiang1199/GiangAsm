#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GiangAsm.Areas.Identity.Data;
using GiangAsm.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace GiangAsm.Controllers
{
    public class BooksController : Controller
    {
        private readonly UserContext _context;
        private readonly UserManager<AppUser> _userManager;

        private readonly int maxofpage = 10;

        private readonly int rowsonepage = 4;


        public BooksController(UserContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [Authorize(Roles = "Seller")]
        // GET: Books
        public async Task<IActionResult> Index(int id = 0, string searchString = "")
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
            return View(bookList);
        }
        public async Task<IActionResult> DisplayBook(string Isbn)
        {
            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.Isbn == Isbn);
            return View("Views/Books/BookDetails.cshtml", book);
        }
        // GET: Books/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Store)
                .FirstOrDefaultAsync(m => m.Isbn == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            string userid = _userManager.GetUserId(HttpContext.User);

            ViewData["StoreId"] = _context.Store.Where(s => s.UserId == userid).FirstOrDefault().Name;
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Isbn,StoreId,Title,PageNum,Author,Category,Price,Desciption")] Book book, IFormFile ImgUrl)
        {
            var userid = _userManager.GetUserId(HttpContext.User);
            ViewData["StoreId"] = _context.Store.Where(s => s.UserId == userid).FirstOrDefault().Name;

            try
            {
                if (ImgUrl == null)
                {
                    book.ImgUrl = "defaut.jpg";
                }
                else
                {
                    string imgName = book.Isbn + Path.GetExtension(ImgUrl.FileName);
                    string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", imgName);
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        ImgUrl.CopyTo(stream);
                    }
                    book.ImgUrl = imgName;
                }
                Store thisStore = _context.Store.Where(s => s.UserId == userid).FirstOrDefault();
                book.StoreId = thisStore.Id;
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (DbUpdateException)
            {
                TempData["msg"] = "<script>alert('You already add this to cart');</script>";
                return RedirectToAction("Create");

            }
        }


        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Id", book.StoreId);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Isbn,StoreId,Title,PageNum,Author,Category,Price,Desciption,ImgUrl")] Book book)
        {
            if (id != book.Isbn)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Isbn))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["StoreId"] = new SelectList(_context.Store, "Id", "Id", book.StoreId);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Store)
                .FirstOrDefaultAsync(m => m.Isbn == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var book = await _context.Book.FindAsync(id);
            _context.Book.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(string id)
        {
            return _context.Book.Any(e => e.Isbn == id);
        }
    }
}
