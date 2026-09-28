/*------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class ProcessorBuilder : IDisposable
    {
        private readonly string _typeName;
        private readonly string _code;
        private readonly List<MetadataReference> _references;

        public static readonly string[] DefaultNamespaces =
        {
                "System",
                "System.Linq",
                "System.Text",
                "System.Text.RegularExpressions",
                "System.Collections.Generic"
        };


        public ProcessorBuilder(string typeName, string code, IEnumerable<string> assemblyNames)
        {

            _typeName = typeName;
            _code = code;

            _references = new List<MetadataReference>();
            foreach (var assemblyName in assemblyNames)
            {
                _references.Add(MetadataReference.CreateFromFile(RuntimeEnvironment.GetRuntimeDirectory() + assemblyName + ".dll"));
            }

            _references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            _references.Add(MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location));
            _references.Add(MetadataReference.CreateFromFile(typeof(IProcessor).GetTypeInfo().Assembly.Location));
            _references.Add(MetadataReference.CreateFromFile(typeof(GatewayData).GetTypeInfo().Assembly.Location));
            _references.Add(MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.JsonConvert).GetTypeInfo().Assembly.Location));
            // Do not remove this line: it is used in AprilBrothers gateway
            _references.Add(MetadataReference.CreateFromFile(typeof(MsgPack.Serialization.MessagePackSerializer).GetTypeInfo().Assembly.Location));

        }

        public Type Compile()
        {
            try
            {
                var options = new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    reportSuppressedDiagnostics: true,
                    optimizationLevel: OptimizationLevel.Release,
                    generalDiagnosticOption: ReportDiagnostic.Error,
                    allowUnsafe: true)
                    .WithUsings(DefaultNamespaces);

                var syntaxTree = CSharpSyntaxTree.ParseText(_code, options: new CSharpParseOptions(LanguageVersion.Latest, kind: SourceCodeKind.Regular));
                var compilation = CSharpCompilation.Create(_typeName, new[] { syntaxTree }, _references, options);

                Assembly assembly;

                using (var ms = new MemoryStream())
                {
                    var result = compilation.Emit(ms);
                    ThrowExceptionIfCompilationFailure(result);
                    ms.Seek(0, SeekOrigin.Begin);
                    assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(ms);
                }

                return assembly.DefinedTypes.Where(t => t.ImplementedInterfaces.Contains(typeof(IProcessor))).FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ThrowExceptionIfCompilationFailure(EmitResult result)
        {
            if (!result.Success)
            {
                var compilationErrors = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error)
                    .ToList();
                if (compilationErrors.Any())
                {
                    var firstError = compilationErrors.First();
                    var errorNumber = firstError.Id;
                    var errorDescription = firstError.GetMessage();
                    var firstErrorMessage = $"{errorNumber}: {errorDescription};";
                    throw new Exception($"Compilation failed, first error is: {firstErrorMessage}");
                }
            }
        }

        public void Dispose()
        {
            // what to do!
        }
    }
}
