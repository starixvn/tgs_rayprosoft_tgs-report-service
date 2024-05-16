using MediatR;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

internal class GetAgentTotalDetailQueryHandler(IAgentFactory agentFactory) : IRequestHandler<GetAgentTotalDetailRequest,
    IEnumerable<AgentTotalDetailModel>>
{
    public async Task<IEnumerable<AgentTotalDetailModel>> Handle(
        GetAgentTotalDetailRequest request,
        CancellationToken cancellationToken)
    {
        var getRequest = new GetSpecialDetailDataRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            From = request.From,
            To = request.To
        };

        var details = await agentFactory.GetSpecialDetailAsync(getRequest);

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
                x.BonusAmount,
                x.PlatformChargeAmount
            };
        }).GroupBy(x => new
        {
            x.GroupId,
            x.Percentage
        }, (group, list) => new AgentTotalDetailModel
        {
            From = list.Min(m => m.ReportDate),
            To = list.Max(m => m.ReportDate),
            Percentage = group.Percentage,
            PlayerWinLoss = -list.Sum(s => s.WinLoss),
            PlayerSpecial = list.Sum(s => s.BonusAmount),
            AgentWinLoss = list.Sum(s => s.WinLoss) * group.Percentage * 0.01M,
            AgentSpecial = -list.Sum(s => s.BonusAmount) * group.Percentage * 0.01M,
            PlatformChargeAmount = list.Sum(s => s.PlatformChargeAmount),
            AgentTotal = list.Sum(s => s.WinLoss) * group.Percentage * 0.01M -
                list.Sum(s => s.BonusAmount) * group.Percentage * 0.01M
        });
    }
}