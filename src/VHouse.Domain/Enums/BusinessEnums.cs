namespace VHouse.Domain.Enums;

public enum BusinessConversationType
{
    General = 1,
    OrderInquiry = 2,
    PriceQuote = 3,
    ProductAvailability = 4,
    DeliveryStatus = 5,
    PaymentInquiry = 6,
    Complaint = 7,
    TechnicalSupport = 8,
    BulkOrder = 9,
    Partnership = 10
}

public enum BusinessPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4,
    Critical = 5
}

public enum CustomerSegment
{
    SmallRetailer = 1,
    MediumRetailer = 2,
    LargeRetailer = 3,
    Restaurant = 4,
    Distributor = 5,
    OnlineStore = 6,
    Wholesaler = 7
}

public enum CommunicationType
{
    OrderConfirmation = 1,
    DeliveryUpdate = 2,
    PaymentReminder = 3,
    ProductAlert = 4,
    PromotionalOffer = 5,
    MarketingCampaign = 6,
    BusinessUpdate = 7,
    TechnicalNotification = 8
}