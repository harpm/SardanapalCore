using System.ComponentModel.DataAnnotations.Schema;

namespace Sardanapal.ViewModel.Models;

public abstract class BaseListItem<TKey> where TKey : IEquatable<TKey>, IComparable<TKey>
{
    [NotMapped]
    public TKey Id { get; set; }
}