namespace Tenders.Application.Dtos;

public record TenderDto(int Id, DateOnly Date, string Title, string Description, decimal Amount, List<SupplierDto> Suppliers);
