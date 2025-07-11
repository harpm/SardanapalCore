﻿
namespace Sardanapal.Share.Extensions;

public static class Exceptions
{
    public static string[] GetHirachicalMessages(this Exception exception)
    {
        List<string> result = new List<string>();

        if (exception != null)
        {
            result.Add($"Message:\t'{exception.Message}'\nStackTrace:\t{exception.StackTrace}");

            if (exception.InnerException != null)
            {
                string[] InnerResult = exception.InnerException.GetHirachicalMessages();
                result.AddRange(InnerResult);
            }
        }

        return result.ToArray();
    }
}