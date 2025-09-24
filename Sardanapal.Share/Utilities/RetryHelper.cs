
namespace Sardanapal.Share.Utilities;

/// <summary>
/// This static class provides helper methods to retry an action until it succeeds or a specified number of attempts is reached.
/// Please note that the action to be retried should return a boolean indicating success (true) or failure (false).
/// Consider running these methods inside Response.FillAsync to properly handle and capture exceptions and log them.
/// </summary>
public static class RetryHelper
{
    public static Task RetryUntillSuccessAsync(int offsetTime, Func<Task<bool>> actToRetry, CancellationToken ct = default)
    {
        return Task.Run(async () =>
        {
            while (true)
            {
                if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);

                var res = await actToRetry();
                if (!res)
                {
                    await Task.Delay(offsetTime * 1000);
                }
                else break;
            }
        });
    }

    public static Task RetryUntillSuccess(int offsetTime, Func<bool> actToRetry, CancellationToken ct = default)
    {
        return Task.Run(async() =>
        {
            while (true)
            {
                if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);

                var res = actToRetry();
                if (!res)
                {
                    await Task.Delay(offsetTime * 1000);
                }
                else break;
            }
        });
    }

    public static Task RetryUntillAsync(int offsetTime, int retryCount, Func<Task<bool>> actToRetry, CancellationToken ct = default)
    {
        return Task.Run(async () =>
        {
            for (int i = 0; i < retryCount; i++)
            {
                if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);

                var res = await actToRetry();
                if (!res)
                {
                    await Task.Delay(offsetTime * 1000);
                }
                else break;
            }
        });
    }

    public static Task RetryUntill(int offsetTime, int retryCount, Func<bool> actToRetry, CancellationToken ct = default)
    {
        return Task.Run(async () =>
        {
            for (int i = 0; i < retryCount; i++)
            {
                if (ct.IsCancellationRequested) throw new OperationCanceledException(ct);

                var res = actToRetry();
                if (!res)
                {
                    await Task.Delay(offsetTime * 1000);
                }
                else break;
            }
        });
    }
}
