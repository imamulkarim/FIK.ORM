using System;
using System.Collections.Generic;
using System.Text;

namespace FIK.ORM.Tests.Share.Models
{
    public class Sales
    {
        public long Id { get; set; }
        public long ItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
