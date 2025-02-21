namespace ESOF.WebApp.DBLayer.Entities;

public class Pizza
{
    public int PizzaId { get; set; }
    public string Name { get; set; }
    public string DoughType { get; set; } // Thin, Whole Grain, etc.
    public ICollection<PizzaIngredient> PizzaIngredients { get; set; }
    public ICollection<OrderPizza> OrderPizzas { get; set; }
}