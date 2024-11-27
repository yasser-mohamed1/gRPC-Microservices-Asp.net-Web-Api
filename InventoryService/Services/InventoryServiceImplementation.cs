namespace InventoryService.Services
{
    using Grpc.Core;
    using InventoryService.Protos;

    public class InventoryServiceImplementation : InventoryService.InventoryServiceBase
    {
        public override Task<DeductItemsResponse> DeductItemsBalance(DeductItemsRequest request, ServerCallContext context)
        {
            bool success = true;
            string message = success ? "Items deducted successfully." : "Failed to deduct items.";

            return Task.FromResult(new DeductItemsResponse
            {
                Success = success,
                Message = message
            });
        }
    }

}
