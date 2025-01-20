
namespace Sardanapal.Contract.IService;

public interface IRequestService
{
    string IP { get; set; }
}

public interface IRequestService<TUserKey>
    where TUserKey : IComparable<TUserKey>, IEquatable<TUserKey>
{
    TUserKey GetUserId();
}