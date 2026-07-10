using System.Diagnostics;
using Xunit;

namespace Sardanapal.RabbitMQ.Tests;

public sealed class DockerFactAttribute : FactAttribute
{
    private static readonly bool _isDockerAvailable = CheckDocker();

    public DockerFactAttribute()
    {
        if (!_isDockerAvailable)
        {
            Skip = "Docker is not running — Testcontainers integration test skipped.";
        }
    }

    private static bool CheckDocker()
    {
        try
        {
            using var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "info",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            proc?.WaitForExit(5000);
            return proc?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
