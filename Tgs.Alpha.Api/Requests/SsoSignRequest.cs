using MediatR;

namespace Tgs.Alpha.Api.Requests;

public class SsoSignRequest : IRequest<SsoModel>
{
    public string? ReferenceId { get; init; }

    public string? BrowserUserAgent { get; init; }

    public string? UserIp { get; init; }
}
