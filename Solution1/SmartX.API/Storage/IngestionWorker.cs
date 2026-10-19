namespace SmartX.API.Storage
{
    public class IngestionWorker : BackgroundService
    {
        private readonly IngestionPipeline _pipeline;
        public IngestionWorker(IngestionPipeline pipeline) => _pipeline = pipeline;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // If paused, or nothing to do, rest briefly instead of spinning the CPU
                if (_pipeline.Paused || _pipeline.ProcessNext() is null)
                {
                    await Task.Delay(50, stoppingToken);
                }
            }
        }
    }
}