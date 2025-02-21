namespace ESOF.WebApp.DBLayer.Entities;

public class OrderPizza
{
    public int OrderId { get; set; }
    public Order Order { get; set; }
    
    public int PizzaId { get; set; }
    public Pizza Pizza { get; set; }
    
    public uint Quantity { get; set; }
}
