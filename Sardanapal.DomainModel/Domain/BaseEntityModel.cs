using System.ComponentModel.DataAnnotations;

namespace Sardanapal.DomainModel.Domain
{
    public interface IDomainModel
    {

    }

    public interface IBaseEntityModel<TKey> : IDomainModel
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        TKey Id { get; set; }
    }

    public abstract class BaseEntityModel<TKey> : IBaseEntityModel<TKey>
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        [Key]
        public virtual TKey Id { get; set; }
    }
}
