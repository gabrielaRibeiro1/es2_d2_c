namespace ESOF.WebApp.DBLayer.Entities;

public class PizzaIngredient
{
    public int PizzaId { get; set; }
    public Pizza Pizza { get; set; }
    
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; }
}
