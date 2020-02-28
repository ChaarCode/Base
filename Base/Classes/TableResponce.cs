using System;
using System.Collections.Generic;
using System.Text;

namespace CharCode.Base.Classes
{
    public class TableResponce<T> where T : class
    {
        public List<T> Items { get; set; }
        public long TotalCount { get; set; }
    }
}
