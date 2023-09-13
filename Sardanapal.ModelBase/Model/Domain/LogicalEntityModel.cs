using Sardanapal.ModelBase.Model.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sardanapal.ModelBase.Model.Domain
{
    public interface ILogicalEntityModel
    {
        bool IsDeleted { get; set; }
    }

    public abstract class LogicalBaseEntityModel<TKey> : BaseEntityModel<TKey>, ILogicalEntityModel
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        public virtual bool IsDeleted { get; set; }
    }

    public abstract class LogicalEntityModel<TKey> : EntityModel<TKey>, ILogicalEntityModel
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        public virtual bool IsDeleted { get; set; }
    }
}
