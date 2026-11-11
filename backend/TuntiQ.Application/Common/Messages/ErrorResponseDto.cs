namespace TuntiQ.Application.Common.Messages;

public sealed record ErrorDto(string Key, string Code, string MessageEn, string MessageFi);

public sealed record ErrorResponseDto(List<ErrorDto> Errors);