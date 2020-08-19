// -----------------------------------------------------------------------
// <copyright file="$FILENAME$" company="$COMPANYNAME$">
// $COPYRIGHT$
// </copyright>
// <summary>
// $SUMMARY$
// </summary>

namespace Soap.PfBase.Alj
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime;
    using System.Runtime.Loader;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;
    using Microsoft.CSharp.RuntimeBinder;
    using Soap.Api.Sample.Afs;
    using Soap.MessagePipeline.Context;

    public class ConfigFunctions
    {
        public static void LoadFromRemoteRepo(AppEnvIdentifier appEnvId, out ApplicationConfig applicationConfig)
        {
            try
            {
                var webClient = new WebClient();
                webClient.Headers.Add($"Authorisation:{GetAuthorizationHeader()}");
                var url = GetFileUrl();
                var configToCompile = webClient.DownloadString(url);
                var config = LoadAndExecute(configToCompile);
                config.Validate();
                applicationConfig = config;
            }
            catch (Exception e)
            {
                throw new Exception("Could not read config from remote repo", e);
            }

            static ApplicationConfig LoadAndExecute(string source)
            {
                var compiledAssembly = Compile(source);

                using var asm = new MemoryStream(compiledAssembly);
                var assemblyLoadContext = new AssemblyLoadContext("ServiceConfig");

                var assembly = assemblyLoadContext.LoadFromStream(asm);

                var config = Activator.CreateInstance(assembly.GetTypes().Single()) as ApplicationConfig;

                return config;

                static byte[] Compile(string sourceCode)
                {
                    using var peStream = new MemoryStream();
                    var result = GenerateCode(sourceCode).Emit(peStream);

                    if (!result.Success)
                    {
                        var failures = result.Diagnostics
                                             .Where(
                                                 diagnostic =>
                                                     diagnostic.IsWarningAsError
                                                     || diagnostic.Severity == DiagnosticSeverity.Error)
                                             .Select(x => x.GetMessage())
                                             .ToList();
                        var error = string.Join(' ', failures);
                        throw new Exception(error);
                    }

                    peStream.Seek(0, SeekOrigin.Begin);

                    return peStream.ToArray();

                    static CSharpCompilation GenerateCode(string sourceCode)
                    {
                        var codeString = SourceText.From(sourceCode);

                        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_3);

                        var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

                        var references = new MetadataReference[]
                        {
                            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(ApplicationConfig).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(AssemblyTargetedPatchBandAttribute).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(CSharpArgumentInfo).Assembly.Location)
                        };

                        var compilationOptions = new CSharpCompilationOptions(
                            OutputKind.DynamicallyLinkedLibrary,
                            optimizationLevel: OptimizationLevel.Release);

                        var compilation = CSharpCompilation.Create("ServiceConfig.dll")
                                                           .WithOptions(compilationOptions)
                                                           .WithReferences(references)
                                                           .AddSyntaxTrees(parsedSyntaxTree);

                        return compilation;
                    }
                }
            }

            static string GetFileUrl()
            {
                var fileUrl = string.Format(
                    $"https://dev.azure.com/{ConfigId.AzureDevopsOrganisation}/soap.config/_apis/git/repositories/soap.config/items?path=Config/{ConfigId.SoapApplicationKey}/{ConfigId.SoapEnvironmentKey}/appConfig.cs&api-version=5.1");
                return fileUrl;
            }

            static string GetAuthorizationHeader()
            {
                var basicAuth = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{ConfigId.AzureDevopsPat}"));
                return $"Basic {basicAuth}";
            }
        }
    }
}