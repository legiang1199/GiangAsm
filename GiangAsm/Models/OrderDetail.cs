namespace GiangAsm.Models
{
    public class OrderDetail
    {
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public string BookIsbn { get; set; } = null!;
        public Book Book { get; set; } = null!;

        public int Quantity { get; set; }

        public double Price { get; set; }

    }
}