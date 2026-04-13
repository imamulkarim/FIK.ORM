using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FIK.ORM.Tests.Share.Models
{
    public class ItemBlob
    {
        public long Id { get; set; }
        public byte[] BlobData { get; set; }
    }
}
