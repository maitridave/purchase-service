using FastEndpoints;

namespace AI.PurchaseService.Domain.Endpoints
{
    public class HealthEndpoint : EndpointWithoutRequest<string>
    {
        public override void Configure()
        {
            Get("/purchase-service/api/v1/test");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Test endpoint";
                s.Description = "Simple test endpoint to verify API is working";
                s.Response<string>(200, "API is working");
            });
        }
        
        public override async Task HandleAsync(CancellationToken ct)
        {
            await SendOkAsync("Purchase Service API is running successfully!", ct);
        }
    }
}