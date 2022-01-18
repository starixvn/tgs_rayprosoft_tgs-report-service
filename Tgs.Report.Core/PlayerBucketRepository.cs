using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository.Configurations;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Core
{
    public class PlayerBucketFactory : IPlayerBucketFactory
    {
        private readonly ITgsService<PlayerBucketHistory> _playerBucketHistoryService;
        private readonly ITgsService<PlayerBucket> _playerBucketService;

        public PlayerBucketFactory(
            ITgsService<PlayerBucket> playerBucketService,
            ITgsService<PlayerBucketHistory> playerBucketHistoryService)
        {
            _playerBucketService = playerBucketService;
            _playerBucketHistoryService = playerBucketHistoryService;
        }

        public async ValueTask FillInBucketAsync(FillInPlayerBucketsRequest request)
        {
            var playerCodes = request.PlayerTurnovers.Select(x => x.PlayerCode).ToArray();

            var playerBuckets = await _playerBucketService.GetQueries()
                .Where(x => playerCodes.Contains(x.PlayerCode) && !x.IsFull).ToArrayAsync();
            var playerBucketHistories = new List<PlayerBucketHistory>();

            foreach (var player in request.PlayerTurnovers)
            {
                var turnover = player.Turnover;
                var buckets = playerBuckets.Where(x => x.PlayerCode == player.PlayerCode).ToArray();

                foreach (var playerBucket in buckets)
                {
                    if (turnover <= 0)
                    {
                        break;
                    }

                    var amount = 0.0M;

                    if (turnover - (playerBucket.BucketAmount - playerBucket.CurrentBucketAmount) >= 0.0M)
                    {
                        amount = playerBucket.BucketAmount - playerBucket.CurrentBucketAmount;
                        turnover -= amount;
                        playerBucket.IsFull = true;
                        playerBucket.CurrentBucketAmount = playerBucket.BucketAmount;
                    }
                    else
                    {
                        amount = turnover;
                        playerBucket.CurrentBucketAmount += turnover;
                        turnover = 0;
                    }

                    playerBucketHistories.Add(new()
                    {
                        PlayerCode = player.PlayerCode,
                        Turnover = amount,
                        From = request.From,
                        To = request.To,
                        CreatedOn = DateTimeOffset.UtcNow
                    });
                }
            }

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            await _playerBucketService.UpdateEntitiesAsync(playerBuckets);
            await _playerBucketHistoryService.CreateEntitiesAsync(playerBucketHistories.ToArray());

            scope.Complete();
        }
    }
}