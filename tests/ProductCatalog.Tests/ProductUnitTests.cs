
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Api.Data;
using ProductCatalog.Api.Models;
using Xunit;

namespace ProductCatalog.Tests;

public class ProductUnitTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

   public ProductUnitTests(WebApplicationFactory<Program> factory)
{
    // Un seul nom de base de données pour tous les DbContext du test
    var databaseName = "UnitTestingDb_" + Guid.NewGuid();

    _factory = factory.WithWebHostBuilder(builder =>
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });
        });
    });
}

    // 1. Test unitaire : Ajout d'un produit
    [Fact]
    public async Task AddProduct_ReturnsCreated_WhenProductIsValid()
    {
        var client = _factory.CreateClient();

        var newProduct = new Product
        {
            Name = "Clavier Mecanique",
            Price = 89.50m
        };

        var response = await client.PostAsJsonAsync("/api/create-product", newProduct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdProduct = await response.Content.ReadFromJsonAsync<Product>();

        Assert.NotNull(createdProduct);
        Assert.Equal("Clavier Mecanique", createdProduct.Name);
        Assert.Equal(89.50m, createdProduct.Price);
    }

    // 2. Test unitaire : Listing des produits
    [Fact]
    public async Task GetAllProducts_ReturnsOkAndProductList()
    {
        var client = _factory.CreateClient();

        // Pré-insertion d'un produit de test
        await client.PostAsJsonAsync(
            "/api/create-product",
            new Product
            {
                Name = "Ecran 27 pouces",
                Price = 250.00m
            });

        var response = await client.GetAsync("/api/get-all-products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<List<Product>>();

        Assert.NotNull(products);
        Assert.NotEmpty(products);
        Assert.Contains(products, p => p.Name == "Souris sans fil");
    }
}
