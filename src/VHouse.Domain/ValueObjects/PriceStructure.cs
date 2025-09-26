namespace VHouse.Domain.ValueObjects;

public record PriceStructure
{
    public decimal Cost { get; init; }
    public decimal Retail { get; init; }
    public decimal Suggested { get; init; }
    public decimal Public { get; init; }
    
    public PriceStructure(decimal cost, decimal retail, decimal suggested, decimal publicPrice)
    {
        if (cost <= 0) throw new ArgumentException("Cost must be positive", nameof(cost));
        if (retail <= cost) throw new ArgumentException("Retail price must be greater than cost", nameof(retail));
        if (suggested < retail) throw new ArgumentException("Suggested price cannot be less than retail", nameof(suggested));
        if (publicPrice < suggested) throw new ArgumentException("Public price cannot be less than suggested", nameof(publicPrice));
        
        Cost = cost;
        Retail = retail;
        Suggested = suggested;
        Public = publicPrice;
    }
    
    public decimal Margin => ((Retail - Cost) / Cost) * 100;
    public decimal Markup => Retail - Cost;
}