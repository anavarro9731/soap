namespace Soap.PfBase.Api
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Loader;
    using System.Text;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Providers.CosmosDb;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;
    using Microsoft.CSharp.RuntimeBinder;
    using Soap.Bus;
    using Soap.Config;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.NotificationServer;

    public static class ConfigFunctions
    {
        public static void LoadAppConfigFromRemoteRepo(out ApplicationConfig applicationConfig)
        {
            string url = null;
            try
            {
                var webClient = new WebClient();
                webClient.Headers.Add("Authorization", GetAuthorizationHeaderValue());
                url = GetFileUrl();
                var configToCompile = webClient.DownloadString(url);

                var config = LoadAndExecute(configToCompile);
                config.Validate();
                applicationConfig = config;
            }
            catch (Exception e)
            {
                throw new Exception($"Could not compile config from remote repo {url}", e);
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
                            MetadataReference.CreateFromFile(typeof(DataStore).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(IDatabaseSettings).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(CosmosDbRepository).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(AzureBus).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(NotificationServer).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(IBus).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(Enumeration<>).Assembly.Location), 
                            MetadataReference.CreateFromFile(typeof(ApplicationConfig).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(AssemblyTargetedPatchBandAttribute).Assembly.Location),
                            MetadataReference.CreateFromFile(typeof(CSharpArgumentInfo).Assembly.Location),
                            MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location)
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
                var appId = EnvVars.AppId;
                appId = appId.Replace("-api-sample", "", StringComparison.InvariantCultureIgnoreCase); //* HACK: Soap solution uses the functionprojectname e.g. soap-api-sample which it should but the repo and devops org are called soap
                //* technically library projects and function project can be stored and built from the same repo, if they use the function project name for the repo, but sometimes that wouldn't make good sense like in the case of Soap
                //* it is better to keep library and function projects in different repo with different pwsh-bootstrap files
                var fileUrl = string.Format(
                    $"https://dev.azure.com/{EnvVars.AzureDevopsOrganisation}/{appId}/_apis/git/repositories/{appId}.config/items?path=Config/{EnvVars.SoapEnvironmentKey}/Config.cs&api-version=5.1");
                return fileUrl;
            }

            static string GetAuthorizationHeaderValue()
            {
                var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($":{EnvVars.AzureDevopsPat}"));
                return $"Basic {basicAuth}";
            }
        }
    }
}