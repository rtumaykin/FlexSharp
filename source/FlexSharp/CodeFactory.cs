using System;
using System.CodeDom.Compiler;
using System.IO;
using System.IO.Compression;
using System.Linq;
using HotAssembly;
using Newtonsoft.Json;

namespace FlexSharp
{
    /// <summary>
    /// Encapsulates base functionality to generate code for derived classes
    /// </summary>
    public abstract class CodeFactory
    {
        private readonly IPersistenceProvider _hotAssemblyPersistenceProvider;

        private readonly Guid _randomizer = Guid.NewGuid();
        protected Guid Randomizer
        {
            get { return _randomizer; }
        }

        protected string Namespace
        {
            get { return $"ns_{Randomizer:N}"; }
        }

        protected string ClassName
        {
            get
            {
                return $"class_{Randomizer:N}";
            }
        }

        private readonly object _data;
        protected T GetInitializationData<T>() where T : class
        {
            return _data as T;
        }
        
        protected CodeFactory (IPersistenceProvider hotAssemblyPersistenceProvider, object data)
        {
            _data = data;
            _hotAssemblyPersistenceProvider = hotAssemblyPersistenceProvider;
        }
        
        /// <summary>
        /// This method implements main code generation for the dynamic rules engine class
        /// </summary>
        /// <returns>Full code for the class to be generated</returns>
        protected abstract string GetCode();

        /// <summary>
        /// Builds a list of referenced assemblies for the class.
        /// </summary>
        /// <returns></returns>
        protected virtual string[] GetReferencedAssemblies()
        {
            return new[]
            {
                GetType().Assembly.Location,
                "System.Core.dll",
                "mscorlib.dll",
                "System.dll"

            };
        }

        protected CompilerResults CompileInternal()
        {
            var code = GetCode();

            var appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                DisallowBindingRedirects = false,
                DisallowCodeDownload = true,
                ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
            };

            var workerAppDomain = AppDomain.CreateDomain("Compiler", null, appDomainSetup);

            try
            {
                var compiler = (Compiler)workerAppDomain.CreateInstanceAndUnwrap(
                  typeof(Compiler).Assembly.GetName().Name,
                  typeof(Compiler).FullName);


                var compilerResults = compiler.Compile(code, 
                    GetReferencedAssemblies().GroupBy(s => s).Select(x => x.First()).ToArray());

                return compilerResults;
            }
            finally
            {
                AppDomain.Unload(workerAppDomain);
            }
        }

        public CompilerResults CompileAndSave(string bundleId)
        {
            var ret = CompileInternal();

            if (ret.Errors.Count != 0)
                return ret;

            var manifest = new Manifest
            {
                AssemblyName = Path.GetFileName(ret.PathToAssembly),
                FullyQualifiedClassName = $"{Namespace}.{ClassName}"
            };

            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempFolder);

            File.WriteAllText(Path.Combine(tempFolder, "manifest.json"), JsonConvert.SerializeObject(manifest));
            File.Copy(ret.PathToAssembly, Path.Combine(tempFolder, Path.GetFileName(ret.PathToAssembly)));


            Directory.CreateDirectory($"{tempFolder}-1");
            ZipFile.CreateFromDirectory(tempFolder, Path.Combine($"{tempFolder}-1", "bundle.zip"));

            _hotAssemblyPersistenceProvider.PersistBundle(bundleId, Path.Combine($"{tempFolder}-1", "bundle.zip"));

            return ret;
        }

        public CompilerResults CheckSyntax()
        {
            return CompileInternal();
        }

    }
}
