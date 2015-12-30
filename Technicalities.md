Technicalities
==============
[Click to return to Pipes](README.md)

This is a very technical section and not worth reading unless you know pipes very well or you are experiencing an unusual problem.
You don't need to read this to get started!

Restriction of Pipe Systems to Trees
------------------------------------

If you start building complex pipe systems, you'll probably find its restriction that your system forms a tree a bit unreasonable. Obviously, your pipe system is unlikely to make sense if you include a cycle, but it is possible your pipe system still forms a DAG (directed acyclic graph).

The reason for this is the splitting pipe. The splitting pipe needs to know that the receivers on its left outlet and right outlet will not disappear. Consider the following:
* A sender arrives.
* The splitting pipe sees that there is a receiver on its left outlet and a receiver on its right outlet.
* The splitting pipe forwards the message to the left outlet.
* The splitting pipe then tries to forward the message to the right outlet, but the right outlet no longer has a receiver!!

This is deeply confusing for the poor splitting pipe. The pipe system is considered "locked" during the bullets above, so a receiver should not have been able to leave. However, if the left and right receivers are really the "same" receiver (the same thread), the splitting pipe won't know what to do. This happens if you connect the two outlets of a splitting pipe to the two inlets of an either inlet pipe for example.

After attempting a few workarounds, it turned out this is actually quite a pain to solve. Therefore, the restriction that the pipe system forms a tree was put in place.

After considering a number of common use-cases, it seems a tree is sufficient for almost any situation. However, if you desperately need to form a DAG the pipe system will **probably** still work so long as the splitting pipe's receivers are known to be different threads. However, I'd recommend you avoid hacking around with this unless you're extremely confident you understand pipes.


