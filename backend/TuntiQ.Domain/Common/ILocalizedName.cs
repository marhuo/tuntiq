namespace TuntiQ.Domain.Common;

public interface ILocalizedName
{
    string NameEn { get; }
    string NameFi { get; }
}
public sealed record LocalizedName(string NameFi, string NameEn) : ILocalizedName;