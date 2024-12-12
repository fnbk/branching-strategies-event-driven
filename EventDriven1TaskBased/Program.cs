
public class OrderProcessor
{
    public async Task<bool> CompleteOrderProcessAsync(int orderId)
    {
        try
        {
            var order = await RetrieveOrderAsync(orderId) ?? throw new InvalidOperationException("Order not found.");
            var discountedOrder = await ApplyDiscountsAsync(order);
            await UpdateOrderAsync(discountedOrder);
            return true; // Indicates success
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error processing order: " + ex.Message);
            return false; // Indicates failure
        }
    }

    public async Task<Order> RetrieveOrderAsync(int orderId)
    {
        Console.WriteLine($"Retrieving Order.");
        await Task.Delay(1000);
        return new Order { Id = orderId };
        // return null;
    }

    public async Task<Order> ApplyDiscountsAsync(Order order)
    {
        Console.WriteLine($"Applying Discounts.");
        await Task.Delay(1000);
        return order;
        // throw new NotImplementedException();
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

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Asynchronous Order Processing Demo");

        int orderId = 123; // Example order ID  
        var orderProcessor = new OrderProcessor();
        bool success = await orderProcessor.CompleteOrderProcessAsync(orderId);

        if (success)
        {
            Console.WriteLine($"Order {orderId} processed successfully.");  
        }
        else
        {
            Console.WriteLine($"Order {orderId} processing failed");  
        }
    }
}
