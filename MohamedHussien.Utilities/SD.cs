using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MohamedHussien.Utilities
{
    //static Details
    public class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Company = "Company";
        public const string Role_Admin= "Admin";
        public const string Role_Employee = "Employee";


        public const string StatusPending = "pending";
        public const string StatusApproved = "approved";
        public const string StatusInProcess= "processing";
        public const string StatusShipped = "shipped";
        public const string StatusCancelled = "cancelled";
        public const string StatusReFunded = "refunded"; 

		public const string PaymentStatusPending = "pending";
		public const string PaymentStatusApproved = "approved";
		public const string PaymentStatusDelayedApproved = "approvedforDelayedPayment";
		public const string PaymentstatusRejected = "rejected";
	
	}
}
