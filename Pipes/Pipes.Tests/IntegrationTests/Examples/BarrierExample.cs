using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Pipes.Builders;
using Pipes.Models.Lets;

namespace Pipes.Tests.IntegrationTests.Examples
{
    [TestFixture]
    public class BarrierExample
    {
        private const int NumberOfWorkers = 20;

        private Thread[] workerThreads;
        private int[] sharedMemory;
        private ISimpleOutlet<int>[] outlets; 

        private ISimpleInlet<int> barrierInlet;

        [SetUp]
        public void SetUp()
        {
            workerThreads = new Thread[NumberOfWorkers];
            sharedMemory = new int[NumberOfWorkers];

            var tree = CreateBinaryTreeOfSplittingPipes(NumberOfWorkers);
            barrierInlet = tree.Item1;
            outlets = tree.Item2;

            for (var i = 0; i < NumberOfWorkers; i++)
            {
                workerThreads[i] = new Thread(CreateWorker(i));
            }
        }

        private static Tuple<ISimpleInlet<int>, ISimpleOutlet<int>[]> CreateBinaryTreeOfSplittingPipes(int numberOfOutlets)
        {
            if (numberOfOutlets < 2) throw new InvalidOperationException();
            
            var outletQueue = new Queue<ISimpleOutlet<int>>();

            var firstSplittingPipe = PipeBuilder.New.SplittingPipe<int>().Build();
            var inlet = firstSplittingPipe.Inlet;
            outletQueue.Enqueue(firstSplittingPipe.LeftOutlet);
            outletQueue.Enqueue(firstSplittingPipe.RightOutlet);

            while (outletQueue.Count < numberOfOutlets)
            {
                var outlet = outletQueue.Dequeue();
                var splittingPipe = PipeBuilder.New.SplittingPipe<int>().Build();
                splittingPipe.Inlet.ConnectTo(outlet);
                outletQueue.Enqueue(splittingPipe.LeftOutlet);
                outletQueue.Enqueue(splittingPipe.RightOutlet);
            }

            var outletArray = outletQueue.ToArray();
            return new Tuple<ISimpleInlet<int>, ISimpleOutlet<int>[]>(inlet, outletArray);
        }

        private ThreadStart CreateWorker(int workerId)
        {
            return () =>
            {
                var numberOfRuns = 0;
                while (true)
                {
                    Thread.Sleep(new Random().Next(0, 100));
                    sharedMemory[workerId] = numberOfRuns;
                    numberOfRuns++;
                    outlets[workerId].Receive();
                    outlets[workerId].Receive();
                }
                // ReSharper disable once FunctionNeverReturns
            };
        }

        [Test]
        public void TestBarrier()
        {
            foreach (var workerThread in workerThreads)
            {
                workerThread.Start();
            }

            for (var i = 0; i < 100; i++)
            {
                barrierInlet.Send(0);
                sharedMemory.All(s => s == i).Should().BeTrue();
                barrierInlet.Send(0);
            }
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var workerThread in workerThreads)
            {
                workerThread.Abort();
            }
        }
    }
}
