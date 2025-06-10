
namespace Sardanapal.Share.Utilities;

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
