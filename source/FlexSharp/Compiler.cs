using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using Microsoft.CSharp;

namespace FlexSharp
{
    internal class Compiler : MarshalByRefObject
    {
        public CompilerResults Compile(string code, string[] referencedAssemblies)
        {
            var compilerParameters = new CompilerParameters
            {
                CompilerOptions = "/t:library /debug",
                GenerateExecutable = false
            };

            compilerParameters.ReferencedAssemblies.AddRange(referencedAssemblies);
            compilerParameters.OutputAssembly = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.dll");

            var providerOptions = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
            CodeDomProvider codeProvider = new CSharpCodeProvider(providerOptions);

            using (codeProvider)
            {
                var results = codeProvider.CompileAssemblyFromSource(compilerParameters, code);

                return results;
            }
        }
    }

}
