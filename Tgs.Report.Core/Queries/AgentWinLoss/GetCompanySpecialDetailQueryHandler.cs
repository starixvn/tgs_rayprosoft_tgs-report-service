using MediatR;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

internal class GetCompanySpecialDetailQueryHandler : IRequestHandler<GetCompanySpecialDetailRequest,
    IEnumerable<CompanySpecialDetailModel>>
{
    private readonly IAgentFactory _agentFactory;

    public GetCompanySpecialDetailQueryHandler(IAgentFactory agentFactory) => _agentFactory = agentFactory;

    public async Task<IEnumerable<CompanySpecialDetailModel>> Handle(
        GetCompanySpecialDetailRequest request,
        CancellationToken cancellationToken)
    {
        var getRequest = new GetSpecialDetailDataRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            PlatformCharge = request.PlatformCharge,
            From = request.From,
            To = request.To
        };

        var details = await _agentFactory.GetSpecialDetailAsync(getRequest);

        var groupId = 0;
        var sortedList = details.OrderBy(x => x.ReportDate).ThenBy(x => x.Percentage).ToArray();

        return sortedList.Select((x, index) =>
        {
            if (index > 0 && x.Percentage != sortedList[index - 1].Percentage)
            {
                groupId = index;
            }

            return new
            {
                GroupId = groupId,
                x.ReportDate,
                x.WinLoss,
                x.Percentage,
                x.BonusAmount
            };
        }).GroupBy(x => new
        {
            x.GroupId,
            x.Percentage
        }, (group, list) => new CompanySpecialDetailModel
        {
            From = list.Min(m => m.ReportDate),
            To = list.Max(m => m.ReportDate),
            Percentage = 100 - group.Percentage,
            PlayerWinLoss = list.Sum(s => s.WinLoss),
            PlayerSpecial = -list.Sum(s => s.BonusAmount),
            CompanyWinLoss = list.Sum(s => s.WinLoss) * (100 - group.Percentage) * 0.01M,
            CompanySpecial = -list.Sum(s => s.BonusAmount) * (100 - group.Percentage) * 0.01M,
            CompanyTotal = list.Sum(s => s.WinLoss) * (100 - group.Percentage) * 0.01M -
                list.Sum(s => s.BonusAmount) * (100 - group.Percentage) * 0.01M
        });
    }
}