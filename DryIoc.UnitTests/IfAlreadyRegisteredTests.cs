﻿using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class IfAlreadyRegisteredTests
    {
        [Test]
        public void By_default_appends_new_default_registration()
        {
            var container = new Container();
            container.Register<I, X>();
            container.Register<I, Y>();

            var services = container.Resolve<I[]>();

            CollectionAssert.AreEqual(
                new[] {typeof(X), typeof(Y) }, 
                services.Select(s => s.GetType()).ToArray());
        }

        public interface I { }
        public class X : I { }
        public class Y : I { }

        [Test]
        public void I_can_say_to_Throw_on_new_default_registration()
        {
            var container = new Container();
            container.Register<IService, Service>(ifAlreadyRegistered: IfAlreadyRegistered.Throw);

            var ex = Assert.Throws<ContainerException>(() =>
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Throw));

            Assert.AreEqual(ex.Error, Error.UnableToRegisterDuplicateDefault);
        }

        [Test]
        public void I_can_say_to_Throw_on_new_default_registration_when_multi_keyed_registrations_present()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: 1, ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IService, Service>(ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IService, AnotherService>(serviceKey: 2, ifAlreadyRegistered: IfAlreadyRegistered.Throw);

            var ex = Assert.Throws<ContainerException>(() =>
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Throw));

            Assert.AreEqual(ex.Error, Error.UnableToRegisterDuplicateDefault);
        }

        [Test]
        public void Can_update_registered_default_with_new_default()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Can_update_registered_named_with_new_named()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: EnumKey.Some);
            container.Register<IService, AnotherService>(serviceKey: EnumKey.Some, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var service = container.Resolve<IService>(EnumKey.Some);

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Can_update_latest_registered_default()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, OneService>();
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var services = container.Resolve<IService[]>();

            CollectionAssert.AreEqual(
                new[] { typeof(Service), typeof(AnotherService) },
                services.Select(service => service.GetType()).ToArray());
        }
    }
}