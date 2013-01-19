﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Hosting;
using System.Web.Http.Dependencies;
using DandyDocSite.Infrastructure;
using Microsoft.Practices.ServiceLocation;
using StructureMap;
using IDependencyResolver = System.Web.Http.Dependencies.IDependencyResolver;
using DandyDoc.Overlays.MsdnLinks;

namespace DandyDocSite
{
	public static class StructureMapConfig
	{

		public static IContainer Init() {
			ObjectFactory.Initialize(Init);
			return ObjectFactory.Container;
		}

		private static void Init(IInitializationExpression x) {
			x.For<AssemblyCollectionGenerator>().Use(_ =>
				new AssemblyCollectionGenerator(
					HostingEnvironment.MapPath("~/bin/DandyDoc.Core.dll"),
					HostingEnvironment.MapPath("~/bin/DandyDoc.SimpleModels.dll"),
					HostingEnvironment.MapPath("~/bin/TestLibrary1.dll")
				)
			);
			x.For<ApiDocNavigation>().Use(c => new ApiDocNavigation(c.GetInstance<AssemblyCollectionGenerator>().GenerateDefinitions()));
			x.For<IMsdnLinkOverlay>().Use(_ => new MsdnDynamicLinkOverlay());
		}

		public class Scope : ServiceLocatorImplBase, IDependencyScope
		{

			public Scope(IContainer container) {
				if (null == container) throw new ArgumentNullException("container");
				Contract.EndContractBlock();
				Container = container;
			}

			public IContainer Container { get; private set; }

			public void Dispose() {
				Container.Dispose();
			}

			public IEnumerable<object> GetServices(Type serviceType) {
				return Container.GetAllInstances(serviceType).Cast<object>();
			}

			protected override IEnumerable<object> DoGetAllInstances(Type serviceType) {
				return GetServices(serviceType);
			}

			public override object GetService(Type serviceType) {
				if (null == serviceType)
					return null;
				try {
					return serviceType.IsAbstract || serviceType.IsInterface
						? Container.TryGetInstance(serviceType)
						: Container.GetInstance(serviceType);
				}
				catch {
					return null;
				}
			}

			protected override object DoGetInstance(Type serviceType, string key) {
				if (String.IsNullOrEmpty(key)) {
					return serviceType.IsAbstract || serviceType.IsInterface
						? Container.TryGetInstance(serviceType)
						: Container.GetInstance(serviceType);
				}
				return Container.GetInstance(serviceType, key);
			}
		}

		public class Resolver : Scope, IDependencyResolver
		{

			public Resolver(IContainer container)
				: base(container) {
				Contract.Requires(null != container);
			}

			public IDependencyScope BeginScope() {
				return new Resolver(Container.GetNestedContainer());
			}
		}

	}
}