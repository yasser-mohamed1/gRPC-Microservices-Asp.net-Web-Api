namespace PaymentService.Services
{
    using Grpc.Core;
    using PaymentService.Protos;

    public class PaymentServiceImplementation : PaymentService.PaymentServiceBase
    {
        public override Task<DeductBalanceResponse> DeductUserBalance(DeductBalanceRequest request, ServerCallContext context)
        {
            bool success = true;
            if(request.TotalPrice <= 0)
            {
                success = false;
            }
            string message = success ? "Balance deducted successfully." : "Failed to deduct balance.";
            return Task.FromResult(new DeductBalanceResponse
            {
                Success = success,
                Message = message
            });
        }
    }

}
