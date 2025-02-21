namespace ESOF.WebApp.DBLayer.Entities;

public class Ingredient
{
    public int IngredientId { get; set; }
    public string Name { get; set; }
    public int Stock { get; set; }
    public double Proteins { get; set; }
    public double Fats { get; set; }
    public double Carbohydrates { get; set; }
    public ICollection<PizzaIngredient> PizzaIngredients { get; set; }
}