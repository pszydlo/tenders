using Tenders.Application.Dtos;
using Tenders.Domain.Models;

namespace Tenders.Application.Extensions;

public static class TendersExtensions
{
    public static TenderDto ToDto (Tender tender)
    {
        return new TenderDto(
            tender.Id, 
            tender.Date, 
            tender.Title, 
            tender.Description, 
            tender.Amount, 
            tender.Suppliers
                .Select(a => a.ToDto())
                .ToList());
    }
}
