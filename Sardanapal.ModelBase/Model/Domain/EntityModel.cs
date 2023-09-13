using Sardanapal.ModelBase.Model.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sardanapal.ModelBase.Model.Domain
{
    public interface IEntityModel<TKey>
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        TKey CreateBy { get; set; }
        TKey ModifiedBy { get; set; }
        DateTime CreatedOnUtc { get; set; }
        DateTime ModifiedOnUtc { get; set; }
    }

    public abstract class EntityModel<TKey> : BaseEntityModel<TKey>, IEntityModel<TKey>
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        public virtual TKey CreateBy { get; set; }
        public virtual TKey ModifiedBy { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime ModifiedOnUtc { get; set; }
    }
}
