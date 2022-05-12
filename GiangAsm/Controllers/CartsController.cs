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
    public class CartsController : Controller
    {
        private readonly UserContext _context;
        private readonly UserManager<AppUser> _userManager;


        public CartsController(UserContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Carts
        public async Task<IActionResult> Index()
        {
            var userid = _userManager.GetUserId(HttpContext.User);

            var cartShopContext = _context.Cart.Include(c => c.Book)
                                                .Include(c => c.User)
                                                .Where(u => u.UserId == userid);

            return View(await cartShopContext.ToListAsync());

        }

        // GET: Carts/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Cart
                .Include(c => c.Book)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }
        

        public IActionResult CartDetail()
        {
            return View("Views/Carts/index.cshtml");
        }

        // GET: Carts/Create
        public IActionResult Create()
        {
            ViewData["BookIsbn"] = new SelectList(_context.Book, "Isbn", "Isbn"); //??
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id"); //??
            return View();
        }

        // POST: Carts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,BookIsbn")] Cart cart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookIsbn"] = new SelectList(_context.Book, "Isbn", "Isbn", cart.BookIsbn);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", cart.UserId);
            return View(cart);
        }

        // GET: Carts/Edit/5
        public async Task<IActionResult> Edit(string uid, string bid)
        {
            /*var thisUserId = _userManager.GetUserId(HttpContext.User);
            Cart myCart = new Cart() { UserId = thisUserId, BookIsbn = Isbn, Quantity= quantity };
            Cart fromDb = _context.Cart.FirstOrDefault(c => c.UserId == thisUserId && c.BookIsbn == Isbn);*/
            if (uid == null || bid == null)
            {
                return NotFound();
            }

            var cart = await _context.Cart
                .FirstOrDefaultAsync(m => m.UserId == uid && m.BookIsbn == bid);
            //if not existing (or null), add it to cart. If already added to Cart before, ignore it.
            return View(cart);

        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("UserId,BookIsbn,Quantity")] Cart cart, int quantity)
        {
            try
            {

                _context.Cart.Update(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                return RedirectToAction("Edit", new { uid = cart.UserId, bid = cart.BookIsbn });
            }
        }

        // GET: Carts/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Cart
                .Include(c => c.Book)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // POST: Carts/Delete/5
        
        public async Task<IActionResult> Remove(string id)
        {
            var userid = _userManager.GetUserId(HttpContext.User);

            var cart = _context.Cart.Where(s => s.UserId == userid).FirstOrDefault();
            _context.Cart.Remove(cart);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddToCart(string isbn,int quantity = 1)
        {
            try
            {

                var thisUserId = _userManager.GetUserId(HttpContext.User);
                    Cart myCart = new Cart() { UserId = thisUserId, BookIsbn = isbn, Quantity = quantity };
                    Cart fromDb = _context.Cart.FirstOrDefault(c => c.UserId == thisUserId && c.BookIsbn == isbn);
                    //if not existing (or null), add it to cart. If already added to Cart before, ignore it.
                    if (fromDb == null)
                    {
                       
                        _context.Add(myCart);
                        await _context.SaveChangesAsync();

                    }
                    return RedirectToAction("Index");

                
            }
            catch (InvalidOperationException)
            {
                TempData["msg"] = "<script>alert('You are seller. Can't get in here.');</script>";
                return RedirectToAction("SearchBook", "Home");
            }

        }
        public async Task<IActionResult> Checkout()
        {
            string thisUserId = _userManager.GetUserId(HttpContext.User);
            List<Cart> myDetailsInCart = await _context.Cart
                .Where(c => c.UserId == thisUserId)
                .Include(c => c.Book)
                .ToListAsync();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    //Step 1: create an order
                    Order myOrder = new Order();
                    myOrder.UserId = thisUserId;
                    myOrder.OrderDate = DateTime.Now;

                    var total = 0;

                    for (int i = 0; i < myDetailsInCart.Count; i++)
                    {
                       total = total+ (myDetailsInCart[i].Quantity * (int)myDetailsInCart[i].Book.Price);
                    }




                    myOrder.Total = total;
                    _context.Add(myOrder);
                    await _context.SaveChangesAsync();

                    //Step 2: insert all order details by var "myDetailsInCart"
                    foreach (var item in myDetailsInCart)
                    {
                        OrderDetail detail = new OrderDetail()
                        {
                            OrderId = myOrder.Id,
                            BookIsbn = item.BookIsbn,
                            Quantity = item.Quantity,
                            Price = item.Quantity * (int)item.Book.Price,
                        };
                        _context.Add(detail);
                    }
                    await _context.SaveChangesAsync();

                    //Step 3: empty/delete the cart we just done for thisUser
                    _context.Cart.RemoveRange(myDetailsInCart);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (DbUpdateException ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Error occurred in Checkout" + ex);
                }
            }

            return RedirectToAction("Index", "Carts");
        }
        private bool CartExists(string id)
        {
            return _context.Cart.Any(e => e.UserId == id);
        }
    }
}
