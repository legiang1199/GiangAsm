using GiangAsm.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiangAsm.Models
{
    public class Store
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string Slogan { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public AppUser User { get; set; } = null!;

        public virtual ICollection<Book> Books { get; set; } = null!;

    }
}