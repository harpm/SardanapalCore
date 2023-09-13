using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sardanapal.ModelBase.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EntityOptions : Attribute
    {
        public string Title { get; set; }
        public string OrderBy { get; set; }

        public EntityOptions(string Title = null, string OrderBy = null)
        {
            this.Title = Title;
            this.OrderBy = OrderBy;
        }
    }
}
