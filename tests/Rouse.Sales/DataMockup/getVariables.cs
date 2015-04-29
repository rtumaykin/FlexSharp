using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.DataMockup
{
    public static class GetVariables
    {
        private static readonly Variable[] Client1VariablesCollection = new Variable[]
        {
            new Variable
            {
                DataType = "System.String",
                Formula = "return \"Hi!\"",
                Name = "DisplayName",
                EvaluationOrder = 1
            }, 
            new Variable(), 
            new Variable()
        };

        private static readonly Variable[] Client2VariablesCollection = new Variable[]
        {
            new Variable(), new Variable(), new Variable()
        };
        public static Variable[] GetVariablesCollection(int clientId)
        {
            return clientId == 1 ? Client1VariablesCollection : Client2VariablesCollection;
        }

    }
}
