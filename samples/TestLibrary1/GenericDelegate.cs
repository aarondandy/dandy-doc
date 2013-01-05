﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary1
{
	/// <summary>
	/// A generic delegate.
	/// </summary>
	/// <typeparam name="TA">The type of the value.</typeparam>
	/// <param name="a">The value.</param>
	/// <returns>The result.</returns>
	public delegate TA GenericDelegate<TA>(TA a);
}
