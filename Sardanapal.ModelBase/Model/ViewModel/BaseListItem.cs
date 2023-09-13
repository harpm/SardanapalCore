using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sardanapal.ModelBase.Model.ViewModel
{
    public abstract class BaseListItem<TKey> where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        [NotMapped]
        public TKey Id { get; set; }
    }
}
