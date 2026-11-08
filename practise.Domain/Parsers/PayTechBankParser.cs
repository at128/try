using practise.Domain.Models;
using Microsoft.Extensions.Logging;

namespace practise.Domain.Parsers;

public class PayTechBankParser(ILogger<PayTechBankParser> logger) : ITransactionParser
{
    public string BankName => "PayTechBank";

    public Transaction Parse(string data)
    {
        string date="";
        int s=0,datesize=8;
        lock(logger)
        {
            logger.LogInformation($"Parsing data for {BankName}: {data}");
        }

        if(s+datesize>data.Length)
        {
            throw new ArgumentException("1Invalid data format: Date is missing or incomplete.");
        }
        for(int i = s; i < s + datesize; i++)
        {
            if(!char.IsDigit(data[i]))
            {
                throw new ArgumentException($"2Invalid data format: Date contains non-digit characters at position {i}.");
            }
            date += data[i];
        }
        s=s+datesize;
        if(s>=data.Length || !char.IsDigit(data[s]))
        {
            throw new ArgumentException("3Invalid data format: Amount is missing or not a digit.");
        }
        string amount="";
        while(s<data.Length && char.IsDigit(data[s]))
        {
            amount += data[s];
            s++;
        }
        if(s>=data.Length || data[s]!=',')
        {
            throw new ArgumentException("4Invalid data format: Amount is missing or not a valid decimal.");
        }

        amount += '.';
        s++;
        while(s<data.Length && char.IsDigit(data[s]))
        {
            amount += data[s];
            s++;
        }
        if(s>=data.Length || data[s] != '#')
        {
            throw new ArgumentException($"5Invalid data format: Reference is missing or not properly formatted. current {s},{data[s]}");
        }
        string reference = "";
        s++;
        while(s<data.Length && data[s] != '#')
        {
            reference += data[s];
            s++;
        }
        if(s>=data.Length || data[s] != '#')
        {
            throw new ArgumentException("6Invalid data format: Key-value pairs are missing or not properly formatted.");
        }
        s++;
        string keyValuePairs = "";
        while(s<data.Length)
        {
            keyValuePairs += data[s];
            s++;
        }

        DateTime dateTime = DateTime.ParseExact(date, "yyyyMMdd", null);
        decimal amountValue = decimal.Parse(amount);


        
        if (reference.Length <= 8)
        {
            throw new Exception("7Invalid data format: Reference is too short to contain transferTo information.");
        }

        if (reference.Length <= 14)
        {
            throw new Exception("8Invalid data format: Reference is too short to contain transferTo and the identifier of the transaction information.");
        }
        string transferTo="";

        for(int i=8;i<14;i++)
        {
            transferTo+=reference[i];
        }


        Dictionary<string, string> keyValueDict = new Dictionary<string, string>();
        string[] pairs = keyValuePairs.Split('/');
        for(int i = 0; i < pairs.Length; i++)
        {
            if(i % 2 == 0)
            {
                if(i + 1 < pairs.Length)
                {
                    keyValueDict[pairs[i]] = pairs[i + 1];
                }
                else
                {
                    throw new ArgumentException("9Invalid data format: Key-value pair is incomplete.");
                }
            }
        }
        return new Transaction
        {
            Id = reference,
            Amount = amountValue,
            KeyValuePairs = keyValueDict,
            TrasferTo = transferTo,
            BankName = BankName,
            Date = dateTime
        };

    }
}
