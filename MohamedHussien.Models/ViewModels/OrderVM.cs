﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MohamedHussien.Models.ViewModels
{
	public class OrderVM
	{
        public OrderHeader orderHeader { get; set; }
        public List<OrderDetails> orderDetail{ get; set; }
    }
}
