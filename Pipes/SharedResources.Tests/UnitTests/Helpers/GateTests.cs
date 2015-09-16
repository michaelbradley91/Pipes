using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using SharedResources.Helpers;

namespace SharedResources.Tests.UnitTests.Helpers
{
    [TestFixture]
    public class GateTests
    {
        private Gateway gateway;

        [SetUp]
        public void SetUp()
        {
            gateway = new Gateway();
        }

        [Test]
        public void Enter_ReturnsAUniqueGatePass()
        {
            // Act
            var gatePass1 = gateway.Enter();
            var gatePass2 = gateway.Enter();

            // Assert
            gatePass1.Should().NotBeNull();
            gatePass2.Should().NotBeNull();
            gatePass1.Should().NotBe(gatePass2);
        }

        [Test]
        public void Leave_GivenTheCorrectGatePass_AllowsYouToLeaveAndExpiresYourPass()
        {
            // Arrange
            var gatePass = gateway.Enter();

            // Act
            gateway.Leave(gatePass);

            // Assert
            gatePass.Expired.Should().BeTrue();
        }

        [ExpectedException(typeof(ArgumentException))]
        [Test]
        public void Leave_GivenAnExpiredGatePass_ThrowsAnArgumentException()
        {
            // Arrange
            var expiredPass = new Gateway.Pass { Expired = true };

            // Act
            gateway.Leave(expiredPass);
        }

        [Test]
        public void Close_GivenTheCorrectGatePass_AllowsYouToCloseTheGateAndMarksYourPass()
        {
            // Arrange
            var pass = gateway.Enter();

            // Act
            gateway.Close(pass);

            // Assert
            pass.ClosedTheGate.Should().BeTrue();
        }

        [Test]
        public void Close_GivenTheCorrectGatePass_LeavesTheGateClosedUntilYouLeave()
        {
            // Arrange
            var pass = gateway.Enter();
            var secondThreadEnteredGate = false;
            var thread = new Thread(() =>
            {
                gateway.Enter();
                secondThreadEnteredGate = true;
            });

            // Act
            gateway.Close(pass);
            thread.Start();
            Thread.Sleep(500);

            // Assert
            secondThreadEnteredGate.Should().BeFalse();

            // Act
            gateway.Leave(pass);
            Thread.Sleep(500);

            // Assert
            secondThreadEnteredGate.Should().BeTrue();
        }

        [Test]
        public void Close_GivenTwoThreadsClosedTheGate_LeavesTheGateClosedUntilBothThreadsHaveLeft()
        {
            // Arrange
            var pass1 = gateway.Enter();
            var pass2 = gateway.Enter();
            var thirdThreadEnteredGate = false;
            var thread = new Thread(() =>
            {
                gateway.Enter();
                thirdThreadEnteredGate = true;
            });

            // Act
            gateway.Close(pass1);
            gateway.Close(pass2);
            thread.Start();
            Thread.Sleep(500);

            // Assert
            thirdThreadEnteredGate.Should().BeFalse();

            // Act
            gateway.Leave(pass1);
            Thread.Sleep(500);

            // Assert
            thirdThreadEnteredGate.Should().BeFalse();

            // Act
            gateway.Leave(pass2);
            Thread.Sleep(500);

            // Assert
            thirdThreadEnteredGate.Should().BeTrue();
        }

    }
}
