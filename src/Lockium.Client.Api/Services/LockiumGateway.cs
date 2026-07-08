using System.Net.Http.Headers;
using System.Net.Http.Json;
using Lockium.Client.Api.Models;
using Lockium.Client.Api.Models.Orders;
using Microsoft.AspNetCore.Http;

namespace Lockium.Client.Api.Services;

public interface ILockiumGateway
{
    Task<OrderLockOpenResponse?> OpenForDepositAsync(long orderId, CancellationToken cancellationToken);
    Task<OrderLockOpenResponse?> OpenForPickupAsync(long orderId, CancellationToken cancellationToken);
}

public sealed class LockiumGateway(
    IHttpClientFactory httpClientFactory,
    IHttpContextAccessor httpContextAccessor,
    Microsoft.Extensions.Options.IOptions<LockiumOptions> options) : ILockiumGateway
{
    public Task<OrderLockOpenResponse?> OpenForDepositAsync(long orderId, CancellationToken cancellationToken) =>
        PostOpenAsync($"api/v1/orders/{orderId}/deposit/open", cancellationToken);

    public Task<OrderLockOpenResponse?> OpenForPickupAsync(long orderId, CancellationToken cancellationToken) =>
        PostOpenAsync($"api/v1/orders/{orderId}/pickup/open", cancellationToken);

    private async Task<OrderLockOpenResponse?> PostOpenAsync(string path, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Lockium");
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        ForwardAuthorization(request);

        using var response = await client.SendAsync(request, cancellationToken);
        return await response.Content.ReadFromJsonAsync<OrderLockOpenResponse>(cancellationToken: cancellationToken);
    }

    private void ForwardAuthorization(HttpRequestMessage request)
    {
        var auth = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(auth))
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(auth);
    }
}
