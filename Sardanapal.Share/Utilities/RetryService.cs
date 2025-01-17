
namespace Sardanapal.Share.Utilities;

public static class RetryService
{
    public static Task RetryUntillSuccessAsync(int offsetTime, Func<Task<bool>> actToRetry)
    {
        return Task.Run(async () =>
        {
            while (true)
            {
                var res = await actToRetry();
                if (!res)
                {
                    await Task.Delay(offsetTime * 1000);
                }
                else break;
            }
        });
    }

    public static Task RetryUntillSuccess(int offsetTime, Func<bool> actToRetry)
    {
        return Task.Run(async() =>
        {
            while (true)
            {
                var res = actToRetry();
                if (!res)
                {
                    await Task.Delay(offsetTime * 1000);
                }
                else break;
            }
        });
    }

    public static Task RetryUntillAsync(int offsetTime, int retryCount, Func<Task<bool>> actToRetry)
    {
        return Task.Run(async () =>
        {
            for (int i = 0; i < retryCount; i++)
            {
                var res = await actToRetry();
                if (!res)
                {
                    await Task.Delay(offsetTime * 1000);
                }
                else break;
            }
        });
    }

    public static Task RetryUntill(int offsetTime, int retryCount, Func<bool> actToRetry)
    {
        return Task.Run(async () =>
        {
            for (int i = 0; i < retryCount; i++)
            {
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
