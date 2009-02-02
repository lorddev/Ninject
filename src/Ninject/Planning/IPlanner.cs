﻿using System;
using System.Collections.Generic;
using Ninject.Components;
using Ninject.Planning.Strategies;

namespace Ninject.Planning
{
	public interface IPlanner : INinjectComponent
	{
		IList<IPlanningStrategy> Strategies { get; }
		IPlan GetPlan(Type type);
	}
}