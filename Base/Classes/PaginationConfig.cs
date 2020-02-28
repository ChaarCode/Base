using System;
using System.Collections.Generic;
using System.Text;

namespace CharCode.Base.Classes
{
    public class PaginationConfig
    {
        public string SortColumn { get; set; }

        public string Order { get; set; }

        public int Take { get; set; }

        public int Skip { get; set; }

        public ExpressionFilterLogic Logic { get; set; } = ExpressionFilterLogic.And;

        public List<ExpressionFilter> ExpressionFilters { get; set; } = new List<ExpressionFilter>();
    }
}
