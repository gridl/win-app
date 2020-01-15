﻿/*
 * Copyright (c) 2020 Proton Technologies AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.OS.Net.NetworkInterface;
using ProtonVPN.Service.Vpn;

namespace ProtonVPN.Service.Test.Vpn
{
    [TestClass]
    public class ObservableNetworkInterfacesTest
    {
        private INetworkInterfaces _interfaces;

        [TestInitialize]
        public void TestInitialize()
        {
            _interfaces = Substitute.For<INetworkInterfaces>();
        }

        [TestMethod]
        public void ShouldRaise_NetworkInterfacesAdded_WhenNewInterfaceAdded_AfterInitialization()
        {
            // Arrange
            var wasRaised = false;
            _interfaces.Interfaces().Returns(new INetworkInterface[] { new TestNetworkInterface("n1") });
            var subject = new ObservableNetworkInterfaces(_interfaces);
            subject.NetworkInterfacesAdded += (s, e) => wasRaised = true;
            // Act
            _interfaces.NetworkAddressChanged += Raise.Event();
            // Assert
            wasRaised.Should().BeTrue();
        }

        [TestMethod]
        public void ShouldRaise_NetworkInterfacesAdded_WhenNewInterfaceAdded()
        {
            // Arrange
            var wasRaised = false;
            _interfaces.Interfaces().Returns(new INetworkInterface[] { new TestNetworkInterface("n1") });
            var subject = new ObservableNetworkInterfaces(_interfaces);
            subject.NetworkInterfacesAdded += (s, e) => wasRaised = true;
            _interfaces.NetworkAddressChanged += Raise.Event();

            _interfaces.Interfaces().Returns(new INetworkInterface[] { new TestNetworkInterface("n1"), new TestNetworkInterface("n2") });
            wasRaised = false;

            // Act
            _interfaces.NetworkAddressChanged += Raise.Event();

            // Assert
            wasRaised.Should().BeTrue();
        }

        [TestMethod]
        public void ShouldRaise_NetworkInterfacesAdded_WhenNewInterfaceAdded_AndOldRemoved()
        {
            // Arrange
            var wasRaised = false;
            _interfaces.Interfaces().Returns(new INetworkInterface[] { new TestNetworkInterface("n1") });
            var subject = new ObservableNetworkInterfaces(_interfaces);
            subject.NetworkInterfacesAdded += (s, e) => wasRaised = true;
            _interfaces.NetworkAddressChanged += Raise.Event();

            _interfaces.Interfaces().Returns(new INetworkInterface[] { new TestNetworkInterface("n2") });
            wasRaised = false;

            // Act
            _interfaces.NetworkAddressChanged += Raise.Event();

            // Assert
            wasRaised.Should().BeTrue();
        }

        [TestMethod]
        public void ShouldNotRaise_NetworkInterfacesAdded_WhenSameNetworkInterfaces()
        {
            // Arrange
            var wasRaised = false;
            _interfaces.Interfaces().Returns(new INetworkInterface[] { new TestNetworkInterface("n1"), new TestNetworkInterface("n2") });
            var subject = new ObservableNetworkInterfaces(_interfaces);
            subject.NetworkInterfacesAdded += (s, e) => wasRaised = true;
            _interfaces.NetworkAddressChanged += Raise.Event();

            _interfaces.Interfaces().Returns(new INetworkInterface[] { new TestNetworkInterface("n1"), new TestNetworkInterface("n2") });
            wasRaised = false;

            // Act
            _interfaces.NetworkAddressChanged += Raise.Event();

            // Assert
            wasRaised.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldNotRaise_NetworkInterfacesAdded_WhenRemovedNetworkInterfaces()
        {
            // Arrange
            var wasRaised = false;
            _interfaces.Interfaces().Returns(new INetworkInterface[] { new TestNetworkInterface("n1"), new TestNetworkInterface("n2") });
            var subject = new ObservableNetworkInterfaces(_interfaces);
            subject.NetworkInterfacesAdded += (s, e) => wasRaised = true;
            _interfaces.NetworkAddressChanged += Raise.Event();

            _interfaces.Interfaces().Returns(new INetworkInterface[] { new TestNetworkInterface("n2") });
            wasRaised = false;

            // Act
            _interfaces.NetworkAddressChanged += Raise.Event();

            // Assert
            wasRaised.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldNotRaise_NetworkInterfacesAdded_WhenNoNetworkInterfaces()
        {
            // Arrange
            var wasRaised = false;
            _interfaces.Interfaces().Returns(new INetworkInterface[] { new TestNetworkInterface("n1") });
            var subject = new ObservableNetworkInterfaces(_interfaces);
            subject.NetworkInterfacesAdded += (s, e) => wasRaised = true;
            _interfaces.NetworkAddressChanged += Raise.Event();

            _interfaces.Interfaces().Returns(new INetworkInterface[] { });
            wasRaised = false;

            // Act
            _interfaces.NetworkAddressChanged += Raise.Event();

            // Assert
            wasRaised.Should().BeFalse();
        }

        [TestMethod]
        public void ShouldNotRaise_NetworkInterfacesAdded_WhenNoNewInterfaces_AfterNoNetworkInterfaces()
        {
            // Arrange
            var wasRaised = false;
            _interfaces.Interfaces().Returns(new INetworkInterface[] { new TestNetworkInterface("n1") });
            var subject = new ObservableNetworkInterfaces(_interfaces);
            subject.NetworkInterfacesAdded += (s, e) => wasRaised = true;
            _interfaces.NetworkAddressChanged += Raise.Event();

            _interfaces.Interfaces().Returns(new INetworkInterface[] { });
            _interfaces.NetworkAddressChanged += Raise.Event();

            _interfaces.Interfaces().Returns(new INetworkInterface[] { new TestNetworkInterface("n1") });
            wasRaised = false;

            // Act
            _interfaces.NetworkAddressChanged += Raise.Event();

            // Assert
            wasRaised.Should().BeFalse();
        }

        #region Helpers

        private class TestNetworkInterface : INetworkInterface
        {
            public TestNetworkInterface(string id)
            {
                Id = id;
            }

            public string Id { get; }
            public bool IsLoopback => false;
        }

        #endregion
    }
}
