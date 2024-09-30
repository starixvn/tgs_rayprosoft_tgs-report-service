using Tgs.Report.Repository.Entities;
using MediatR;
using Tgs.Alpha.Api.Requests;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Tgs.Alpha.Api.Queries;

public class GetUserByReferenceIdHandler(TgsContext context, IConfiguration configuration) : IRequestHandler<SsoSignRequest, SsoModel>
{
    private readonly TgsContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<SsoModel> Handle(SsoSignRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.ReferenceId))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ReferenceId == request.ReferenceId, cancellationToken);
            if (user != null)
            {
                var token = "";
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                using (var hmacsha256 = new HMACSHA256(user.Salt))
                {
                    var hashMessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes($"{request.ReferenceId}-{timestamp}"));
                    token = Convert.ToHexString(hashMessage);
                }

                return new SsoModel { ReturnUrl = $"{configuration["BackOfficeUrl"]}/login?referenceId={request.ReferenceId}&timestamp={timestamp}&token={token}" };
            }
        }

        return new SsoModel { ReturnUrl = "" };
    }
}
