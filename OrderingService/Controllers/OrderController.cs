using Grpc.Net.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderingService.Dtos;
using PaymentService.Protos;
using static PaymentService.Protos.PaymentService;
using static InventoryService.Protos.InventoryService;
using InventoryService.Protos;

namespace OrderingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly GrpcChannel _paymentChannel;
        private readonly GrpcChannel _inventoryChannel;

        public OrderController()
        {
            _paymentChannel = GrpcChannel.ForAddress("https://localhost:7134"); // replace with your service ip address
            _inventoryChannel = GrpcChannel.ForAddress("https://localhost:7075");
        }

        [HttpPost("submitOrder")]
        public async Task<IActionResult> SubmitOrder([FromBody] OrderRequest orderRequest)
        {
            var paymentClient = new PaymentServiceClient(_paymentChannel);
            var paymentResponse = await paymentClient.DeductUserBalanceAsync(new DeductBalanceRequest
            {
                UserId = orderRequest.UserId,
                TotalPrice = orderRequest.TotalPrice
            });

            if (!paymentResponse.Success)
            {
                return BadRequest(paymentResponse.Message);
            }

            var inventoryClient = new InventoryServiceClient(_inventoryChannel);

            DeductItemsRequest request = new ();

            foreach (var item in orderRequest.Items)
            {
                request.Items.Add(new InventoryService.Protos.Item { ItemId = item.ItemId, Quantity = item.Quantity});
            }

            var inventoryResponse = await inventoryClient.DeductItemsBalanceAsync(request);

            if (!inventoryResponse.Success)
            {
                return BadRequest(inventoryResponse.Message);
            }
            return Ok("Order processed successfully.");
        }
    }

}
