using Tenders.Application.Dtos;
using Tenders.Domain.Models;

namespace Tenders.Application.Extensions;

public static class SuppliersExtensions
{
    public static SupplierDto ToDto(this Supplier supplier)
    {
        return new SupplierDto(supplier.Id, supplier.Name);
    }
}
