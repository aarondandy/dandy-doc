﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Xml;
using DandyDoc.Overlays.Cref;
using DandyDoc.Utility;
using Mono.Cecil;

namespace DandyDoc.Overlays.XmlDoc
{
	public class DelegateTypeDefinitionXmlDoc : TypeDefinitionXmlDoc
	{

		internal DelegateTypeDefinitionXmlDoc(TypeDefinition typeDefinition, XmlNode xmlNode, CrefOverlay crefOverlay)
			: base(typeDefinition, xmlNode, crefOverlay)
		{
			Contract.Requires(null != typeDefinition);
			Contract.Requires(null != xmlNode);
			Contract.Requires(null != crefOverlay);
		}

		public ParsedXmlElementBase DocsForParameter(string name) {
			if (String.IsNullOrEmpty(name)) throw new ArgumentException("Invalid parameter name.", "name");
			Contract.EndContractBlock();
			return ParameterizedXmlDocBase.DocsForParameter(name, this);
		}

		public ParsedXmlElementBase Returns { get { return SelectParsedXmlNode("returns") as ParsedXmlElementBase; } }

		public IList<ParsedXmlException> Exceptions {
			get{
				var nodes = SelectParsedXmlNodes("exception");
				return null == nodes ? null : nodes.ConvertAll(n => (ParsedXmlException)n);
			}
		}

	}
}
