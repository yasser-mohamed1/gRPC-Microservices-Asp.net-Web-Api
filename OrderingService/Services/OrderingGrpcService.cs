using Grpc.Core;
using Grpc.Net.Client;
using OrderingService.Protos;
using PaymentService.Protos;
using InventoryService.Protos;
using static PaymentService.Protos.PaymentService;
using static InventoryService.Protos.InventoryService;

namespace OrderingService.Services
{
    public class OrderingGrpcService : OrderingService.Protos.OrderingGrpcService.OrderingGrpcServiceBase
    {
        private readonly GrpcChannel _paymentChannel;
        private readonly GrpcChannel _inventoryChannel;

        public OrderingGrpcService()
        {
            _paymentChannel = GrpcChannel.ForAddress("https://localhost:7134");
            _inventoryChannel = GrpcChannel.ForAddress("https://localhost:7075");
        }

        public override async Task<OrderGrpcResponse> SubmitOrder(OrderGrpcRequest request, ServerCallContext context)
        {
            try
            {
                var paymentClient = new PaymentServiceClient(_paymentChannel);
                var paymentResponse = await paymentClient.DeductUserBalanceAsync(new DeductBalanceRequest
                {
                    UserId = request.UserId,
                    TotalPrice = request.TotalPrice
                });

                if (!paymentResponse.Success)
                {
                    return new OrderGrpcResponse
                    {
                        Success = false,
                        Message = $"Payment failed: {paymentResponse.Message}"
                    };
                }

                var inventoryClient = new InventoryServiceClient(_inventoryChannel);
                var inventoryRequest = new DeductItemsRequest();

                foreach (var item in request.Items)
                {
                    inventoryRequest.Items.Add(new InventoryService.Protos.Item
                    {
                        ItemId = item.ItemId,
                        Quantity = item.Quantity
                    });
                }

                var inventoryResponse = await inventoryClient.DeductItemsBalanceAsync(inventoryRequest);

                if (!inventoryResponse.Success)
                {
                    return new OrderGrpcResponse
                    {
                        Success = false,
                        Message = $"Inventory deduction failed: {inventoryResponse.Message}"
                    };
                }

                return new OrderGrpcResponse
                {
                    Success = true,
                    Message = "Order processed successfully via gRPC."
                };
            }
            catch (Exception ex)
            {
                return new OrderGrpcResponse
                {
                    Success = false,
                    Message = $"Internal order error: {ex.Message}"
                };
            }
        }
    }
}
