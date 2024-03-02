using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MohamedHussien.Models
{
    public  class OrderHeader
    {
        public int Id { get; set; }
        public string  ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser applicationUser { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }
        public double  TaotalOrder  { get; set; }

        public string?  OrderStatus  { get; set; }
        public string?  PaymentStatus  { get; set; }
        public string?  TrahingNumber  { get; set; }
        public string?  Carrier  { get; set; }

        public DateTime PaymentDate { get; set; }
        public DateTime PaymentDueDate { get; set; }

        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? Phone { get; set; }
        [Required]
        public string StreetAdress  { get; set; }
        [Required]
        public string City  { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string PostalCode  { get; set; }
        [Required]
        public string Name  { get; set; }
    }
}
