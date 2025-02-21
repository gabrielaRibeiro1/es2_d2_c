namespace ESOF.WebApp.DBLayer.Entities;

public class Order
{
    public int OrderId { get; set; }
    public int? CustomerId { get; set; }
    public Customer Customer { get; set; }
    
    public string DeliveryAddress { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string Status { get; set; }
    
    public ICollection<OrderPizza> OrderPizzas { get; set; }
}
