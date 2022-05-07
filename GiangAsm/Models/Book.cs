using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiangAsm.Models
{
    public class Book
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Isbn { get; set; } = null!;

        public int StoreId { get; set; }

        public Store? Store { get; set; } = null!;

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; } = null!;

        public string Title { get; set; } = null!;

        [Display(Name = "Page Number")]
        public int PageNum { get; set; }

        public string Author { get; set; } = null!;

        public string Category { get; set; } = null!;

        public double Price { get; set; }

        public string Desciption { get; set; } = null!;
        public string ImgUrl { get; set; } 

        public virtual ICollection<Cart>? Carts { get; set; }
    }
}