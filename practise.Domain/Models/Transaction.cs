namespace practise.Domain.Models;

public class Transaction
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TrasferTo { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Dictionary<string, string> KeyValuePairs { get; set; } = new Dictionary<string, string>();

    
    public bool IsProcessed { get; set; } = false;
    public DateTime DateOfProcessing { get; set; }
}