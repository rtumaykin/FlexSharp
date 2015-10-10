using System.Collections.Generic;
using System.Linq;
using FlexSharp;
using HotAssembly;
using ICSharpCode.NRefactory.CSharp;
using Sample.DataMockup;

namespace Sample
{
    public class SampleCodeFactory : CodeFactory
    {
        private readonly int _clientId;

        public SampleCodeFactory(IPersistenceProvider hotAssemblyPersistenceProvider, CodeFactoryInitializationModel data) 
            : base(hotAssemblyPersistenceProvider, data)
        {
            _clientId = data.ClientId;
        }

        private class FieldInfo
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
                    return DataTypeName + ("System.String/String/string".Split('/').Contains(DataTypeName) ? "" : "?");
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
            public int EvaluationOrder
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
                    return DataTypeName + ("System.String/String/string".Split('/').Contains(DataTypeName) ? "" : "?");
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
        private IList<FieldInfo> _columns;

#if DEBUG
        public string GetCodeInternal()
        {
            return GetCode();
        }
#endif

        protected override string GetCode()
        {
            GetFieldsInfo();
            GetVariablesInfo();
            var code = "using System;\rusing System.Collections.Generic;\rusing System.Linq;\r" + GetCode_NameSpaceAndBelow();

            var formattingOptions = FormattingOptionsFactory.CreateAllman();
            formattingOptions.AlignElseInIfStatements = true;
            formattingOptions.ArrayInitializerWrapping = Wrapping.WrapIfTooLong;
            formattingOptions.ChainedMethodCallWrapping = Wrapping.WrapIfTooLong;
            formattingOptions.AutoPropertyFormatting = PropertyFormatting.ForceOneLine;
            formattingOptions.IndexerArgumentWrapping = Wrapping.WrapIfTooLong;
            formattingOptions.IndexerClosingBracketOnNewLine = NewLinePlacement.SameLine;
            formattingOptions.IndexerDeclarationClosingBracketOnNewLine = NewLinePlacement.SameLine;
            formattingOptions.IndexerDeclarationParameterWrapping = Wrapping.WrapIfTooLong;
            formattingOptions.MethodCallArgumentWrapping = Wrapping.WrapIfTooLong;
            formattingOptions.MethodCallClosingParenthesesOnNewLine = NewLinePlacement.SameLine;
            formattingOptions.MethodDeclarationClosingParenthesesOnNewLine = NewLinePlacement.SameLine;
            formattingOptions.MethodDeclarationParameterWrapping = Wrapping.WrapIfTooLong;

            formattingOptions.NewLineAferIndexerDeclarationOpenBracket = NewLinePlacement.SameLine;
            formattingOptions.NewLineAferIndexerOpenBracket = NewLinePlacement.SameLine;
            formattingOptions.NewLineAferMethodCallOpenParentheses = NewLinePlacement.SameLine;
            formattingOptions.NewLineAferMethodDeclarationOpenParentheses = NewLinePlacement.SameLine;




            var formattedCode = new CSharpFormatter(FormattingOptionsFactory.CreateAllman()).Format(code);

            return formattedCode;
        }

        private string GetCode_NameSpaceAndBelow()
        {
            var code =
                string.Format(
                    "namespace {0}\r{{\r\tpublic class {1} : Sample.RulesEngine\r\t{{\rpublic {1} () {{}}\r{2}{3}{4}{5}{6}{7}\r\t}}\r}}",
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
            var code = $"private class Variables_{Randomizer:N} {{";

            code = _variables
                .Aggregate(code,
                (current, variable) =>
                    current +
                    $"public {variable.FullDataTypeName} {variable.Name} {{ get; private set; }}\r");

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
            var code = $"private void MapDataToAsset_{Randomizer:N}() {{\r";

            // initialize all local variables to null
            code = _columns
                .Aggregate(code,
                    (current, column) =>
                        current + $"{column.FullDataTypeName} {column.LocalVariableName} = null;\r");

            // map incoming object to the internal object
            code += "foreach (var field in Data)\r{\rswitch (field.Key)\r{";

            code =
                _columns
                    .Aggregate(code,
                        (current, column) =>
                            current +
                            ("System.String/String/string".Split('/').Contains(column.DataTypeName)
                                ? $"case \"{column.ColumnName}\": {column.LocalVariableName} = field.Value; break;"
                                : string.Format(
                                    "case \"{0}\": {{{2} outValue; {1} = {2}.TryParse(field.Value, out outValue) ? outValue : ({2}?)null;}} break;",
                                    column.ColumnName, column.LocalVariableName, column.DataTypeName)));

            code += string.Format("}} _asset_{0:N} = new Asset_{0:N} (", Randomizer);

            code += string.Join(", ", _columns
                .Select(column => $"{column.LocalVariableName}"));

            code += ");\r}\r}";
            return code;
        }

        private void GetVariablesInfo()
        {
            _variables = DataEmulator.GetVariablesCollection(_clientId).Select(v => new VariableInfo
            {
                Name = v.Name,
                Formula = v.Formula,
                EvaluationOrder = v.EvaluationOrder,
                DataTypeName = v.DataType
            }).ToList();
        }

        private string GetCode_Variables()
        {
            return string.Join("\r", _variables.OrderBy(v => v.EvaluationOrder).Select(
                variable =>
                    $"private {variable.FullDataTypeName} Compute_{variable.Name}_{Randomizer:N}(){{\r{variable.Formula}\r}}\r"));
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
            var code =
                $"private object MapVariablesToOutput_{Randomizer:N}() {{\rreturn new Dictionary<string, string>{{";

            code += string.Join(", ", _variables
                .Select(variableInfo => "System.String/String/string".Split('/').Contains(variableInfo.DataTypeName)
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
            var code = $"private class Asset_{Randomizer:N} {{";

            code = _columns
                .Aggregate(code,
                (current, column) =>
                    current +
                    $"public {column.FullDataTypeName} {column.ColumnName} {{ get; private set; }}\r");

            // build constructor
            code += $"\rpublic Asset_{Randomizer:N}(";

            code += string.Join(", ", _columns
                .Select(column => $"{column.FullDataTypeName} {column.LocalVariableName}"));

            code += "){";
            code = _columns
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

        private void GetFieldsInfo()
        {
            _columns = DataEmulator.GetClientFields(_clientId).Select(f => new FieldInfo
            {
                ColumnName = f.Name,
                DataTypeName = f.DataType
            }).ToList();
        }

    }
}
