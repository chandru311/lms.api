using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace lms.api.Models
{
    public class Address
    {
        [Key]
        public long AddressId { get; set; }

        [Required]
        public long AiId { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string HomeNo { get; set; }

        [Required]
        public string PostalCode { get; set; }

        [AllowNull]
        public string LandMark { get; set; }

        [AllowNull]
        public string CreatedBy { get; set; }
        [AllowNull]
        public DateTime CreatedAt { get; set; }
        [AllowNull]
        public string ModifiedBy { get; set; }
        [AllowNull]
        public DateTime ModifiedAt { get; set; }

    }
}
