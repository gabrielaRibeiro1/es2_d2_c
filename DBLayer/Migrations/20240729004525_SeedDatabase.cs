using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESOF.WebApp.DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class SeedDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert Ingredients
            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "IngredientId", "Name", "Stock", "Proteins", "Fats", "Carbohydrates" },
                values: new object[,]
                {
                    { 1, "Tomato", 300, 1.0, 0.2, 3.9 },
                    { 2, "Cheese", 150, 25.0, 33.0, 1.3 },
                    { 3, "Pepperoni", 90, 22.0, 40.0, 1.0 },
                    { 4, "Olives", 180, 1.0, 15.0, 6.0 },
                    { 5, "Mushrooms", 240, 3.1, 0.3, 3.3 },
                    { 6, "Bacon", 120, 37.0, 42.0, 1.3 },
                    { 7, "Onions", 200, 1.1, 0.1, 9.3 },
                    { 8, "Peppers", 160, 1.0, 0.2, 4.6 },
                    { 9, "Spinach", 100, 2.9, 0.4, 3.6 },
                    { 10, "Pineapple", 80, 0.5, 0.1, 13.1 }
                }
            );

            // Insert Pizzas
            migrationBuilder.InsertData(
                table: "Pizzas",
                columns: new[] { "PizzaId", "Name", "DoughType" },
                values: new object[,]
                {
                    { 1, "Margherita", "Thin" },
                    { 2, "Pepperoni", "Thick" },
                    { 3, "Vegetarian", "Whole Grain" },
                    { 4, "Hawaiian", "Thin" },
                    { 5, "Meat Lovers", "Thick" },
                    { 6, "BBQ Chicken", "Whole Grain" },
                    { 7, "Four Cheese", "Thin" },
                    { 8, "Supreme", "Thick" },
                    { 9, "Spinach Alfredo", "Whole Grain" }
                }
            );

            // Insert PizzaIngredient relationships
            migrationBuilder.InsertData(
                table: "PizzaIngredients",
                columns: new[] { "PizzaId", "IngredientId" },
                values: new object[,]
                {
                    { 1, 1 }, // Margherita -> Tomato
                    { 1, 2 }, // Margherita -> Cheese
                    { 2, 2 }, // Pepperoni -> Cheese
                    { 2, 3 }, // Pepperoni -> Pepperoni
                    { 3, 1 }, // Vegetarian -> Tomato
                    { 3, 4 }, // Vegetarian -> Olives
                    { 3, 5 }, // Vegetarian -> Mushrooms
                    { 4, 1 }, // Hawaiian -> Tomato
                    { 4, 2 }, // Hawaiian -> Cheese
                    { 4, 10 }, // Hawaiian -> Pineapple
                    { 5, 2 }, // Meat Lovers -> Cheese
                    { 5, 3 }, // Meat Lovers -> Pepperoni
                    { 5, 6 }, // Meat Lovers -> Bacon
                    { 6, 2 }, // BBQ Chicken -> Cheese
                    { 6, 7 }, // BBQ Chicken -> Onions
                    { 6, 8 }, // BBQ Chicken -> Peppers
                    { 7, 2 }, // Four Cheese -> Cheese
                    { 7, 4 }, // Four Cheese -> Olives
                    { 8, 2 }, // Supreme -> Cheese
                    { 8, 3 }, // Supreme -> Pepperoni
                    { 8, 5 }, // Supreme -> Mushrooms
                    { 8, 8 }, // Supreme -> Peppers
                    { 9, 2 }, // Spinach Alfredo -> Cheese
                    { 9, 9 }, // Spinach Alfredo -> Spinach
                    { 9, 7 } // Spinach Alfredo -> Onions
                }
            );

            // Insert Customers
            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "Name", "Phone", "Address" },
                values: new object[,]
                {
                    { 1, "John Doe", "123456789", "123 Main St" },
                    { 2, "Jane Smith", "987654321", "456 Elm St" },
                    { 3, "Bob Johnson", "555666777", "789 Oak St" }
                }
            );

            // Insert Orders
            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[]
                {
                    "OrderId", "CustomerId", "DeliveryAddress", "OrderDate", "DeliveryDate", "Status"
                },
                values: new object[,]
                {
                    { 1, 1, "123 Main St", DateTime.Now, DateTime.Now.AddHours(1), "Received" },
                    { 2, 1, "456 Elm St", DateTime.Now, DateTime.Now.AddHours(2), "In Preparation" },
                    {
                        3, 2, "101 Pine St", DateTime.Now, DateTime.Now.AddHours(3),
                        "Out for Delivery"
                    },
                    { 4, 2, "101 Pine St", DateTime.Now, DateTime.Now.AddHours(1.5), "Received" },
                    { 5, 2, "202 Maple St", DateTime.Now, DateTime.Now.AddHours(2.5), "In Preparation" },
                    {
                        6, 2, "303 Birch St", DateTime.Now, DateTime.Now.AddHours(3.5),
                        "Out for Delivery"
                    },
                    { 7, 3, "404 Cedar St", DateTime.Now, DateTime.Now.AddHours(4), "Received" },
                    { 8, 3, "505 Walnut St", DateTime.Now, DateTime.Now.AddHours(4.5), "In Preparation" },
                    {
                        9, 3,  "606 Chestnut St", DateTime.Now, DateTime.Now.AddHours(5),
                        "Out for Delivery"
                    }
                }
            );
            
            // Insert OrderPizza relationships
            migrationBuilder.InsertData(
                table: "OrderPizzas",
                columns: new[] { "OrderId", "PizzaId", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, 1 }, // Order 1 -> Margherita
                    { 1, 2, 1 }, // Order 1 -> Pepperoni
                    { 1, 4, 1 }, // Order 1 -> Hawaiian
                    { 2, 2, 2 }, // Order 2 -> Pepperoni
                    { 3, 3, 3 }, // Order 3 -> Vegetarian
                    { 4, 4, 2 }, // Order 4 -> Hawaiian
                    { 5, 5, 1 }, // Order 5 -> Meat Lovers
                    { 6, 6, 1 }, // Order 6 -> BBQ Chicken
                    { 6, 1, 2 }, // Order 6 -> Margherita
                    { 7, 7, 4 }, // Order 7 -> Four Cheese
                    { 8, 8, 4 }, // Order 8 -> Supreme
                    { 9, 9, 1 }, // Order 9 -> Spinach Alfredo
                    { 9, 1, 1 }, // Order 9 -> Margherita
                    { 9, 5, 1 }, // Order 9 -> Meat Lovers
                    { 9, 6, 1 }  // Order 9 -> BBQ Chicken
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
