// using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CouponsCodeSystemServer.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Key]
        public required string Code { get; set; }      // Unique coupon code
        public required string Description { get; set; }
        public decimal DiscountAmount { get; set; }  // The discount amount (or percentage)
        public bool IsPercentages { get; set; }
        public DateTime CreatedDate { get; set; }  // The date the coupon was created
        public DateTime? ExpirationDate { get; set; }  // Optional expiration date
        public bool AllowDoublePromotion { get; set; }  // Whether it allows double promotions
        public int UsageLimit { get; set; }  // The maximum number of times this coupon can be used
        public int UsageCount { get; set; }  // The number of times this coupon has been used

        // Foreign key to the User who created this coupon
        public int UserId { get; set; }
        // public required User User { get; set; }  // Navigation property
    }
}