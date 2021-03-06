﻿using System;
using System.Diagnostics.Contracts;

#pragma warning disable 1591,0067,0169,0649

namespace TestLibrary1
{
    /// <summary>
    /// A class uses to test code contracts.
    /// </summary>
    public class ClassWithContracts
    {

        /// <summary>
        /// Constructor showing a legacy requires.
        /// </summary>
        /// <param name="text"></param>
        public ClassWithContracts(string text) {
            if (String.IsNullOrEmpty(text)) throw new ArgumentException("Nope!", "text");
            if (text.Equals("nope")) throw new ArgumentException("Was nope...", "text");
            Contract.EnsuresOnThrow<ArgumentException>(Text == null);
            Contract.EnsuresOnThrow<ArgumentException>(Text != "nope!");
            Contract.EndContractBlock();
            Text = text;
        }

        /// <summary>
        /// Auto-property with an invariant.
        /// </summary>
        [Pure]
        public string Text { get; private set; }

        /// <summary>
        /// A pure method that ensures on return.
        /// </summary>
        /// <returns>stuff</returns>
        [Pure]
        public string SomeStuff() {
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            return "stuff";
        }

        public string Stuff {
            get {
                Contract.Ensures(Contract.Result<string>() != null);
                return "stuff";
            }
            set {
                Contract.Requires(value != null);
                throw new NotImplementedException();
            }
        }

        public string SameStuff {
            [Pure]
            get {
                Contract.Ensures(null != Contract.Result<string>());
                return "stuff";
            }
        }

        public string OneButNotTheOther {
            get { throw new NotImplementedException(); }
            set {
                Contract.Requires(value != null);
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// A non implemented property.
        /// </summary>
        /// <exception cref="System.NotSupportedException">Not supported!</exception>
        public string NotImplemented {
            get {
                if (SameStuff == "stuff") throw new InvalidOperationException();
                Contract.EndContractBlock();
                throw new NotImplementedException();
            }
            set {
                if (value == "Nope!") throw new Exception("NOPE!");
                Contract.EndContractBlock();
            }
        }

        [ContractInvariantMethod]
        private void CodeContractInvariant() {
            Contract.Invariant(!String.IsNullOrEmpty(Text));
        }

    }
}
