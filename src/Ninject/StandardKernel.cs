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
using Ninject.Activation;
using Ninject.Activation.Caching;
using Ninject.Activation.Strategies;
using Ninject.Injection;
using Ninject.Injection.Linq;
using Ninject.Modules;
using Ninject.Planning;
using Ninject.Planning.Strategies;
using Ninject.Selection;
using Ninject.Selection.Heuristics;
#endregion

namespace Ninject
{
	/// <summary>
	/// The standard implementation of a kernel.
	/// </summary>
	public class StandardKernel : KernelBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StandardKernel"/> class.
		/// </summary>
		/// <param name="modules">The modules to load into the kernel.</param>
		public StandardKernel(params IModule[] modules) : base(modules) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardKernel"/> class.
		/// </summary>
		/// <param name="settings">The configuration to use.</param>
		/// <param name="modules">The modules to load into the kernel.</param>
		public StandardKernel(INinjectSettings settings, params IModule[] modules) : base(settings, modules) { }

		/// <summary>
		/// Adds components to the kernel during startup.
		/// </summary>
		protected override void AddComponents()
		{
			Components.Add<IPipeline, Pipeline>();
			Components.Add<IActivationStrategy, PropertyInjectionStrategy>();
			Components.Add<IActivationStrategy, MethodInjectionStrategy>();
			Components.Add<IActivationStrategy, InitializableStrategy>();
			Components.Add<IActivationStrategy, StartableStrategy>();
			Components.Add<IActivationStrategy, BindingActionStrategy>();
			Components.Add<IActivationStrategy, DisposableStrategy>();

			Components.Add<ICache, Cache>();
			Components.Add<ICachePruner, CachePruner>();

			Components.Add<IPlanner, Planner>();
			Components.Add<IPlanningStrategy, ConstructorReflectionStrategy>();
			Components.Add<IPlanningStrategy, PropertyReflectionStrategy>();
			Components.Add<IPlanningStrategy, MethodReflectionStrategy>();

			Components.Add<ISelector, Selector>();
			Components.Add<IConstructorScorer, StandardConstructorScorer>();
			Components.Add<IPropertyInjectionHeuristic, StandardPropertyInjectionHeuristic>();
			Components.Add<IMethodInjectionHeuristic, StandardMethodInjectionHeuristic>();

			Components.Add<IInjectorFactory, InjectorFactory>();
			Components.Add<IModuleLoader, ModuleLoader>();
		}
	}
}
