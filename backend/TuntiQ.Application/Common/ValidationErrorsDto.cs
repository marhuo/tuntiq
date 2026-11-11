using System.Collections.ObjectModel;

namespace TuntiQ.Application.Common;

public class ValidationErrorsDto
{
    public ReadOnlyCollection<ValidationErrorDto> Errors { get; private set; } = new([]);
}

public record class ValidationErrorDto(string Key, string Error);
