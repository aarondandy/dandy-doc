﻿using System.Diagnostics.Contracts;
using Mono.Cecil;

namespace DandyDoc.Cecil
{
    /// <summary>
    /// An assembly resolver designed to load all meta-data ahead of time using
    /// <see cref="Mono.Cecil.ReadingMode.Immediate" />.
    /// </summary>
    /// <remarks>
    /// This resolver is useful in situations that require thread safety
    /// when using a version of Mono Cecil that is not thread safe.
    /// Be warned however that this will cause Cecil to be very slow.
    /// </remarks>
    /// <seealso cref="DandyDoc.CodeDoc.ThreadSafeCodeDocRepositoryWrapper"/>
    public class CecilImmediateAssemblyResolver : IAssemblyResolver
    {

        public static readonly CecilImmediateAssemblyResolver Default
            = new CecilImmediateAssemblyResolver();

        public static ReaderParameters CreateReaderParameters() {
            return new ReaderParameters(ReadingMode.Immediate) {
                AssemblyResolver = Default
            };
        }

        public CecilImmediateAssemblyResolver() {
            Core = new DefaultAssemblyResolver();
            ImmediateParams = new ReaderParameters(ReadingMode.Immediate) {
                AssemblyResolver = this
            };
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Core != null);
            Contract.Invariant(ImmediateParams != null);
        }

        public IAssemblyResolver Core { get; private set; }

        public ReaderParameters ImmediateParams { get; private set; }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters) {
            return Core.Resolve(fullName, parameters);
        }

        public AssemblyDefinition Resolve(string fullName) {
            return Core.Resolve(fullName, ImmediateParams);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) {
            return Core.Resolve(name, parameters);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name) {
            return Core.Resolve(name, ImmediateParams);
        }
    }
}
