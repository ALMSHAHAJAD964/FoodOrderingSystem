namespace FoodOrderingSystem.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string? DeliveryAddress { get; set; }
        public string? Phone { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; 
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
