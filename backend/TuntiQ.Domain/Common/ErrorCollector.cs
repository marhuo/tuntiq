namespace TuntiQ.Domain.Common;

public sealed class ErrorCollector
{
    private readonly List<ValidationError> _errors = [];

    public bool Has(string key) => _errors.Any(e => e.Key == key);

    public void Add(string key, ErrorCode code, string messageEn, string messageFi) =>
        _errors.Add(new ValidationError(key, code, messageEn, messageFi));

    public void Add(ValidationError error) => _errors.Add(error);

    public void Required(string key, string? value, string fieldEn, string fieldFi)
    {
        if (string.IsNullOrWhiteSpace(value))
            _errors.Add(ValidationErrors.Required(key, fieldEn, fieldFi));
    }

    public void MaxLength(string key, string? value, string fieldEn, string fieldFi, int min, int max)
    {
        int length = value?.Length ?? 0;
        if (length < min || length > max)
            _errors.Add(ValidationErrors.InvalidLength(key, fieldEn, fieldFi, min, max));
    }

    public void ThrowIfAny()
    {
        if (_errors.Count > 0)
            throw new ValidationException(_errors);
    }
}