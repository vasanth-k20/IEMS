using System.ComponentModel.DataAnnotations;

namespace IEMS.Models
{
    public class Income
    {

        public int Id { get; set; }

        [Required]
        public string Source { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public string Account { get; set; }

    }
}
