using VHouse.Application.DTOs;
using VHouse.Domain.Entities;

namespace VHouse.Application.Common;

public static class ConsignmentMapper
{
    public static ConsignmentDto ToDto(Consignment consignment, bool includeDetails = false)
    {
        var dto = new ConsignmentDto
        {
            Id = consignment.Id,
            ConsignmentNumber = consignment.ConsignmentNumber,
            ClientTenantId = consignment.ClientTenantId,
            ClientName = consignment.ClientTenant?.TenantName ?? "N/A",
            ConsignmentDate = consignment.ConsignmentDate,
            ExpiryDate = consignment.ExpiryDate,
            Status = consignment.Status,
            StatusSpanish = ConsignmentStatusExtensions.ToSpanish(consignment.Status),
            StatusEmoji = ConsignmentStatusExtensions.GetEmoji(consignment.Status),
            Notes = consignment.Notes,
            Terms = consignment.Terms,
            TotalValueAtCost = consignment.TotalValueAtCost,
            TotalValueAtRetail = consignment.TotalValueAtRetail,
            StorePercentage = consignment.StorePercentage,
            BernardPercentage = consignment.BernardPercentage,
            TotalSold = consignment.TotalSold,
            AmountDueToBernard = consignment.AmountDueToBernard,
            AmountDueToStore = consignment.AmountDueToStore,
            CreatedAt = consignment.CreatedAt,
            UpdatedAt = consignment.UpdatedAt,
            Items = consignment.ConsignmentItems.Select(item => new ConsignmentItemDto
            {
                Id = item.Id,
                ConsignmentId = item.ConsignmentId,
                ProductId = item.ProductId,
                ProductName = item.Product?.ProductName ?? "N/A",
                ProductEmoji = item.Product?.Emoji ?? "📦",
                QuantityConsigned = item.QuantityConsigned,
                QuantitySold = item.QuantitySold,
                QuantityReturned = item.QuantityReturned,
                QuantityAvailable = item.QuantityAvailable,
                CostPrice = item.CostPrice,
                RetailPrice = item.RetailPrice,
                Notes = item.Notes,
                CreatedAt = item.CreatedAt
            }).ToList()
        };

        if (includeDetails)
        {
            dto.Sales = consignment.ConsignmentSales.Select(sale => new ConsignmentSaleDto
            {
                Id = sale.Id,
                ConsignmentId = sale.ConsignmentId,
                ConsignmentItemId = sale.ConsignmentItemId,
                ProductName = sale.ConsignmentItem?.Product?.ProductName ?? "N/A",
                SaleDate = sale.SaleDate,
                QuantitySold = sale.QuantitySold,
                UnitPrice = sale.UnitPrice,
                TotalSaleAmount = sale.TotalSaleAmount,
                StoreAmount = sale.StoreAmount,
                BernardAmount = sale.BernardAmount,
                SaleReference = sale.SaleReference,
                Notes = sale.Notes,
                CreatedAt = sale.CreatedAt
            }).ToList();
        }

        return dto;
    }

    public static ConsignmentSaleDto ToSaleDto(ConsignmentSale sale)
    {
        return new ConsignmentSaleDto
        {
            Id = sale.Id,
            ConsignmentId = sale.ConsignmentId,
            ConsignmentItemId = sale.ConsignmentItemId,
            ProductName = sale.ConsignmentItem?.Product?.ProductName ?? "N/A",
            SaleDate = sale.SaleDate,
            QuantitySold = sale.QuantitySold,
            UnitPrice = sale.UnitPrice,
            TotalSaleAmount = sale.TotalSaleAmount,
            StoreAmount = sale.StoreAmount,
            BernardAmount = sale.BernardAmount,
            SaleReference = sale.SaleReference,
            Notes = sale.Notes,
            CreatedAt = sale.CreatedAt
        };
    }
}
