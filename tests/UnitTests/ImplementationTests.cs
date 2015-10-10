using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotAssembly;
using NUnit.Framework;
using Sample;

namespace UnitTests
{
    [TestFixture]
    public class ImplementationTests
    {
        [Test]
        public void GetCode()
        {
            var start1 = DateTime.Now;

            var pfp = new FilePersistenceProvider();
            var cf1 = new Sample.SampleCodeFactory(pfp, new CodeFactoryInitializationModel { ClientId = 1 });
            var code1 = cf1.GetCodeInternal();
            var cfres1 = cf1.CompileAndSave("client1");

            Assert.That(cfres1.Errors.Count == 0,
                "There were {0} errors during the compilation. First one was {1}.\rCode: \r{2}",
                cfres1.Errors.Count, cfres1.Errors.Count > 0 ? cfres1.Errors[0].ErrorText : "", code1);
            
            var cf2 = new Sample.SampleCodeFactory(pfp, new CodeFactoryInitializationModel { ClientId = 2 });
            var code2 = cf2.GetCodeInternal();
            var cfres2 = cf2.CompileAndSave("client2");

            Assert.That(cfres2.Errors.Count == 0, "There were {0} errors during the compilation. First one was {1}.\rCode: \r{2}",
                cfres2.Errors.Count, cfres2.Errors.Count > 0 ? cfres2.Errors[0].ErrorText : "", code2);

            var ha = new HotAssembly.InstantiatorFactory<Sample.RulesEngine>(pfp);
            ha.Instantiate("client1");
            ha.Instantiate("client2");
            Debug.WriteLine($"Took {DateTime.Now.Subtract(start1).TotalMilliseconds} ms to prepare all");

            var start = DateTime.Now;
            for (var i = 0; i < 1000000; i++)
            {
                var data1 = DataEmulator.GetSampleData(1);
                var i1 = ha.Instantiate("client1");
                var res1 = i1.Compute(data1);

                var data2 = DataEmulator.GetSampleData(2);
                var res2 = ha.Instantiate("client2").Compute(data2);
            }
            Debug.WriteLine($"Took {DateTime.Now.Subtract(start).TotalMilliseconds} ms for {1000000} cycles");
        }

        [Test]
        public void Should_Successfully_Check_Syntax()
        {
            var pfp = new FilePersistenceProvider();
            var cf1 = new Sample.SampleCodeFactory(pfp, new CodeFactoryInitializationModel { ClientId = 1 });
            var code1 = cf1.GetCodeInternal();
            var cfres1 = cf1.CheckSyntax();

            Assert.That(cfres1.Errors.Count == 0,
                "There were {0} errors during the compilation. First one was {1}.\rCode: \r{2}",
                cfres1.Errors.Count, cfres1.Errors.Count > 0 ? cfres1.Errors[0].ErrorText : "", code1);
        }
    }

    public class FilePersistenceProvider : IPersistenceProvider
    {
        public string PersistedPath
        {
            get { return Path.Combine(Path.GetTempPath(), _randomizer); }
        }

        private readonly string _randomizer = Guid.NewGuid().ToString("N");

        public void GetBundle(string bundleId, string destinationPath)
        {
            File.Copy(Path.Combine(PersistedPath, bundleId, $"{bundleId}.zip"), destinationPath, true);
        }

        public void PersistBundle(string bundleId, string sourcePath)
        {
            var destinationPath = Path.Combine(PersistedPath, bundleId);
            Directory.CreateDirectory(destinationPath);
            File.Copy(sourcePath, Path.Combine(PersistedPath, bundleId, $"{bundleId}.zip"));
        }
    }

    static class DataEmulator
    {
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
                {"MakeModel", $"{makeModel.Item1}/{makeModel.Item2}"},
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
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 *random.NextDouble() + 65)));
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
