using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace lms.api.Models.RequestModels
{
    public class CreateAddressRequest
    {
        [Required]
        public long AiId { get; set; }
        [Required]
        public string Country { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string HomeNo { get; set; }

        [Required]
        public string PostalCode { get; set; }
#nullable enable
        [AllowNull]
        public string? LandMark { get; set; }
#nullable disable
    }
}
