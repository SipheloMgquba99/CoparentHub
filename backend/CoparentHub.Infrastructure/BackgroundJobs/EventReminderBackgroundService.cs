using CoparentHub.Application.Features.Push;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoparentHub.Infrastructure.BackgroundJobs
{
    public class EventReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<EventReminderBackgroundService> logger,
        IConfiguration config) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalMinutes = config.GetValue("Reminders:PollIntervalMinutes", 5);
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                    var result = await sender.Send(new SendDueEventRemindersCommand(), stoppingToken);

                    if (result.IsSuccess && result.Value > 0)
                        logger.LogInformation("Sent reminders for {Count} upcoming event(s)", result.Value);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Event reminder poll failed");
                }
            }
        }
    }
}
