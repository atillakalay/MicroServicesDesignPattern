namespace Order.API.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string BuyerId { get; set; }
        public OrderStatus Status { get; set; }
        public Address Address { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<OrderItem> Items { get; set; }
        public string FailMessage { get; set; } = string.Empty;
    }
}