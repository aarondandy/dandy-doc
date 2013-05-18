﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.CRef;
using DandyDoc.XmlDoc;

namespace DandyDoc.CodeDoc
{
    /// <summary>
    /// A code doc field model.
    /// </summary>
    public class CodeDocField : CodeDocMemberContentBase, ICodeDocValueMember
    {

        /// <summary>
        /// Creates a new model for a field member.
        /// </summary>
        /// <param name="cRef">The code reference of this member.</param>
        public CodeDocField(CRefIdentifier cRef) : base(cRef){
            Contract.Requires(cRef != null);
        }

        /// <inheritdoc/>
        public ICodeDocMember ValueType { get; set; }

        [Obsolete]
        public bool HasValueDescription{
            get {
                var valueDesc = ValueDescription;
                return valueDesc != null && valueDesc.HasChildren;
            }
        }

        [Obsolete]
        public XmlDocElement ValueDescription { get { return XmlDocs == null ? null : XmlDocs.ValueElement; } }

        /// <inheritdoc/>
        public bool HasValueDescriptionContents { get { return ValueDescriptionContents != null && ValueDescriptionContents.Count > 0; } }

        /// <inheritdoc/>
        public IList<XmlDocNode> ValueDescriptionContents {
            /*get {
                Contract.Ensures(Contract.Result<IList<XmlDocNode>>() != null);
                return XmlDocs != null && XmlDocs.HasValueContents
                    ? XmlDocs.ValueContents
                    : new XmlDocNode[0];
            }*/
            get; set; }

        /// <summary>
        /// Indicates that this field is a literal value.
        /// </summary>
        public bool IsLiteral { get; set; }

        /// <summary>
        /// Indicates that this field is an init-only field.
        /// </summary>
        public bool IsInitOnly { get; set; }

    }
}
