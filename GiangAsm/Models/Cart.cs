using GiangAsm.Areas.Identity.Data;

namespace GiangAsm.Models
{
    public class Cart
    {
        public string UserId { get; set; }
        public string BookIsbn { get; set; }
        public int Quantity { get; set; }
        public AppUser? User { get; set; }
        public Book? Book { get; set; }

    }
}

