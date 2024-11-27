using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IEMS.Models
{
    public class Expense
    {

        [Key]
        public int ItemId { get; set; }
        [Required]
        public string ItemName { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        [Required]
        public string Account { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime ExpenseDate { get; set; } = DateTime.Now;

        [Required]
        public string Category { get; set; }

        public string? CustomCategory { get; set; }

        public string? Description { get; set; }

        public byte[]? FileData { get; set; }

        public string? FileType { get; set; } 

        public string? FileName { get; set; } 
    }
}
