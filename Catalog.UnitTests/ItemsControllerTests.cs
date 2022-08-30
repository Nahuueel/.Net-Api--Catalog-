using Catalog.Api.Controllers;
using Catalog.Api.Dtos;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Catalog.UnitTests;

public class UnitTest1
{
    private readonly Mock<IItemsRepository> repositoryStub = new();
    private readonly Mock<ILogger<ItemsController>> loggerStub = new();
    private readonly Random rand = new();

    [Fact]
    public async Task GetItemAsync_WithUnexistingItem_ReturnNotFound()
    {
        //arrange
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Item)null); 

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);
        //act

        var result = await controller.GetItemAsync(Guid.NewGuid());

        //assert 
        result.Result.Should().BeOfType<NotFoundResult>();

    }

    [Fact]
    public async Task GetItemAsyn_WithExistingItem_ReturnsExpectedItem(){
        //arrange
        var expectedItem = CreateRandomItem();

        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync(expectedItem); 

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);
        
        //act
        var result = await controller.GetItemAsync(Guid.NewGuid());

        //assert
        result.Value.Should().BeEquivalentTo(expectedItem);

    }

    [Fact]
    public async Task GetItemsAsyn_WithExistingItems_ReturnsAllItems(){
        //arrange
        var expectedItems = new[] {CreateRandomItem(),CreateRandomItem(),CreateRandomItem()};

        repositoryStub.Setup(repo => repo.GetItemsAsync())
            .ReturnsAsync(expectedItems); 

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //act
        var actualItems = await controller.GetItemsAsync();

        //assert
        actualItems.Should().BeEquivalentTo(expectedItems);

    }

    [Fact]
    public async Task GetItemsAsync_WithMatchingItems_ReturnsMatchingItems(){
        //arrange
        var allItems = new[] 
        {
            new Item(){Name = "Potion"},
            new Item(){Name = "Antidote"},
            new Item(){Name = "hi-Potion"}
        };

        var nameToMatch = "Potion";

        repositoryStub.Setup(repo => repo.GetItemsAsync())
            .ReturnsAsync(allItems); 

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //act
        IEnumerable<ItemDto> foundItems = await controller.GetItemsAsync(nameToMatch);

        //assert
        foundItems.Should().OnlyContain(
            item => item.Name == allItems[0].Name || item.Name == allItems[2].Name
        );

    }

    [Fact]
    public async Task CreateItemAsync_WithItemToCreate_ReturnsCreatedItem(){
        //arrange
        var ItemToCreate = new CreateItemDto(
                            Guid.NewGuid().ToString(), 
                            Guid.NewGuid().ToString(), 
                            rand.Next(1000));

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //act
        var result = await controller.CreateItemAsync(ItemToCreate);      

        //assert
        var createdItem = (result.Result as CreatedAtActionResult).Value as ItemDto;
        ItemToCreate.Should().BeEquivalentTo(
            createdItem,
            options => options.ComparingByMembers<ItemDto>().ExcludingMissingMembers()
        );
        createdItem.Id.Should().NotBeEmpty();
        createdItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMilliseconds(1000));
    
    }

        [Fact]
    public async Task UpdateItemAsync_WithExistingItem_ReturnsNoConent(){
        //arrange
        Item existingItem = CreateRandomItem();
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync(existingItem); 

        var itemId = existingItem.Id;
        var itemToUpdate = new UpdateItemDto(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(), 
            existingItem.Price + rand.Next(100));

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //act
        var result = await controller.UpdateItemAsync(itemId, itemToUpdate);
        
        //assert
        result.Should().BeOfType<NoContentResult>();

    }


    [Fact]
    public async Task DeleteItemAsync_WithExistingItem_ReturnsNoConent(){
        
        //arrange
        Item existingItem = CreateRandomItem();
        repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync(existingItem); 

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);

        //act
        var result = await controller.DeleteItemAsync(existingItem.Id);
        
        //assert
        result.Should().BeOfType<NoContentResult>();

    }


    private Item CreateRandomItem(){
        return new(){
            Id = Guid.NewGuid(),
            Name = Guid.NewGuid().ToString(),
            Price = rand.Next(1000),
            CreatedDate = DateTimeOffset.UtcNow
        }; 
    }
}