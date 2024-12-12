# Branching Strategies - Event Driven

## The Tangled Web

Consider an online store backend service that processes customer order - retrieving, discounting, and updating orders, all executed asynchronously. This is usually the point where things get messy: your code becomes tangled with nested callbacks, and it starts to look like this:

```csharp
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

//
// Usage example
//

int orderId = 123;

bool success = await CompleteOrderProcessAsync(orderId);
if (success)
{
    Console.WriteLine("Order processing completed successfully.");
}
else
{
    Console.WriteLine("Order processing failed.");
}
```

This approach suffers from deeply nested structures, error-prone error handling, and a challenging debugging experience.

## 1. Task-based Asynchronous Pattern (TAP)

The first step towards simplifying our code is to adopt the Task-based Asynchronous Pattern (TAP) using `async` and `await` keywords, which streamline the writing of asynchronous code. This pattern allows us to write code that appears linearly like synchronous code, but operates asynchronously.

```csharp
public async Task<bool> CompleteOrderProcessAsync(int orderId)
{
    try
    {
        var order = await RetrieveOrderAsync(orderId) ?? throw new InvalidOperationException("Order not found.");
        var discountedOrder = await ApplyDiscountsAsync(order);
        await UpdateOrderAsync(discountedOrder);
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error processing order: " + ex.Message);
        return false;
    }
}
```

Just like that, our nested callbacks are a thing of the past. The code is cleaner, exceptions can be caught in a unified manner, and the cognitive load on any developer reading or debugging the code is greatly reduced.


## 2. TPL Dataflow

Next, let's explore how the Task Parallel Library (TPL) Dataflow, a library for building complex asynchronous and parallel processing pipelines, offers a different technique compared to TAP.

```csharp
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
```

By employing TPL Dataflow, our process is now represented as a series of connected blocks, resembling an assembly line. This not only makes the execution flow more explicit but also simplifies handling the data through each asynchronous step.


## 3. Reactive Extensions

For scenarios that deal with streams of data or events, Reactive Extensions (Rx) provides a powerful model. Rx turns our entire asynchronous process into observable sequences, enabling us to approach the problem with a more declarative mindset.

```csharp
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

//
// Usage example
//

int orderId = 123;

var orderProcessor = new OrderProcessor();
var processedOrderObservable = orderProcessor.CompleteOrderProcess(orderId);

processedOrderObservable.Subscribe(
    onNext: success => Console.WriteLine(success ? "Order processed and saved successfully." : "Order processing failed."),
    onError: ex => Console.WriteLine("An error occurred: " + ex.Message),
    onCompleted: () => Console.WriteLine("Processing complete.")
);
```

By using Reactive Extensions, we now have a chain of operations that clearly defines what should happen at each step, and we can gracefully handle exceptions. Subscriptions manage the outcome, making the code elegant and conceptually consistent.

## 4. Event Aggregator Pattern

Last on our transformative journey, we reach the Event Aggregator pattern, which provides a centralized mechanism to manage events between publishers and subscribers, thus avoiding direct callback references in event-driven systems.

```csharp
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
}

//
// Usage example
//

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
```

The Event Aggregator pattern embodies the principle of loose coupling. With it, we can publish events from one part of an application and subscribe to them in another without the two needing to know about each other directly.

