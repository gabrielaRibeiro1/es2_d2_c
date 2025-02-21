namespace ESOF.WebApp.DBLayer.Entities;

public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public ICollection<Order> Orders { get; set; }
}