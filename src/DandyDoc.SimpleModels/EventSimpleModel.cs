﻿using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;
using Mono.Cecil;

namespace DandyDoc.SimpleModels
{
	public class EventSimpleModel : DefinitionMemberSimpleModelBase<EventDefinition>, IEventSimpleModel
	{

		public EventSimpleModel(EventDefinition definition, ITypeSimpleModel declaringModel)
			: base(definition, declaringModel)
		{
			Contract.Requires(definition != null);
			Contract.Requires(declaringModel != null);
		}

		public override string SubTitle { get { return "Event"; } }

		public ISimpleMemberPointerModel DelegateType {
			get {
				return new ReferenceSimpleMemberPointer(FullTypeDisplayNameOverlay.GetDisplayName(Definition.EventType), Definition.EventType);
			}
		}

	}
}