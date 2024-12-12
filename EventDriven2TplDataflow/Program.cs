
using System.Threading.Tasks.Dataflow;

public class OrderProcessor
{
    public async Task<bool> CompleteOrderProcessAsync(int orderId)
    {
        var retrieveBlock = new TransformBlock<int, Order>(async id => await RetrieveOrderAsync(id));
        var applyDiscountsBlock = new TransformBlock<Order, Order>(async order => await ApplyDiscountsAsync(order));
        var updateOrderBlock = new ActionBlock<Order>(async discountedOrder => await UpdateOrderAsync(discountedOrder));

        retrieveBlock.LinkTo(applyDiscountsBlock, new DataflowLinkOptions { PropagateCompletion = true });
        applyDiscountsBlock.LinkTo(updateOrderBlock, new DataflowLinkOptions { PropagateCompletion = true });

        retrieveBlock.Post(orderId);
        retrieveBlock.Complete();

        try
        {
            await updateOrderBlock.Completion; // Await the completion of the last block  
            Console.WriteLine("Process completed successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Order processing failed: {ex.Message}");
            return false;
        }
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
        //return order;
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
