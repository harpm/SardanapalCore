using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Sardanapal.Contract.IModel;
using Sardanapal.Domain.Model;
using Sardanapal.Service.Repository;
using Sardanapal.ViewModel.Models;
using Sardanapal.ViewModel.Response;
using Xunit;

namespace Sardanapal.Service.Tests.Unit;

public class MemoryCrudServiceTests
{
    public class Item : BaseEntityModel<int>, IBaseEntityModel<int>
    {
        public string Name { get; set; }
    }

    public class ItemVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ItemNewVM
    {
        public string Name { get; set; }
    }

    public class ItemEditableVM
    {
        public string Name { get; set; }
    }

    public record ItemSearchVM : GridSearchModelVM<int>;

    public class ItemRepo : MemoryRepositoryBase<int, Item>
    {
    }

    public class ItemProfile : Profile
    {
        public ItemProfile()
        {
            CreateMap<ItemNewVM, Item>();
            CreateMap<Item, ItemVM>();
            CreateMap<Item, ItemEditableVM>();
            CreateMap<ItemEditableVM, Item>();
        }
    }

    public class ItemService
        : MemoryCrudServiceBase<ItemRepo, int, Item, ItemSearchVM, ItemVM, ItemNewVM, ItemEditableVM>
    {
        public ItemService(ItemRepo repository, IMapper mapper, ILogger logger)
            : base(repository, mapper, logger)
        {
        }

        protected override string ServiceName => "ItemService";

        protected override IEnumerable<Item> Search(IEnumerable<Item> entities, ItemSearchVM searchVM)
        {
            return entities;
        }
    }

    private static ItemService CreateService(out ItemRepo repo)
    {
        repo = new ItemRepo();
        var mapper = new Mapper(new MapperConfiguration(c => c.AddProfile<ItemProfile>()));
        var logger = Substitute.For<ILogger>();
        return new ItemService(repo, mapper, logger);
    }

    [Fact]
    public async Task Add_ThenGet_ShouldRoundTripEntity()
    {
        // Arrange
        var service = CreateService(out _);

        // Act
        IResponse<int> addResult = await service.Add(new ItemNewVM { Name = "widget" });
        IResponse<ItemVM> getResult = await service.Get(addResult.Data);

        // Assert
        addResult.StatusCode.Should().Be(StatusCode.Succeeded);
        getResult.IsSuccess.Should().BeTrue();
        getResult.Data.Name.Should().Be("widget");
    }

    [Fact]
    public async Task Get_ShouldReturnNotExists_WhenIdMissing()
    {
        // Arrange
        var service = CreateService(out _);

        // Act
        IResponse<ItemVM> result = await service.Get(123);

        // Assert
        result.StatusCode.Should().Be(StatusCode.NotExists);
    }

    [Fact]
    public async Task GetAll_ShouldReturnMappedList()
    {
        // Arrange
        var service = CreateService(out var repo);
        await repo.AddAsync(new Item { Id = 1, Name = "a" });
        await repo.AddAsync(new Item { Id = 2, Name = "b" });

        // Act
        IResponse<GridVM<int, ItemVM>> result =
            await service.GetAll<ItemVM>(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.List.Should().HaveCount(2);
        result.Data.SearchModel.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Edit_ShouldUpdateExistingEntity()
    {
        // Arrange
        var service = CreateService(out var repo);
        await repo.AddAsync(new Item { Id = 1, Name = "old" });

        // Act
        IResponse<bool> result = await service.Edit(1, new ItemEditableVM { Name = "updated" });

        // Assert
        result.IsSuccess.Should().BeTrue();
        (await repo.FetchByIdAsync(1)).Name.Should().Be("updated");
    }

    [Fact]
    public async Task Edit_ShouldReturnNotExists_WhenIdMissing()
    {
        // Arrange
        var service = CreateService(out _);

        // Act
        IResponse<bool> result = await service.Edit(404, new ItemEditableVM { Name = "x" });

        // Assert
        result.StatusCode.Should().Be(StatusCode.NotExists);
    }

    [Fact]
    public async Task Delete_ShouldRemoveEntity()
    {
        // Arrange
        var service = CreateService(out var repo);
        await repo.AddAsync(new Item { Id = 1, Name = "a" });

        // Act
        IResponse<bool> result = await service.Delete(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        (await repo.FetchByIdAsync(1)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_ShouldSurfaceException_WhenIdMissing()
    {
        // Arrange - repo.DeleteAsync throws KeyNotFoundException, FillAsync wraps it as Exception status
        var service = CreateService(out _);

        // Act
        IResponse<bool> result = await service.Delete(404);

        // Assert
        result.StatusCode.Should().Be(StatusCode.Exception);
    }
}
