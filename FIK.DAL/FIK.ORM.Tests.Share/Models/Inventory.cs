using System;
using System.Collections.Generic;
using System.Text;

namespace FIK.ORM.Tests.Share.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
    }
}
