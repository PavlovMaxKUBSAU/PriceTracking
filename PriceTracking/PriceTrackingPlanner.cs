namespace PriceTracking
{
    public class PriceTrackingPlanner : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _timer;

        public PriceTrackingPlanner(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CheckPriceChanges, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async void CheckPriceChanges(object state)
        {
            var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<PriceTrackingService>();
            await service.CheckPriceChangesAsync();
        }
    }
}
