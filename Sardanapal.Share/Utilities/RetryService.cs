
namespace Sardanapal.Share.Utilities;

public static class RetryService
{
    public static Task RetryUntillSuccessAsync(int seconds, Func<Task<bool>> actToRetry)
    {
        return Task.Run(async () =>
        {
            while (true)
            {
                var res = await actToRetry();
                if (!res)
                {
                    await Task.Delay(seconds * 1000);
                }
                else break;
            }
        });
    }

    public static Task RetryUntillSuccess(int seconds, Func<bool> actToRetry)
    {
        return Task.Run(async() =>
        {
            while (true)
            {
                var res = actToRetry();
                if (!res)
                {
                    await Task.Delay(seconds * 1000);
                }
                else break;
            }
        });
    }
}
