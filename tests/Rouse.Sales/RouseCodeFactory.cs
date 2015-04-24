using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlexSharp;
using HotAssembly;
using Rouse.Sales.Rules.Enums.common;
using Rouse.Sales.Rules.Executables.RulesRepository;

namespace Rouse.Sales
{
    public class RouseCodeFactory :CodeFactory  
    {

        public RouseCodeFactory(IPersistenceProvider hotAssemblyPersistenceProvider, CodeFactoryInitializationModel data) 
            : base(hotAssemblyPersistenceProvider, data)
        {
        }

        private class ColumnInfo
        {
            public string ColumnName
            {
                get;
                set;
            }
            public string DataTypeName
            {
                get;
                set;
            }
            public Column_Source ColumnSource
            {
                get;
                set;
            }

            public string LocalVariableName
            {
                get
                {
                    return ColumnName.Substring(0, 1).ToLower() + ColumnName.Substring(1, ColumnName.Length - 1);
                }
            }

            public string FullDataTypeName
            {
                get
                {
                    return DataTypeName + (DataTypeName == "String" ? "" : "?");
                }
            }

        }

        private class VariableInfo
        {
            public string Name
            {
                get;
                set;
            }
            public string Formula
            {
                get;
                set;
            }
            public string ColumnName
            {
                get;
                set;
            }
            public string DataTypeName
            {
                get;
                set;
            }
            public string FullDataTypeName
            {
                get
                {
                    return DataTypeName + (DataTypeName == "String" ? "" : "?");
                }
            }
            public string LocalVariableName
            {
                get
                {
                    return Name.Substring(0, 1).ToLower() + Name.Substring(1, Name.Length - 1);
                }
            }
        }

        private IList<VariableInfo> _variables;
        private IList<ColumnInfo> _columns;


        protected override string GetCode()
        {
            GetColumnsInfo();
            GetVariablesInfo();
            var code = "using System;\rusing System.Collections.Generic;\rusing System.Linq;\r" + GetCode_NameSpaceAndBelow();

            return code;
        }

        private string GetCode_NameSpaceAndBelow()
        {
            var code =
                string.Format(
                    "namespace {0}\r{{\r\tpublic class {1} : Rouse.Sales.RulesEngine\r\t{{\rpublic {1} (object data) {{}}\r{2}{3}{4}{5}{6}{7}\r\t}}\r}}",
                    Namespace,
                    ClassName,
                    GetCode_PrivateAssetClass(),
                    GetCode_PrivateVariablesClass(),
                    GetCode_MapDataToAsset(),
                    GetCode_MapOutput(),
                    GetCode_ComputeInternal(),
                    GetCode_Variables());

            return code;
        }

        private string GetCode_PrivateVariablesClass()
        {
            // build the class definition and properties
            var code = string.Format("private class Variables_{0:N} {{", Randomizer);

            code = _variables
                .Aggregate(code,
                (current, variable) =>
                    current +
                    string.Format("public {0} {1} {{ get; private set; }}\r",
                        variable.FullDataTypeName,
                        variable.Name));

            // build setters
            code =
                (_variables
                    .Aggregate(code,
                        (current, variableInfo) =>
                            current +
                            string.Format("public void Set_{0}_{2:N} ({3} {1}) {{ {0} = {1}; }}\r", variableInfo.Name,
                                variableInfo.LocalVariableName, Randomizer, variableInfo.FullDataTypeName)));

            code += "}";

            //build instance of the Variables class:
            code +=
                string.Format(
                    "private Variables_{0:N} _variables_{0:N} = new Variables_{0:N}();\rprivate Variables_{0:N} Variables {{get {{ return _variables_{0:N}; }}}}\r",
                    Randomizer);

            return code;
        }

        private string GetCode_MapDataToAsset()
        {
            var code = string.Format("private void MapDataToAsset_{0:N}() {{\r", Randomizer);

            // initialize all local variables to null
            code = _columns.Where(column => column.ColumnSource == Column_Source.External)
                .Aggregate(code,
                    (current, column) =>
                        current + string.Format("{0} {1} = null;\r", column.FullDataTypeName, column.LocalVariableName));

            // map incoming object to the internal object
            code += "foreach (var field in Data)\r{\rswitch (field.Key)\r{";

            code =
                _columns.Where(column => column.ColumnSource == Column_Source.External)
                    .Aggregate(code,
                        (current, column) =>
                            current +
                            (column.DataTypeName == "String"
                                ? string.Format("case \"{0}\": {1} = field.Value; break;", column.ColumnName,
                                    column.LocalVariableName)
                                : string.Format(
                                    "case \"{0}\": {{{2} outValue; {1} = {2}.TryParse(field.Value, out outValue) ? outValue : ({2}?)null;}} break;",
                                    column.ColumnName, column.LocalVariableName, column.DataTypeName)));

            code += string.Format("}} _asset_{0:N} = new Asset_{0:N} (", Randomizer);

            code += string.Join(", ", _columns.Where(column => column.ColumnSource == Column_Source.External)
                .Select(column => string.Format("{0}", column.LocalVariableName)));

            code += ");\r}\r}";
            return code;
        }

        private void GetVariablesInfo()
        {
            _variables =
                Variables_List.Execute(GetInitializationData<CodeFactoryInitializationModel>().ClientId, null)
                    .Recordset0.Select(
                        record => new VariableInfo
                        {
                            ColumnName = record.OutputColumnName,
                            Formula = record.Formula,
                            Name = record.Name,
                            DataTypeName = record.DataTypeName
                        }
                    ).ToList();
        }

        private string GetCode_Variables()
        {
            return string.Join("\r", _variables.Select(
                variable => string.Format("private {0} Compute_{1}_{2:N}(){{\r{3}\r}}\r", variable.FullDataTypeName,
                    variable.Name, Randomizer, variable.Formula)));
        }

        private string GetCode_ComputeInternal()
        {

            var code = string.Format(
                "protected override object ComputeInternal(){{\rMapDataToAsset_{0:N}();\r" +
                "{1}\rreturn MapVariablesToOutput_{0:N}();\r}}",
                Randomizer,
                string.Join("",
                    _variables.Select(
                        variable =>
                            string.Format("Variables.Set_{0}_{1:N}(Compute_{2}_{1:N}());\r", variable.Name, Randomizer,
                                variable.Name))));

            return code;
        }

        /// <summary>
        /// Builds code for a method MapVariablesToOutput_xxxxxxxxxx. The responsibility of this method is to return all computed values
        /// </summary>
        /// <returns></returns>
        private string GetCode_MapOutput()
        {
            var code = string.Format("private object MapVariablesToOutput_{0:N}() {{\rreturn new Dictionary<string, string>{{", Randomizer);

            code += string.Join(", ", _variables
                .Where(variableInfo => !string.IsNullOrWhiteSpace(variableInfo.ColumnName))
                .Select(variableInfo => variableInfo.DataTypeName == "String"
                    ? string.Format(
                        "{{ \"{0}\", Variables.{0}}}\r",
                        variableInfo.Name)
                    : string.Format(
                        "{{ \"{0}\", Variables.{0}.HasValue ? String.Format(\"{{0}}\", Variables.{0}.Value) : null}}\r",
                        variableInfo.Name)));
            code += "};\r}";

            return code;
        }

        /// <summary>
        /// Builds code for the private asset class
        /// </summary>
        /// <returns></returns>
        private string GetCode_PrivateAssetClass()
        {

            // build the class definition and properties
            var code = string.Format("private class Asset_{0:N} {{", Randomizer);

            code = _columns
                .Where(columnInfo => columnInfo.ColumnSource != Column_Source.Computed)
                .Aggregate(code,
                (current, column) =>
                    current +
                    string.Format("public {0} {1} {{ get; private set; }}\r",
                        column.FullDataTypeName,
                        column.ColumnName));

            // build constructor
            code += string.Format("\rpublic Asset_{0:N}(", Randomizer);

            code += string.Join(", ", _columns
                .Where(column => column.ColumnSource != Column_Source.Computed)
                .Select(column => string.Format("{0} {1}",
                    column.FullDataTypeName,
                    column.LocalVariableName)));

            code += "){";
            code = _columns
                .Where(column => column.ColumnSource != Column_Source.Computed)
                .Aggregate(code,
                    (current, column) =>
                        current +
                        (column.ColumnName + " = " + column.LocalVariableName + ";\r"));
            code += "}\r}";

            //build instance of the Asset class:
            code +=
                string.Format(
                    "private Asset_{0:N} _asset_{0:N};\rprivate Asset_{0:N} Asset {{get {{ return _asset_{0:N}; }}}}\r",
                    Randomizer);

            return code;
        }

        private void GetColumnsInfo()
        {
            _columns = Columns_List.Execute(GetInitializationData<CodeFactoryInitializationModel>().ClientId, null)
                .Recordset0.Select(record => new ColumnInfo
                {
                    ColumnName = record.ColumnName,
                    DataTypeName = record.DataTypeName,
                    ColumnSource = (Column_Source)record.SourceId
                }).ToList();
        }

    }
}
