using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

Console.WriteLine("=== Week 3 Laboratory: Data Manipulation, Collections, LINQ, Files, Exceptions ===");

var products = new List<Product>();

var inputProducts = new List<Product>
{
    new("SKU001", "Laptop", "Electronics", 48000m, 5),
    new("SKU002", "Mouse", "Electronics", 750m, 20),
    new("SKU003", "Keyboard", "Electronics", 1500m, 0),
    new("SKU004", "Rice", "Grocery", 60m, 3),
    new("SKU005", "Coffee", "Grocery", 250m, 15),
    new("", "Invalid Product", "Miscellaneous", -10m, -2),
    new("SKU001", "Duplicate Laptop", "Electronics", 48000m, 5)
};

foreach (var product in inputProducts)
{
    var errors = ProductValidator.Validate(product, products);

    if (errors.Count == 0)
    {
        products.Add(product);
        Console.WriteLine($"Added: {product.Name}");
    }
    else
    {
        Console.WriteLine($"Rejected: {product.Name}");

        foreach (var error in errors)
        {
            Console.WriteLine($"  - {error}");
        }
    }
}

// Dictionary lookup
var productLookup = products.ToDictionary(p => p.Sku, p => p);

string searchSku = "SKU002";

Console.WriteLine($"\nDictionary search for {searchSku}:");

if (productLookup.TryGetValue(searchSku, out var foundProduct))
{
    Console.WriteLine($"Found: {foundProduct.Name}, Price: {foundProduct.Price:C}");
}
else
{
    Console.WriteLine("Product not found.");
}

// LINQ search using FirstOrDefault
var coffee = products.FirstOrDefault(p =>
    p.Name.Equals("Coffee", StringComparison.OrdinalIgnoreCase));

Console.WriteLine("\nLINQ search for Coffee:");

if (coffee is not null)
{
    Console.WriteLine($"Found: {coffee.Name}");
}
else
{
    Console.WriteLine("Not found.");
}

// LINQ filtering and sorting
var electronics = products
    .Where(p => p.Category == "Electronics")
    .OrderBy(p => p.Name)
    .ToList();

Console.WriteLine("\nElectronics Products:");

foreach (var product in electronics)
{
    Console.WriteLine($"{product.Name} - {product.Price:C}");
}

// LINQ high-value products
var highValueProducts = products
    .Where(p => p.Price >= 1000)
    .OrderByDescending(p => p.Price)
    .Select(p => new
    {
        p.Sku,
        p.Name,
        p.Price
    })
    .ToList();

Console.WriteLine("\nHigh-Value Products:");

foreach (var item in highValueProducts)
{
    Console.WriteLine($"{item.Sku} | {item.Name} | {item.Price:C}");
}

// LINQ out-of-stock products
var outOfStockProducts = products
    .Where(p => p.Stock == 0)
    .Select(p => p.Name)
    .ToList();

Console.WriteLine("\nOut of Stock:");

foreach (var productName in outOfStockProducts)
{
    Console.WriteLine(productName);
}

// LINQ grouping
var groupedProducts = products
    .GroupBy(p => p.Category)
    .OrderBy(g => g.Key)
    .ToList();

Console.WriteLine("\nProducts by Category:");

foreach (var group in groupedProducts)
{
    Console.WriteLine($"{group.Key}: {group.Count()} product(s)");

    foreach (var product in group)
    {
        Console.WriteLine($"  - {product.Name}");
    }
}

// LINQ aggregation
decimal totalInventoryValue = products.Sum(p => p.Price * p.Stock);

Console.WriteLine($"\nTotal inventory value: {totalInventoryValue:C}");

// Save products to JSON
string jsonFileName = "products.json";

FileService.SaveAsJson(products, jsonFileName);

Console.WriteLine($"\nProducts saved to {jsonFileName}.");

// Load products from JSON
var loadedProducts = FileService.LoadFromJson(jsonFileName);

Console.WriteLine($"Loaded {loadedProducts.Count} products from {jsonFileName}.");

// Try loading a missing file
var missingProducts = FileService.LoadFromJson("missing-products.json");

Console.WriteLine($"Loading missing file returned {missingProducts.Count} products.");

// Export text report
string reportFileName = "report.txt";

FileService.ExportTextReport(products, reportFileName);

Console.WriteLine($"Report exported to {reportFileName}.");

// Read and display text report
if (File.Exists(reportFileName))
{
    string[] reportLines = File.ReadAllLines(reportFileName);

    Console.WriteLine("\nReport Content:");

    foreach (var line in reportLines)
    {
        Console.WriteLine(line);
    }
}

// Exception handling demo
Console.WriteLine("\nException Handling Demo:");

try
{
    string invalidPriceText = "abc";
    decimal invalidPrice = decimal.Parse(invalidPriceText);

    Console.WriteLine($"Parsed price: {invalidPrice}");
}
catch (FormatException ex)
{
    Console.WriteLine($"Format error caught: {ex.Message}");
}
finally
{
    Console.WriteLine("Finally block executed.");
}

Console.WriteLine("\nActivity completed.");

public record Product(string Sku, string Name, string Category, decimal Price, int Stock);

public static class ProductValidator
{
    public static List<string> Validate(Product product, List<Product> existingProducts)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(product.Sku))
        {
            errors.Add("SKU is required.");
        }

        if (string.IsNullOrWhiteSpace(product.Name))
        {
            errors.Add("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(product.Category))
        {
            errors.Add("Category is required.");
        }

        if (product.Price < 0)
        {
            errors.Add("Price must be zero or higher.");
        }

        if (product.Stock < 0)
        {
            errors.Add("Stock must be zero or higher.");
        }

        bool skuExists = existingProducts.Any(p =>
            string.Equals(p.Sku, product.Sku, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(product.Sku) && skuExists)
        {
            errors.Add("SKU already exists.");
        }

        return errors;
    }
}

public static class FileService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public static void SaveAsJson(List<Product> products, string filePath)
    {
        try
        {
            string json = JsonSerializer.Serialize(products, Options);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving JSON file: {ex.Message}");
            throw;
        }
    }

    public static List<Product> LoadFromJson(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("JSON file not found. Returning empty list.");
                return new List<Product>();
            }

            string json = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Product>();
            }

            return JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON format error: {ex.Message}");
            return new List<Product>();
        }
        catch (IOException ex)
        {
            Console.WriteLine($"File access error: {ex.Message}");
            return new List<Product>();
        }
    }

    public static void ExportTextReport(List<Product> products, string filePath)
    {
        try
        {
            var lines = new List<string>
            {
                "Product Inventory Report",
                $"Generated: {DateTime.Now}",
                new string('-', 40)
            };

            foreach (var product in products)
            {
                lines.Add($"{product.Sku} | {product.Name} | {product.Category} | {product.Price:C} | Stock: {product.Stock}");
            }

            lines.Add(new string('-', 40));
            lines.Add($"Total products: {products.Count}");
            lines.Add($"Total inventory value: {products.Sum(p => p.Price * p.Stock):C}");

            File.WriteAllLines(filePath, lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting report: {ex.Message}");
            throw;
        }
    }
}