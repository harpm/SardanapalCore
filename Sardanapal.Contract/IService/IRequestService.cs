
namespace Sardanapal.Contract.IService;

public interface IRequestService<TUserKey>
    where TUserKey : IComparable<TUserKey>, IEquatable<TUserKey>
{
    string IP { get; set; }
    TUserKey GetUserId();
}