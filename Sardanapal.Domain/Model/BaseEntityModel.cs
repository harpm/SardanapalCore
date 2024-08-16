using System.ComponentModel.DataAnnotations;

namespace Sardanapal.Domain.Model;

public abstract class BaseEntityModel<TKey> : IBaseEntityModel<TKey>
    where TKey : IComparable<TKey>, IEquatable<TKey>
{
    [Key]
    public virtual TKey Id { get; set; }
}