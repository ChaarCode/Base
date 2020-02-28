using System;
using System.Collections.Generic;
using System.Text;

namespace CharCode.Base.Classes
{
    public class ExpressionFilter
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public Comparison Comparison { get; set; }
    }
}
