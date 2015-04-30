using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sample.DataMockup
{
    public static class DataEmulator
    {
        private static readonly Field[] Client1Fields =
        {
            new Field {Name = "EquipmentNumber", DataType = "System.String"},
            new Field {Name = "YearPurchased", DataType = "System.Int32"},
            new Field {Name = "MakeModel", DataType = "System.String"},
            new Field {Name = "Cost", DataType = "System.Decimal"},
            new Field {Name = "HoursUsedLast12Months", DataType = "System.Int32"},
            new Field {Name = "GarageStorage", DataType = "System.Boolean"},
            new Field {Name = "State", DataType = "System.String"}
        };

        private static readonly Field[] Client2Fields =
        {
            new Field {Name = "EquipNo", DataType = "System.String"},
            new Field {Name = "AcqDate", DataType = "System.DateTime"},
            new Field {Name = "Make", DataType = "System.String"},
            new Field {Name = "Model", DataType = "System.String"},
            new Field {Name = "PaidAmount", DataType = "System.Decimal"},
            new Field {Name = "Color", DataType = "System.String"},
            new Field {Name = "HasAirConditioner", DataType = "System.Boolean"},
            new Field {Name = "WheelsCount", DataType = "System.Int32"},
            new Field {Name = "DivisionCode", DataType = "System.String"}
        };

        public static Field[] GetClientFields(int clientId)
        {
            if (new[] {1, 2}.Contains(clientId))
                return clientId == 1 ? Client1Fields : Client2Fields;

            throw new Exception(string.Format("Invalid ClientId {0}", clientId));
        }

        private static readonly Variable[] Client1VariablesCollection = new Variable[]
        {
            new Variable
            {
                DataType = "System.Boolean",
                Formula = "return Asset.YearPurchased < DateTime.Now.Year - 5 && Asset.Cost > 5000 && Asset.HoursUsedLast12Months < 100;",
                Name = "ForSaleFlag",
                EvaluationOrder = 1
            }, 
            new Variable
            {
                DataType = "System.String",
                Name = "Make",
                Formula = "return Asset.MakeModel.Split('/')[0];",
                EvaluationOrder = 2
            }, 
            new Variable
            {
                DataType = "System.String",
                Name = "Model",
                Formula = "return Asset.MakeModel.Split('/')[1];",
                EvaluationOrder = 3
            },
            new Variable
            {
                DataType = "System.Boolean",
                Name = "Featured",
                Formula = "return Asset.GarageStorage == true && Variables.Make == \"Toyota\";",
                EvaluationOrder = 4
            }
        };

        private static readonly Variable[] Client2VariablesCollection = new Variable[]
        {
            new Variable
            {
                DataType = "System.Int32",
                Name = "YearPurchased",
                EvaluationOrder = 1,
                Formula = "return Asset.AcqDate.HasValue ? Asset.AcqDate.Value.Year : -1;"
            }, 
            new Variable
            {
                DataType = "System.Boolean",
                Formula = "return Variables.YearPurchased < DateTime.Now.Year - 5 && Asset.PaidAmount > 5000 && Asset.HasAirConditioner == true;",
                Name = "ForSaleFlag",
                EvaluationOrder = 2
            }, 
            new Variable
            {
                DataType = "System.Boolean",
                Name = "Featured",
                Formula = "return Asset.WheelsCount == 4 && Asset.Color == \"Black\";",
                EvaluationOrder = 3
            }
        };

        public static Variable[] GetVariablesCollection(int clientId)
        {
            return clientId == 1 ? Client1VariablesCollection : Client2VariablesCollection;
        }
    }
}
