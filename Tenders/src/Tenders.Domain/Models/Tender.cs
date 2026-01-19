namespace Tenders.Domain.Models;

public record Tender(int Id, DateOnly Date, string Title, string Description, decimal Amount, List<Supplier> Suppliers);
