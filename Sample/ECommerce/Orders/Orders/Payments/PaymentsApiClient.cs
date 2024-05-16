using System.Net.Http.Json;
using System.Text.Json;
using Orders.Payments.DiscardingPayment;
using Orders.Payments.RequestingPayment;

namespace Orders.Payments;

public class PaymentsApiClient(HttpClient client)
{
    public  Task<HttpResponseMessage> Request(RequestPayment command, CancellationToken ct)
    {
        return client.PostAsJsonAsync("/api/payments", command, ct);
    }

    public Task<HttpResponseMessage> Discard(DiscardPayment command, CancellationToken ct)
    {
        return client.DeleteAsync($"/api/payments/{command.PaymentId}", ct);
    }
}
