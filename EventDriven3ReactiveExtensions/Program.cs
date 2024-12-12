using System.Reactive.Linq;

public class OrderProcessor
{
    public IObservable<bool> CompleteOrderProcess(int orderId)
    {
        var orderObservable = Observable.FromAsync(() => RetrieveOrderAsync(orderId));
        return orderObservable
            .SelectMany(order =>
            {
                if (order == null)
                {
                    throw new InvalidOperationException("Order not found.");
                }
                return Observable.FromAsync(() => ApplyDiscountsAsync(order));
            })
            .SelectMany(discountedOrder => Observable.FromAsync(() => UpdateOrderAsync(discountedOrder)))
            .Select(_ => true)
            .Catch<bool, Exception>(ex =>
            {
                Console.WriteLine($"Error processing order: {ex.Message}");
                return Observable.Return(false);
            });
    }

    public async Task<Order> RetrieveOrderAsync(int orderId)
    {
        Console.WriteLine($"Retrieving Order.");
        await Task.Delay(1000);
        return new Order { Id = orderId };
        //return null;
    }

    public async Task<Order> ApplyDiscountsAsync(Order order)
    {
        Console.WriteLine($"Applying Discounts.");
        await Task.Delay(1000);
        return order;
        //throw new NotImplementedException();
    }

    public async Task UpdateOrderAsync(Order order)
    {
        Console.WriteLine($"Updating Order.");
        await Task.Delay(1000);
    }
}

public class Order
{
    public int Id { get; set; }
}

//
// Main entry point
//

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Asynchronous Order Processing Demo");

        int orderId = 123;

        var orderProcessor = new OrderProcessor();
        var processedOrderObservable = orderProcessor.CompleteOrderProcess(orderId);

        processedOrderObservable.Subscribe(
            onNext: success => Console.WriteLine(success ? "Order processed and saved successfully." : "Order processing failed."),
            onError: ex => Console.WriteLine("An error occurred: " + ex.Message),
            onCompleted: () => Console.WriteLine("Processing complete.")
        );

        // block
        Console.ReadKey(); // Press any key to exit...
    }
}
