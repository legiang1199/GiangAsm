using GiangAsm.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiangAsm.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public AppUser? User { get; set; } = null!;

        public DateTime OrderDate { get; set; }

        public double Total { get; set; }

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; } = null!;
    }
}