
//
// Define event structures  
//

public struct OrderSuccessEvent
{
    public int OrderId { get; set; }
}

public struct OrderFailureEvent
{
    public int OrderId { get; set; }
    public string Error { get; set; }
}

//
// Order EventAggregator  
//

public class OrderEventAggregator
{
    public event Func<int, Task> OnOrderProcessed;
    public event Func<OrderSuccessEvent, Task> OrderSuccessFunc;
    public event Func<OrderFailureEvent, Task> OrderFailureFunc;

    public async Task PublishOrderAsync(int dataId)
    {
        await OnOrderProcessed?.Invoke(dataId);
    }

    public void PublishSuccessEvent(OrderSuccessEvent e)
    {
        OrderSuccessFunc?.Invoke(e);
    }

    public void PublishFailureEvent(OrderFailureEvent e)
    {
        OrderFailureFunc?.Invoke(e);
    }

    public void SubscribeSuccessEvent(Func<OrderSuccessEvent, Task> callback)
    {
        OrderSuccessFunc += callback;
    }

    public void SubscribeFailureEvent(Func<OrderFailureEvent, Task> callback)
    {
        OrderFailureFunc += callback;
    }
}

public class Order
{
    public int Id { get; set; }
}

public class OrderProcessor
{
    private OrderEventAggregator _eventAggregator;

    public OrderProcessor(OrderEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _eventAggregator.OnOrderProcessed += ProcessOrderAsync;
    }

    private async Task ProcessOrderAsync(int orderId)
    {
        try
        {
            var order = await RetrieveOrderAsync(orderId) ?? throw new InvalidOperationException("Order not found.");
            var discountedOrder = await ApplyDiscountsAsync(order);
            await UpdateOrderAsync(discountedOrder);
            _eventAggregator.PublishSuccessEvent(new OrderSuccessEvent { OrderId = orderId });
        }
        catch (Exception ex)
        {
            _eventAggregator.PublishFailureEvent(new OrderFailureEvent
            {
                OrderId = orderId,
                Error = ex.Message
            });
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
    }
}

//
// Main entry point
//

class Program
{
    static async Task Main(string[] args)
    {
        int orderId = 123;

        var eventAggregator = new OrderEventAggregator();

        eventAggregator.SubscribeSuccessEvent(async (e) =>
        {
            Console.WriteLine($"Order {e.OrderId} processed successfully.");
        });

        eventAggregator.SubscribeFailureEvent(async (e) =>
        {
            Console.WriteLine($"Order {e.OrderId} processing failed: {e.Error}");
        });

        var orderProcessor = new OrderProcessor(eventAggregator);
        await eventAggregator.PublishOrderAsync(orderId);
    }
}



