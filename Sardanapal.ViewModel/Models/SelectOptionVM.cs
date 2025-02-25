
namespace Sardanapal.ViewModel.Models;

public record SelectOptionVM<TKey, TValue>
{
    public TKey Key { get; set; }
    public TValue Value { get; set; } 
}