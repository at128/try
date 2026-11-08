using practise.Domain.Models;

namespace practise.Domain.Parsers;

public interface ITransactionParser
{
    public string BankName { get; }
    Transaction Parse(string data);
}