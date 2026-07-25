using AutoMapper;
using ClosedXML.Excel;
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

public class MemoryPanelServiceTests
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

    public class ItemPanelService
        : MemoryPanelServiceBase<ItemRepo, int, Item, ItemSearchVM, ItemVM, ItemNewVM, ItemEditableVM>
    {
        public ItemPanelService(ItemRepo repository, IMapper mapper, ILogger logger)
            : base(repository, mapper, logger)
        {
        }

        protected override string ServiceName => "ItemPanelService";

        protected override IEnumerable<Item> Search(IEnumerable<Item> entities, ItemSearchVM searchVM)
        {
            return entities;
        }
    }

    private static ItemPanelService CreateService(out ItemRepo repo)
    {
        repo = new ItemRepo();
        IMapper mapper = new Mapper(new MapperConfiguration(c => c.AddProfile<ItemProfile>()));
        ILogger logger = Substitute.For<ILogger>();
        return new ItemPanelService(repo, mapper, logger);
    }

    private static XLWorkbook Parse(byte[] bytes)
    {
        using MemoryStream stream = new MemoryStream(bytes);
        return new XLWorkbook(stream);
    }

    [Fact]
    public async Task GetExcel_ShouldReturnValidExcelBytes_WithData()
    {
        // Arrange
        ItemPanelService service = CreateService(out var repo);
        await repo.AddAsync(new Item { Id = 1, Name = "alpha" });
        await repo.AddAsync(new Item { Id = 2, Name = "beta" });

        // Act
        IResponse<byte[]> result = await service.GetExcel<ItemVM>(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().NotBeEmpty();

        using XLWorkbook workbook = Parse(result.Data);
        IXLWorksheet sheet = workbook.Worksheets.First();
        sheet.Cell(1, 1).GetValue<string>().Should().Be("Id");
        sheet.Cell(1, 2).GetValue<string>().Should().Be("Name");
        sheet.LastRowUsed().RowNumber().Should().Be(3);
    }

    [Fact]
    public async Task GetExcel_ShouldExportAllRows_IgnoringPaging()
    {
        // Arrange
        ItemPanelService service = CreateService(out var repo);
        for (int i = 1; i <= 30; i++)
        {
            await repo.AddAsync(new Item { Id = i, Name = "n" + i });
        }

        GridSearchModelVM<int, ItemSearchVM> search = new GridSearchModelVM<int, ItemSearchVM>
        {
            PageIndex = 0,
            PageSize = 25
        };

        // Act
        IResponse<byte[]> result = await service.GetExcel<ItemVM>(search);

        // Assert - default PageSize is 25, but export must contain all 30 rows
        result.IsSuccess.Should().BeTrue();
        using XLWorkbook workbook = Parse(result.Data);
        IXLWorksheet sheet = workbook.Worksheets.First();
        sheet.LastRowUsed().RowNumber().Should().Be(31);
    }

    [Fact]
    public async Task GetExcel_ShouldRespectRequestedColumns()
    {
        // Arrange
        ItemPanelService service = CreateService(out var repo);
        await repo.AddAsync(new Item { Id = 1, Name = "alpha" });

        GridSearchModelVM<int, ItemSearchVM> search = new GridSearchModelVM<int, ItemSearchVM>
        {
            Columns = new[] { "Name" }
        };

        // Act
        IResponse<byte[]> result = await service.GetExcel<ItemVM>(search);

        // Assert
        result.IsSuccess.Should().BeTrue();
        using XLWorkbook workbook = Parse(result.Data);
        IXLWorksheet sheet = workbook.Worksheets.First();
        sheet.Cell(1, 1).GetValue<string>().Should().Be("Name");
        sheet.Row(1).LastCellUsed().WorksheetColumn().ColumnNumber().Should().Be(1);
    }
}
