
namespace Sardanapal.ViewModel.Model;

public class RangeVM<T>
    where T : IComparable<T>, IEquatable<T>
{
    public T Start { get; set; }
    public T End { get; set; }

    public bool Contains(T item)
    {
        return item.CompareTo(Start) >= 0 && item.CompareTo(End) <= 0;
    }
}
