New Pipe Example
==============
[Click to return to Pipes](README.md)

This assumes that you have read the [Composite Pipe Example](SimpleExample.md).
In particular, this example will not cover extending the fluent build syntax.

### Our Goals
Creating pipes from existing pipes is fairly easy, but now we would like to implement a pipe which cannot be built from existing pipes.

We would like to create a **SwitchOutletPipe** with the following behaviour:
* The switch outlet pipe initially will only send a message down its left outlet.
* Every time a message is sent, it will switch outlets, so that the next message must be sent through the opposite outlet.

For example, we'd like to be able to write this test:
```c#
var switchPipe = PipeBuilder.New.SwitchOutletPipe<int>().Build();

var inletCapacityPipe = PipeBuilder.New.CapacityPipe<int>().WithCapacity(int.MaxValue).Build();
var leftOutletCapacityPipe = PipeBuilder.New.CapacityPipe<int>().WithCapacity(int.MaxValue).Build();
var rightOutletCapacityPipe = PipeBuilder.New.CapacityPipe<int>().WithCapacity(int.MaxValue).Build();

for (var i = 0; i < 10; i++)
{
    inletCapacityPipe.Inlet.Send(i);
}

switchPipe.Inlet.ConnectTo(inletCapacityPipe.Outlet);
switchPipe.LeftOutlet.ConnectTo(leftOutletCapacityPipe.Inlet);
switchPipe.RightOutlet.ConnectTo(rightOutletCapacityPipe.Inlet);

inletCapacityPipe.StoredMessages.Should().BeEmpty();
leftOutletCapacityPipe.StoredMessages.Should().BeEquivalentTo(0, 2, 4, 6, 8);
rightOutletCapacityPipe.StoredMessages.Should().BeEquivalentTo(1, 3, 5, 7, 9);
```

### Getting Started

Our Switch Outlet Pipe will need to expose the following inlets and outlets.
```c#
public interface ISwitchOutletPipe<TMessage> : IPipe
{
    ISimpleInlet<TMessage> Inlet { get; }
    ISimpleOutlet<TMessage> LeftOutlet { get; }
    ISimpleOutlet<TMessage> RightOutlet { get; }
}
```

Our implementation will look like:
```c#
public class SwitchOutletPipe<TMessage> : SimplePipe<TMessage>, ISwitchOutletPipe<TMessage>
{
    private IOutlet nextOutlet;
    public ISimpleInlet<TMessage> Inlet { get; }
    public ISimpleOutlet<TMessage> LeftOutlet { get; }
    public ISimpleOutlet<TMessage> RightOutlet { get; }
    
    public SwitchOutletPipe(ISimpleInlet<TMessage> inlet, ISimpleOutlet<TMessage> leftOutlet, ISimpleOutlet<TMessage> rightOutlet)
        : base(new[] {inlet}, new[] {leftOutlet, rightOutlet})
    {
        Inlet = inlet;
        LeftOutlet = leftOutlet;
        RightOutlet = rightOutlet;

        nextOutlet = LeftOutlet;
    }
    
    private void Switch()
    {
        nextOutlet = nextOutlet == LeftOutlet ? RightOutlet : LeftOutlet;
    }
    
    protected override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage) { ... }
    protected override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage) { ... }
}
```
Some notes:
* We've extended something called **SimplePipe**.
  * This is a simple extension of the Pipe base class designed to make creating a pipe easier.
  * It is called a "simple" pipe as it requires your pipe to handle only one type of message (TMessage).
* **nextOutlet** will remember which outlet should receive the next message.
* **Switch** will switch the next outlet to receive a message.

The two methods FindSender and FindReceiver are the most challenging part of the implementation. We'll look at those in detail:

### Senders and Receivers

* A **Sender** is a function that should trigger the sending of a message.
  * For example, executing `sender()` might allow a thread waiting to send a message to continue execution.
* A **Receiver** is a function that should trigger the receiving of a message.
  * For example, executing `receiver(m)` might allow a thread waiting to receive a message to continue execution after being passed "m".

Therefore, the method `FindSender` is asking you to find someone who could send a message, and `FindReceiver` is asking you to find someone who can receive a message.

#### Contract

Both methods require you to return a function. This function must:
* Never throw an exception.
* Only modify the pipe system if executed. The act of calling the FindSender or FindReceiver method itself should not modify the pipe system at all.
* Return null if you could not find a receiver / sender.
* Aim to **Resolve the Pipe System**

A pipe system is called **resolved** if it is impossible for any sender's message to move closer to a receiver. All messages should be pushed down the pipe system as far as possible.

**Note:** You can rely on the FindSender / FindReceiver function to only be run by one thread at a time. In fact, only one thread can run within an entire pipe system at any one time.

### Implementing FindReceiver

Firstly, we need to check which outlet should receive the message.
```c#
protected override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
{
    return nextOutlet == LeftOutlet ? CreateReceiver(LeftOutlet) : CreateReceiver(RightOutlet);
}
```
Now we need to check if the correct outlet can actually receive the message.

Every outlet supports a method **FindReceiver** which will check to see if the outlet has a thread ready to receive the message, or if it is connected to a pipe system that can receive the message. If the outlet does not have a receiver, it will return null.

Every inlet supports an analogous method **FindSender** which we'll use further below.

Therefore, we can implement **CreateReceiver** as follows:
```c#
private Action<TMessage> CreateReceiver(IOutlet<TMessage> outlet)
{
    // See if the outlet can receive a message
    var receiver = outlet.FindReceiver();
    if (receiver == null) return null; // It cannot, so neither can this pipe.
    
    // The outlet can receive the message.
    return m =>
    {
        // Ensure the opposite outlet receives the message next time.
        Switch();
        
        // Pass the message to the outlet's receiver.
        receiver(m);
    };
}
```

### Implementing FindSender
This is harder than FindReceiver in our case. Consider the following:
* We are asked to find a receiver.
* We check the next outlet we should use, and it does indeed have a receiver.
* We pass it the message and switch our outlets.
* However, our inlet has more messages ready.
* Since we switched our outlet, our new next outlet may already have a receiver that was previously blocked.
* Therefore, the pipe system won't be **resolved** unless we pull another message from our inlet to this outlet.

Therefore, after we've switched outlets, we need to check if we can send another message. This gives us the implementation:

```c#
protected override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage)
{
    var permittedOutlet = nextOutlet == LeftOutlet ? LeftOutlet : RightOutlet;
    var oppositeOutlet = nextOutlet == LeftOutlet ? RightOutlet : LeftOutlet;
    
    // We only allow the next outlet to receive messages.
    if (outletReceivingMessage != permittedOutlet) return null;

    // Check if the inlet has a message it can send.
    var sender = Inlet.FindSender();
    if (sender == null) return null; //it did not

    // The inlet does have a message it can send.
    return () =>
    {
        Switch();
        // Get the message from the inlet.
        var message = sender();
        
        // We check if the outlet we just switched to can receive a message.
        var oppositeReceiver = oppositeOutlet.FindReceiver();
        if (oppositeReceiver == null) return message;

        // It can receive a message. Check our inlet to see if another message is available.
        var secondSender = Inlet.FindSender();
        if (secondSender == null) return message;
        
        // Another message was available, so switch again.
        Switch();
        oppositeReceiver(secondSender());

        // Finally, return the original message provided by our inlet.
        return message;
    };
}
```
That's a little complicated, but implements exactly and only what is necessary to resolve the pipe system.

You may wonder why we don't need to check our inlet for yet another message. We can rely on the outlet receiving the message (parameter of the function) to ask us for *another* message if it is capable of receiving a second message, as it too will try to resolve the pipe system.

If you don't believe me, connect this to other pipes and run some tests against it until you are satisfied. All pipes will work in this way.

That's all there is to it. You can extend the fluent build syntax to make creating the pipe much easier, and use the test at the start of this example if you like. For convenience, here's the code. (Explanation in the [Composite Pipe Example](SimpleExample.md))

```c#
public static class PipeExtensions
{
    public static ISwitchOutletPipeBuilder<TMessage> SwitchOutletPipe<TMessage>(this IPipeBuilder pipeBuilder)
    {
        return new SwitchOutletPipeBuilder<TMessage>();
    }
}

public interface ISwitchOutletPipeBuilder<TMessage>
{
    ISwitchOutletPipe<TMessage> Build();
}

public class SwitchOutletPipeBuilder<TMessage> : ISwitchOutletPipeBuilder<TMessage>
{
    public ISwitchOutletPipe<TMessage> Build()
    {
        var promisedPipe = new Promised<IPipe>();

        var inlet = new SimpleInlet<TMessage>(promisedPipe);
        var leftOutlet = new SimpleOutlet<TMessage>(promisedPipe);
        var rightOutlet = new SimpleOutlet<TMessage>(promisedPipe);

        return promisedPipe.Fulfill(new SwitchOutletPipe<TMessage>(inlet, leftOutlet, rightOutlet));
    }
}
```

### Golden Advice
FindSender and FindReceiver can be difficult to wrap your head around as the pipe system acts like a functional programming language. However, if you obey this advice, you should avoid most issues:

```c#
protected override Func<TMessage> FindSender(IOutlet<TMessage> outletReceivingMessage)
{
    // NEVER call outletReceivingMessage.FindReceiver() in this function.
    // The outlet should ask you for a sender if it needs another message.
    
    // Your aim, without calling the forbidden method, is
    // to pass as many messages through the pipe system as possible.
}

protected override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
{
    // NEVER call inletSendingMessage.FindSender() in this function.
    // The inlet should ask you for a receiver if it can send another message.
    
    // Your aim, without calling the forbidden method, is the same as in FindSender;
    // to pass as many messages through the pipe system as possible.
}
```

#### Efficiency Bonus
Consider the following implementation of the Capacity Pipe's FindReceiver:

```c#
protected override Action<TMessage> FindReceiver(IInlet<TMessage> inletSendingMessage)
{
    if (storedMessages.Any())
    {
        // Possibly Surprising Part
        if (HasSpareCapacity()) return message => storedMessages.Enqueue(message);
    }
    else
    {
        var receiver = Outlet.FindReceiver();
        if (receiver != null) return receiver;

        if (HasSpareCapacity()) return message => storedMessages.Enqueue(message);
    }
    return null;
}
```
The commented line might surprise you. Given the capacity pipe is holding messages, why doesn't it check if its outlet can receive any of them?

This is where the assumption that our pipe system forms a tree really comes to our rescue. The capacity pipe's outlet has not been touched by this method. As the pipe system is a tree, we know that the pipe system off of this outlet has not been modified at all, meaning it is already resolved.

Therefore, since the capacity pipe already has messages stored in it, our outlet could not receive a message previously

As nothing has changed, this means it still cannot receive a message. This allows the capacity pipe to know Outlet.FindReceiver() will return null without calling it. If the pipe system is large, avoiding this call can save a lot of time.
