namespace VHouse.Interfaces
{
    public interface IChatbotService
    {
        Task<List<int>> ExtractProductIdsAsync(string catalogJson, string customerInput);
    }
}