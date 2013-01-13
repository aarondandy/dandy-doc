﻿using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface ISimpleModelMembersCollection
	{

		IList<ITypeSimpleModel> Types { get; }

		IList<IDelegateSimpleModel> Delegates { get; }

		// Methods

		// Properties

		// Fields

		// Events

	}
}
