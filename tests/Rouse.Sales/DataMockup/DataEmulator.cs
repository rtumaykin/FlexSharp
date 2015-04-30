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
                Formula = "return Asset.GarageStorage && Variables.Make == \"Toyota\";",
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
                Formula = "return Asset.AcqDate.Year;"
            }, 
            new Variable
            {
                DataType = "System.Boolean",
                Formula = "return Variables.YearPurchased < DateTime.Now.Year - 5 && Asset.PaidAmount > 5000 && Asset.HasAirConditioner;",
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


        public static Dictionary<string, string> GetSampleData(int clientId)
        {
            return clientId == 1 ? GetSampleData1() : GetSampleData2();
        }

        private static Dictionary<string, string> GetSampleData1()
        {
            var makeModel = GetMakeModel();
            return new Dictionary<string, string>
            {
                {"EquipmentNumber", RandomString(15)},
                {"YearPurchased", new Random().Next(1997, 2015).ToString()},
                {"MakeModel", string.Format("{0}/{1}", makeModel.Item1, makeModel.Item2)},
                {"Cost", new Random().Next(1000, 30000).ToString()},
                {"HoursUsedLast12Months", new Random().Next(10, 1000).ToString()},
                {"GarageStorage", (new Random().Next(0, 1) == 0).ToString()},
                {"State", new Random().Next(0, 1) == 0 ? "CA" : "AZ"}
            };
        }

        private static Dictionary<string, string> GetSampleData2()
        {
            var makeModel = GetMakeModel();
            return new Dictionary<string, string>
            {
                {"EquipNo", RandomString(15)},
                {"AcqDate", RandomDay().ToString("yyyy/MM/dd")},
                {"Make", makeModel.Item1},
                {"Model", makeModel.Item2},
                {"PaidAmount", new Random().Next(1000, 30000).ToString()},
                {"Color", new Random().Next(0, 1) == 0 ? "Black" : "Red"},
                {"HasAirConditioner", (new Random().Next(0, 1) == 0).ToString()},
                {"WheelsCount", (new Random().Next(1, 4)*2).ToString()},
                {"DivisionCode", new Random().Next(0, 1) == 0 ? "CA1" : "AZ10"}
            };
        }

        private static DateTime RandomDay()
        {
            var start = new DateTime(1995, 1, 1);
            var gen = new Random();

            var range = (DateTime.Today - start).Days;
            return start.AddDays(gen.Next(range));
        }

        private static Random random = new Random((int)DateTime.Now.Ticks);

        private static string RandomString(int size)
        {
            var builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        private static Tuple<string, string> GetMakeModel()
        {
            var make = new Random().Next(0, 1) == 0 ? "Toyota" : "Honda";
            var model = MakeModels[make].ToArray()[new Random().Next(0, 2)];

            return new Tuple<string, string>(make, model);
        }

        private static Dictionary<string, List<string>> MakeModels = new Dictionary<string, List<string>>
        {
            {"Toyota", new List<string>{"Camry", "Corolla", "4Runner"}},
            {"Honda", new List<string>{"Odyssey", "Civic", "Accord"}}
        }; 
    }
}
