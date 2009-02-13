﻿#region License
// Author: Nate Kohari <nkohari@gmail.com>
// Copyright (c) 2007-2009, Enkari, Ltd.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
#region Using Directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject.Infrastructure;
using Ninject.Infrastructure.Disposal;
using Ninject.Infrastructure.Language;
#endregion

namespace Ninject.Components
{
	/// <summary>
	/// An internal container that manages and resolves components that contribute to Ninject.
	/// </summary>
	public class ComponentContainer : DisposableObject, IComponentContainer
	{
		private readonly Multimap<Type, Type> _mappings = new Multimap<Type, Type>();
		private readonly Dictionary<Type, INinjectComponent> _instances = new Dictionary<Type, INinjectComponent>();

		/// <summary>
		/// Gets or sets the kernel that owns the component container.
		/// </summary>
		public IKernel Kernel { get; set; }

		/// <summary>
		/// Releases resources held by the object.
		/// </summary>
		public override void Dispose()
		{
			foreach (INinjectComponent instance in _instances.Values)
				instance.Dispose();

			_mappings.Clear();
			_instances.Clear();

			base.Dispose();
		}

		/// <summary>
		/// Registers a service in the container.
		/// </summary>
		/// <typeparam name="TService">The component's service type.</typeparam>
		/// <typeparam name="TImplementation">The component's implementation type.</typeparam>
		public void Add<TService, TImplementation>()
			where TService : INinjectComponent
			where TImplementation : TService, INinjectComponent
		{
			_mappings.Add(typeof(TService), typeof(TImplementation));
		}

		/// <summary>
		/// Removes all registrations for the specified service.
		/// </summary>
		/// <typeparam name="T">The component's service type.</typeparam>
		public void RemoveAll<T>()
			where T : INinjectComponent
		{
			RemoveAll(typeof(T));
		}

		/// <summary>
		/// Removes all registrations for the specified service.
		/// </summary>
		/// <param name="service">The component's service type.</param>
		public void RemoveAll(Type service)
		{
			foreach (Type implementation in _mappings[service])
			{
				if (_instances.ContainsKey(implementation))
					_instances[implementation].Dispose();

				_instances.Remove(implementation);
			}

			_mappings.RemoveAll(service);
		}

		/// <summary>
		/// Gets one instance of the specified component.
		/// </summary>
		/// <typeparam name="T">The component's service type.</typeparam>
		/// <returns>The instance of the component.</returns>
		public T Get<T>()
			where T : INinjectComponent
		{
			return (T) Get(typeof(T));
		}

		/// <summary>
		/// Gets all available instances of the specified component.
		/// </summary>
		/// <typeparam name="T">The component's service type.</typeparam>
		/// <returns>A series of instances of the specified component.</returns>
		public IEnumerable<T> GetAll<T>()
			where T : INinjectComponent
		{
			return GetAll(typeof(T)).Cast<T>();
		}

		/// <summary>
		/// Gets one instance of the specified component.
		/// </summary>
		/// <param name="service">The component's service type.</param>
		/// <returns>The instance of the component.</returns>
		public object Get(Type service)
		{
			if (service == typeof(IKernel))
				return Kernel;

			if (service.IsGenericType)
			{
				Type gtd = service.GetGenericTypeDefinition();
				Type argument = service.GetGenericArguments()[0];

				if (gtd.IsInterface && typeof(IEnumerable<>).IsAssignableFrom(gtd))
					return LinqReflection.CastSlow(GetAll(argument), argument);
			}

			Type implementation = _mappings[service].FirstOrDefault();

			if (implementation == null)
				throw new InvalidOperationException(String.Format("No component of type {0} has been registered", service));

			return ResolveInstance(implementation);
		}

		/// <summary>
		/// Gets all available instances of the specified component.
		/// </summary>
		/// <param name="service">The component's service type.</param>
		/// <returns>A series of instances of the specified component.</returns>
		public IEnumerable<object> GetAll(Type service)
		{
			foreach (Type implementation in _mappings[service])
				yield return ResolveInstance(implementation);
		}

		private object ResolveInstance(Type type)
		{
			return _instances.ContainsKey(type) ? _instances[type] : CreateNewInstance(type);
		}

		private object CreateNewInstance(Type type)
		{
			ConstructorInfo constructor = SelectConstructor(type);
			var arguments = constructor.GetParameters().Select(parameter => Get(parameter.ParameterType)).ToArray();

			try
			{
				var component = constructor.Invoke(arguments) as INinjectComponent;
				component.Kernel = Kernel;
				_instances.Add(type, component);

				return component;
			}
			catch (TargetInvocationException ex)
			{
				ex.RethrowInnerException();
				return null;
			}
		}

		private ConstructorInfo SelectConstructor(Type type)
		{
			var constructor = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();

			if (constructor == null)
				throw new NotSupportedException(String.Format("Couldn't resolve a constructor to create instance of type {0}", type));

			return constructor;
		}
	}
}