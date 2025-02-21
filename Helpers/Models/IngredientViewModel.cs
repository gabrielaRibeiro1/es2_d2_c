namespace Helpers.Models;

public class IngredientViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Stock { get; set; }
    public double Proteins { get; set; }
    public double Fats { get; set; }
    public double Carbohydrates { get; set; }
}