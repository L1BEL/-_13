using System.Linq;
using ClothesAccessoriesApp.Models;

namespace ClothesAccessoriesApp.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Products.Any())
        {
            return;
        }

        var categories = new[]
        {
            new Category { Name = "Кроссовки" },
            new Category { Name = "Куртки" },
            new Category { Name = "Рюкзаки" },
            new Category { Name = "Аксессуары" }
        };

        var brands = new[]
        {
            new Brand { Name = "Velocity" },
            new Brand { Name = "North Orbit" },
            new Brand { Name = "Urban Peak" }
        };

        var materials = new[]
        {
            new Material { Name = "Мембранная ткань" },
            new Material { Name = "Технологичный трикотаж" },
            new Material { Name = "Переработанный нейлон" }
        };

        context.Categories.AddRange(categories);
        context.Brands.AddRange(brands);
        context.Materials.AddRange(materials);
        context.SaveChanges();

        context.Products.AddRange(
            new Product
            {
                Name = "Sprint Pro X",
                Description = "Легкие беговые кроссовки для городских тренировок и зала.",
                Price = 8990m,
                StockQuantity = 24,
                CategoryId = categories[0].Id,
                BrandId = brands[0].Id,
                MaterialId = materials[1].Id
            },
            new Product
            {
                Name = "Storm Layer",
                Description = "Ветровка для активного отдыха с влагозащитой и спортивным силуэтом.",
                Price = 12990m,
                StockQuantity = 11,
                CategoryId = categories[1].Id,
                BrandId = brands[1].Id,
                MaterialId = materials[0].Id
            },
            new Product
            {
                Name = "Trail Pack 18",
                Description = "Компактный рюкзак для фитнеса, прогулок и коротких поездок.",
                Price = 6990m,
                StockQuantity = 17,
                CategoryId = categories[2].Id,
                BrandId = brands[2].Id,
                MaterialId = materials[2].Id
            },
            new Product
            {
                Name = "Pulse Cap",
                Description = "Дышащая бейсболка с контрастным спортивным кантом.",
                Price = 2490m,
                StockQuantity = 30,
                CategoryId = categories[3].Id,
                BrandId = brands[0].Id,
                MaterialId = materials[2].Id
            });

        context.SaveChanges();
    }
}
