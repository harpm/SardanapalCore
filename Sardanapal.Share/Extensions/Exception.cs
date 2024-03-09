
namespace Sardanapal.Share.Extensions;

public static class Exceptions
{
    public static string[] GetHirachicalMessages(this Exception exception)
    {
        List<string> result = new List<string>();

        if (exception != null)
        {
            result.Add(exception.Message);

            if (exception.InnerException != null)
            {
                string[] InnerResult = exception.InnerException.GetHirachicalMessages();
                result.AddRange(InnerResult);
            }
        }

        return result.ToArray();
    }
}