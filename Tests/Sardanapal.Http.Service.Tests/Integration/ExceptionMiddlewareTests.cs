using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Sardanapal.Http.Service.Middlewares;
using Xunit;

namespace Sardanapal.Http.Service.Tests.Integration;

public class ExceptionMiddlewareTests
{
    private static TestServer CreateServer(bool throwException)
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(s => s.AddLogging())
            .Configure(app =>
            {
                app.UseMiddleware<SdHandleExceptionMiddlwere>();
                app.Run(async ctx =>
                {
                    if (throwException)
                    {
                        throw new InvalidOperationException("boom");
                    }

                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                    await ctx.Response.WriteAsync("ok");
                });
            });

        return new TestServer(builder);
    }

    [Fact]
    public async Task Middleware_ShouldPassThrough_WhenNoException()
    {
        // Arrange
        using var server = CreateServer(throwException: false);
        using var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("ok");
    }

    [Fact]
    public async Task Middleware_ShouldReturn500WithJsonBody_WhenExceptionThrown()
    {
        // Arrange
        using var server = CreateServer(throwException: true);
        using var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType.MediaType.Should().Be("application/json");

        string body = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(body);
        // StatusCode.Exception == 5
        document.RootElement.GetProperty("statusCode").GetByte().Should().Be(5);
        document.RootElement.GetProperty("userMessage").GetString().Should().NotBeNullOrEmpty();
        document.RootElement.GetProperty("developerMessages").GetArrayLength().Should().BeGreaterThan(0);
    }
}
