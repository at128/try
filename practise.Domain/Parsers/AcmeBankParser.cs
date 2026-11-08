using practise.Domain.Models;

namespace practise.Domain.Parsers;

public class AcmeBankParser : ITransactionParser
{
    public string BankName => "AcmeBank";

    public Transaction Parse(string data)
    {

        string[] parts = data.Split("//");

        if (parts.Length < 3)
        {
            throw new ArgumentException("Invalid data format: Insufficient parts in the transaction data.");
        }

        string amount = parts[0];
        string reference = parts[1];
        string date = parts[2];

        for(int i = 0; i < amount.Length; i++)
        {
            if(amount[i]==',')
            {
                amount = amount.Replace(',', '.');
                break;
            }
        }

        if(!decimal.TryParse(amount, out decimal amountValue))
        {
            throw new ArgumentException("Invalid data format: Amount is not a valid decimal.");
        }

        if(!DateTime.TryParseExact(date, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
        {
            throw new ArgumentException("Invalid data format: Date is not in the expected format (yyyyMMdd).");
        }

        if(reference.Length <= 8)
        {
            throw new ArgumentException("Invalid data format: Reference is too short to contain transferTo information.");
        }



        string transferTo = reference.Length > 14 ? reference.Substring(8, 6) : throw new ArgumentException("Invalid data format: Reference is too short to contain transferTo information.");

        
        return new Transaction
        {
            Id = reference,
            Amount = amountValue,
            KeyValuePairs = new Dictionary<string, string>(),
            TrasferTo = transferTo,
            BankName = BankName,
            Date = dateTime
        };
    }
}