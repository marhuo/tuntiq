namespace TuntiQ.Domain.Common;

public class ValidationException(IReadOnlyList<ValidationError> errors) : Exception("Validation failed.")
{
    public ValidationException(ValidationError error) : this([error]) { }

    public ValidationException(string key, ErrorCode code, string messageEn, string messageFi) : this(new ValidationError(key, code, messageEn, messageFi))
    {
    }
    public IReadOnlyList<ValidationError> Errors { get; } = errors;


    public static ValidationException Required(string key, string fieldEn, string fieldFi) =>
        new(ValidationErrors.Required(key, fieldEn, fieldFi));

    public static ValidationException InvalidLength(string key, string fieldEn, string fieldFi, int min, int max) =>
        new(ValidationErrors.InvalidLength(key, fieldEn, fieldFi, min, max));

    public static ValidationException Taken(string key, string fieldEn, string fieldFi) =>
        new(ValidationErrors.Taken(key, fieldEn, fieldFi));

    public static ValidationException NotFound(string key, string fieldEn, string fieldFi) =>
        new(ValidationErrors.NotFound(key, fieldEn, fieldFi));

    public static ValidationException NotFound() => 
        new(new ValidationError("", ErrorCode.NotFound,
            "The requested item was not found.",
            "Pyydettyä kohdetta ei löytynyt."));

    public static ValidationException Deleted(string nameEn, string nameFi) =>
        new(new ValidationError("id", ErrorCode.Deleted,
            $"{nameEn} has been deleted.",
            $"{nameFi} on poistettu."));

    public static ValidationException Deleted() =>
        new(new ValidationError("", ErrorCode.Deleted,
            "The item has been deleted.",
            "Kohde on poistettu."));
}
