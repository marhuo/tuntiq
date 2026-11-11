namespace TuntiQ.Domain.Common;

public enum ErrorCode { Required, InvalidLength, InvalidFormat, OutOfRange, Taken, NotFound, NotAllowed, Overlap, Deleted }

public sealed record ValidationError(string Key, ErrorCode Code, string MessageEn, string MessageFi);

public static class ValidationErrors
{
    public static ValidationError Required(string key, string fieldEn, string fieldFi) =>
        new(key, ErrorCode.Required, $"{fieldEn} is required.", $"{fieldFi} on pakollinen.");

    public static ValidationError InvalidLength(string key, string fieldEn, string fieldFi, int min, int max) =>
        new(key, ErrorCode.InvalidLength,
            $"{fieldEn} must be {min}-{max} characters.",
            $"{fieldFi} pitää olla {min}-{max} merkkiä.");

    public static ValidationError Taken(string key, string fieldEn, string fieldFi) =>
        new(key, ErrorCode.Taken, $"{fieldEn} is already taken.", $"{fieldFi} on jo käytössä.");

    public static ValidationError NotFound(string key, string fieldEn, string fieldFi) =>
        new(key, ErrorCode.NotFound, $"{fieldEn} was not found.", $"{fieldFi} ei löydy.");

}
