
public class OrderProcessor
{
    public Task<bool> CompleteOrderProcessAsync(int orderId)
    {
        var tcs = new TaskCompletionSource<bool>();

        RetrieveOrderAsync(orderId).ContinueWith(retrieveTask =>
        {
            if (retrieveTask.Status == TaskStatus.RanToCompletion)
            {
                var order = retrieveTask.Result;
                if (order != null)
                {
                    ApplyDiscountsAsync(order).ContinueWith(discountTask =>
                    {
                        if (discountTask.Status == TaskStatus.RanToCompletion)
                        {
                            var discountedOrder = discountTask.Result;
                            UpdateOrderAsync(discountedOrder).ContinueWith(saveTask =>
                            {
                                if (saveTask.Status == TaskStatus.RanToCompletion)
                                {
                                    tcs.SetResult(true); // Indicates success  
                                }
                                else
                                {
                                    Console.WriteLine($"Error processing order: {saveTask.Exception.Message}");
                                    tcs.SetResult(false);
                                }
                            });
                        }
                        else
                        {
                            Console.WriteLine($"Error processing order: {discountTask.Exception.Message}");
                            tcs.SetResult(false);
                        }
                    });
                }
                else
                {
                    Console.WriteLine($"Error processing order: Order not found.");
                    tcs.SetResult(false);
                }
            }
            else
            {
                Console.WriteLine($"Error processing order: {retrieveTask.Exception.Message}");
                tcs.SetResult(false);
            }
        });

        return tcs.Task;
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
