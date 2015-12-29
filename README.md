Pipes
=====
Disclaimer
----------
This package is pretty new so it might have bugs in it. It is thoroughly tested (when this was written - 226 tests!) but as this project concerns parallel programming, it is hard to test enough!

If you find a bug, please email michael.bradley@hotmail.co.uk, preferably with the simplest code you can manage to reproduce the problem.

Features That Might Be Coming
-----------------------------

* Generalised versions of the pipes (n inlets / m outlets).
* Repeater pipe that repeats a message n times.
* Filter pipe to automatically "swallow" certain messages.

Please email michael.bradley@hotmail.co.uk if you'd like something specific. Thanks!

Summary
-------

**Pipes** is a concurrency abstraction designed to simplify communication and synchronisation between threads.

Concurrency is about completing multiple tasks at the same time. As more and more devices have multiple cores to exploit, parallel programming is set to become the norm! However, the tools we have now often feel unintuitive, produce convoluted systems and are hard to reason about.

Pipes aims to simplify this. Processes communicate and synchronise with each other by sending messages through (thread-safe) pipes. To illustrate:

**Thread 1:**
<pre><code>//... do some work
var message = pipe.Outlet.Receive(); //blocks until thread 2 is read to send the message
//... do more work
</code></pre>

**Thread 2:**
<pre><code>//... do some work
pipe.Inlet.Send(message); //blocks until thread 1 is ready to receive the message
//... do more work
</code></pre>

The real power of pipes can be better seen when pipes have multiple inlets / outlets. You can:
* Create your own pipes easily. (For example, see if you can understand how the [capacity pipe](https://github.com/michaelbradley91/Pipes/blob/master/Pipes/Pipes/Models/Pipes/CapacityPipe.cs) was implemented.)
* Create your own inlets / outlets.
* Connect multiple pipes together to form a pipe system!

This project is written in C# and is available as a Nuget Package with ID Parallel.Pipes, so it's super easy to get started! Read more below to learn how to use them.

Pipes was originally inspired by CSO developed by Bernard Sufrin at Oxford University. You can learn more about CSO [here](http://www.cs.ox.ac.uk/people/bernard.sufrin/CSO/cso-doc-scala2.11.4/index.html#ox.CSO$). However, Pipes' goals have deviated from CSO a little - there is a much stronger focus on making it extensible - so you have the power to do whatever you need to without worrying about thread management at all!

Code Examples
-------------

### Getting Started
The life of a pipe begins with the **PipeBuilder**. You specify the pipe you would like using a fluent syntax. some examples:

<pre>
<code>
var basicPipe = PipeBuilder.New.BasicPipe<int>().Build();
var capacityPipe = PipeBuilder.New.CapacityPipe<string>().WithCapacity(100).Build();
var eitherInletPipe = PipeBuilder.New.EitherInletPipe<bool>().WithPrioritisingTieBreaker(Priority.Right).Build();
</code>
</pre>
