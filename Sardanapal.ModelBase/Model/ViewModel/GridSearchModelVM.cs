using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sardanapal.ModelBase.Model.ViewModel
{
    public abstract class GridSearchModelVM
    {
        public string SortId { get; set; }
        public bool SortAsccending { get; set; } = true;
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }

    public class GridSearchModelVM<TSearch> : GridSearchModelVM
        where TSearch : class
    {
        public TSearch? Fields { get; set; }
    }
}
