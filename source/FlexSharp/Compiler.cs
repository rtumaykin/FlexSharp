using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CSharp;

namespace FlexSharp
{
    internal class Compiler : MarshalByRefObject
    {
        public CompilerResults Compile(string code, bool checkSyntaxOnly, string assemblyFullPath, string[] referencedAssemblies)
        {
            var compilerParameters = new CompilerParameters
            {
                CompilerOptions = "/t:library /debug",
                GenerateExecutable = false,
                GenerateInMemory = checkSyntaxOnly
            };

            compilerParameters.ReferencedAssemblies.AddRange(referencedAssemblies);
            if (!checkSyntaxOnly)
                compilerParameters.OutputAssembly = assemblyFullPath;

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
