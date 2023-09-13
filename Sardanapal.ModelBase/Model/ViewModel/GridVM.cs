using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sardanapal.ModelBase.Model.ViewModel
{
    public class GridVM<T, S>
        where T : class
        where S : class
    {
        public List<T> List { get; set; }
        public GridSearchModelVM<S> SearchModel { get; set; }
        public int TotalCount { get; set; }

        public GridVM()
        {

        }

        public GridVM(GridSearchModelVM<S> searchModel)
        {
            SearchModel = searchModel;
        }
    }
}
