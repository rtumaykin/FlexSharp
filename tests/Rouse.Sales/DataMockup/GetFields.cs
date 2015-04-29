using System;
using System.Linq;

namespace Sample.DataMockup
{
    public static class GetFields
    {
        private static readonly Field[] Client1Fields = {
            new Field{Name = "EquipmentNumber", DataType = "System.String"},
            new Field{Name = "YearPurchased", DataType = "System.Int32"},
            new Field{Name = "MakeModel", DataType = "System.String"},
            new Field{Name = "Cost", DataType = "System.Decimal"},
            new Field{Name = "HoursUsedLast12Months", DataType = "System.Int32"},
            new Field{Name = "GarageStorage", DataType = "System.Boolean"},
            new Field{Name = "State", DataType = "System.String"}
        };

        private static readonly Field[] Client2Fields = {
            new Field{Name = "EquipNo", DataType = "System.String"},
            new Field{Name = "AcqDate", DataType = "System.DateTime"},
            new Field{Name = "Make", DataType = "System.String"},
            new Field{Name = "Model", DataType = "System.String"},
            new Field{Name = "PaidAmount", DataType = "System.Decimal"},
            new Field{Name = "Color", DataType = "System.String"},
            new Field{Name = "HasAirConditioner", DataType = "System.Boolean"},
            new Field{Name = "WheelsCount", DataType = "System.Int32"},
            new Field{Name = "DivisionCode", DataType = "System.String"}
        };

        public static Field[] GetClientFields(int clientId)
        {
            if (new[] {1, 2}.Contains(clientId))
                return clientId == 1 ? Client1Fields : Client2Fields;

            throw new Exception(string.Format("Invalid ClientId {0}", clientId));
        }
    }
}
