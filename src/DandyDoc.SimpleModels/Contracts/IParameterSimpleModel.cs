﻿using System.Collections.Generic;

namespace DandyDoc.SimpleModels.Contracts
{
	public interface IParameterSimpleModel
	{

		string DisplayName { get; }

		bool HasSummary { get; }

		IComplexTextNode Summary { get; }

		ISimpleMemberPointerModel Type { get; }

		bool HasFlair { get; }

		IList<IFlairTag> Flair { get; }

	}
}