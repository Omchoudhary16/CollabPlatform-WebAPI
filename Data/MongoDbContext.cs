using CollabPlatform.Api.Models;
using MongoDB.Driver;

namespace CollabPlatform.Api.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("categories");
    public IMongoCollection<CollaborationRequest> CollaborationRequests => _database.GetCollection<CollaborationRequest>("collaborationRequests");

    public async Task SeedCategoriesAsync()
    {
        if (await Categories.CountDocumentsAsync(FilterDefinition<Category>.Empty) == 0)
        {
            var defaultCategories = new List<Category>
            {
                new() { Name = "Fashion" },
                new() { Name = "Beauty" },
                new() { Name = "Fitness" },
                new() { Name = "Tech" },
                new() { Name = "Travel" },
                new() { Name = "Food" }
            };
            await Categories.InsertManyAsync(defaultCategories);
        }
    }
}