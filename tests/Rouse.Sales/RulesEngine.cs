using System;
using System.Collections.Generic;

namespace Sample
{
    public abstract class RulesEngine
    {
        /// <summary>
        /// This object will be
        /// </summary>
        protected IDictionary<string, string> Data
        {
            get;
            private set;
        }
        public object Compute(object initializationData)
        {
            Initialize(initializationData);
            return ComputeInternal();
        }
        protected abstract object ComputeInternal();

        private void Initialize(object initializationData)
        {
            Data = initializationData as IDictionary<string, string>;
            if (Data == null)
                throw new Exception("Wrong type");
        }
    }
}
