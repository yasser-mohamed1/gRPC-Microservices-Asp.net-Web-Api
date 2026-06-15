namespace InventoryService.Services
{
    using Grpc.Core;
    using InventoryService.Protos;

    public class InventoryServiceImplementation : InventoryService.InventoryServiceBase
    {
        public override Task<DeductItemsResponse> DeductItemsBalance(DeductItemsRequest request, ServerCallContext context)
        {
            bool success = true;
            for(int i = 0; i < request.Items.Count; i++)
            {
                if (request.Items[i].Quantity <= 0)
                {
                    success = false;
                    break;
                }
            }
            string message = success ? "Items deducted successfully." : "Failed to deduct items.";

            return Task.FromResult(new DeductItemsResponse
            {
                Success = success,
                Message = message
            });
        }
    }

}
