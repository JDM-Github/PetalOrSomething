using Microsoft.EntityFrameworkCore;
using PetalOrSomething.Data;

public class TransactionExpirationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public TransactionExpirationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var now = DateTime.UtcNow;

                var expiredTransactions = await context.TransactionOrders
                    .Where(t => t.Status == "Pending" && t.ExpirationTime <= now)
                    .ToListAsync(stoppingToken);

                foreach (var transaction in expiredTransactions)
                {
                    transaction.Status = "Expired";
                }

                if (expiredTransactions.Any())
                {
                    await context.SaveChangesAsync(stoppingToken);
                }
            }

            Console.WriteLine("Transaction expiration service is running.");
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
