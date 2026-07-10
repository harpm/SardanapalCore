
namespace Sardanapal.Share.Utilities;

/// <summary>
/// This static class provides helper methods to retry an action until it succeeds or a specified number of attempts is reached.
/// Please note that the action to be retried should return a boolean indicating success (true) or failure (false).
/// Consider running these methods inside Response.FillAsync to properly handle and capture exceptions and log them.
/// </summary>
public static class RetryHelper
{
    public static Task RetryUntilSuccessAsync(int offsetTime, Func<Task<bool>> actToRetry, CancellationToken ct = default)
    {
        return Task.Run(async () =>
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                bool succeeded = false;
                try
                {
                    succeeded = await actToRetry();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    // Transient failure; retry after delay.
                }

                if (!succeeded)
                {
                    await Task.Delay(offsetTime * 1000, ct);
                }
                else break;
            }
        });
    }

    public static Task RetryUntilSuccess(int offsetTime, Func<bool> actToRetry, CancellationToken ct = default)
    {
        return Task.Run(async () =>
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                bool succeeded = false;
                try
                {
                    succeeded = actToRetry();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    // Transient failure; retry after delay.
                }

                if (!succeeded)
                {
                    await Task.Delay(offsetTime * 1000, ct);
                }
                else break;
            }
        });
    }

    public static Task RetryUntilAsync(int offsetTime, int retryCount, Func<Task<bool>> actToRetry, CancellationToken ct = default)
    {
        return Task.Run(async () =>
        {
            Exception lastException = null;

            for (int i = 0; i < retryCount; i++)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    if (await actToRetry())
                    {
                        return;
                    }
                    lastException = null;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                if (i < retryCount - 1)
                {
                    await Task.Delay(offsetTime * 1000, ct);
                }
            }

            if (lastException != null)
            {
                throw lastException;
            }
        });
    }

    public static Task RetryUntil(int offsetTime, int retryCount, Func<bool> actToRetry, CancellationToken ct = default)
    {
        return Task.Run(async () =>
        {
            Exception lastException = null;

            for (int i = 0; i < retryCount; i++)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    if (actToRetry())
                    {
                        return;
                    }
                    lastException = null;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                if (i < retryCount - 1)
                {
                    await Task.Delay(offsetTime * 1000, ct);
                }
            }

            if (lastException != null)
            {
                throw lastException;
            }
        });
    }
}
