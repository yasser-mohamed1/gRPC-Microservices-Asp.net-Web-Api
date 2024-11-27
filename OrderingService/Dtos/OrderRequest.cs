namespace OrderingService.Dtos
{
    public class OrderRequest
    {
        public int UserId { get; set; }
        public int TotalPrice { get; set; }
        public List<Item> Items { get; set; }
    }
}
