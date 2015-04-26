using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotAssembly;
using NUnit.Framework;
using Rouse.Sales;
using SQL.TableVariables.common;

namespace UnitTests
{
    [TestFixture]
    public class Test
    {
        [Test]
        public void MakeSure_All_Compiles()
        {
            var start = DateTime.Now;
            var e = SQL.Enums.common.DataType.DateTime;

            var eng =
                new RouseCodeFactory(new SqlPersistenceProvider(), 
                    new CodeFactoryInitializationModel()
                    {
                        ClientId = 1
                    });

            var compileResults = eng.CompileAndSave();

            if (compileResults.Errors.Count > 0)
                throw new Exception("Errors during compilation found");

            var cnt = 0;

            var engineId = Guid.Parse(Path.GetFileNameWithoutExtension(compileResults.PathToAssembly));

            var l = new List<AssetFieldDataRow>();
            Debug.WriteLine("All prep is done. Total elapsed {0} ms", DateTime.Now.Subtract(start).TotalMilliseconds);

            var res = SQL.Executables.calc.GetSampleData.Execute(null);
            var ef = new HotAssembly.InstantiatorFactory<RulesEngine>(new SqlPersistenceProvider());

            Debug.WriteLine("Retrieved records. Total elapsed {0} ms", DateTime.Now.Subtract(start).TotalMilliseconds);

            var tasks = new List<Task>();

            foreach (var row in res.Recordset0)
            {
                var dataIn = new Dictionary<string, string>
                {
                    {"EquipNo", row.EquipNo},
                    {"AcqDate", row.AcquisitionDate.HasValue ? row.AcquisitionDate.Value.ToString("O") : null},
                    {"ClientMake", row.ClientMake},
                    {"ClientModel", row.ClientModel},
                    {"ForSaleFlag", row.ForSaleFlag},
                    {"Cost", row.Cost.HasValue ? row.Cost.Value.ToString(CultureInfo.InvariantCulture) : null},
                    {"Year", row.Year.HasValue ? row.Year.Value.ToString(CultureInfo.InvariantCulture) : null}
                };


                var inst = ef.Instantiate(engineId.ToString("N"), null);
                var ret = inst.Compute(dataIn) as Dictionary<string, string>;

                var x = ret;

                l.AddRange(new[]
                {
                    new SQL.TableVariables.common.AssetFieldDataRow(row.EquipNo, 5, row.EquipNo),

                    new SQL.TableVariables.common.AssetFieldDataRow(row.EquipNo, 6,
                        row.AcquisitionDate.HasValue ? row.AcquisitionDate.Value.ToString("O") : null),
                    new SQL.TableVariables.common.AssetFieldDataRow(row.EquipNo, 7, row.ClientMake),
                    new SQL.TableVariables.common.AssetFieldDataRow(row.EquipNo, 8, row.ClientModel),

                    new SQL.TableVariables.common.AssetFieldDataRow(row.EquipNo,
                        12,
                        row.ForSaleFlag),

                    new SQL.TableVariables.common.AssetFieldDataRow(row.EquipNo, 11,
                        row.Cost.HasValue ? row.Cost.Value.ToString(CultureInfo.InvariantCulture) : null),

                    new SQL.TableVariables.common.AssetFieldDataRow(row.EquipNo, 9,
                        row.Year.HasValue ? row.Year.Value.ToString(CultureInfo.InvariantCulture) : null),
                    new SQL.TableVariables.common.AssetFieldDataRow(row.EquipNo, 13, ret["RulValue"]),
                    new SQL.TableVariables.common.AssetFieldDataRow(row.EquipNo, 14, ret["ReverseDescriptionAndYear"])
                });

                if (cnt % 1000 == 0)
                {
                    Debug.WriteLine("Processed {1} records. Total elapsed {0} ms", DateTime.Now.Subtract(start).TotalMilliseconds, cnt);
                }
                if ((cnt + 1) % 50000 == 0)
                {
                    var _cnt = cnt;
                    var data = l;
                    l = new List<AssetFieldDataRow>();
                    tasks.Add(Save(data, _cnt));
                }
                cnt++;
            }

            tasks.Add(Save(l, Int32.MaxValue));

            Task.WaitAll(tasks.ToArray());
            Debug.WriteLine("All finished. Total elapsed {0} ms", DateTime.Now.Subtract(start).TotalMilliseconds);

        }

        private async Task Save(IList<AssetFieldDataRow> data, int cnt)
        {
            var start = DateTime.Now;
            await SQL.Executables.asset.BatchUpdate.ExecuteAsync(1, new AssetFieldData(data), false, null, 0);
            Debug.WriteLine("Data Save for iteration {1} completed. Total elapsed {0} ms", DateTime.Now.Subtract(start).TotalMilliseconds, cnt);
        }

        private async void Ave(IList<AssetFieldDataRow> data, int cnt)
        {
            var start = DateTime.Now;
            await SQL.Executables.asset.BatchUpdate.ExecuteAsync(1, new AssetFieldData(data), false, null, 0);
            Debug.WriteLine("Data Save for iteration {1} completed. Total elapsed {0} ms", DateTime.Now.Subtract(start).TotalMilliseconds, cnt);
        }
    }
}
