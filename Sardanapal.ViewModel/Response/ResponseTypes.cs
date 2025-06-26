
namespace Sardanapal.ViewModel.Response;

public enum OperationType : byte
{
    Fetch = 0,
    Add = 1,
    Edit = 2,
    Delete = 3,
    Function = 4
}

public enum StatusCode : byte
{
    Succeeded = 0,
    Failed = 1,
    Canceled = 3,
    NotExists = 4,
    Exception = 5,
    Duplicate = 6
}