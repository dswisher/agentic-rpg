using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using MeaiPoc.Cli.Models;
using Microsoft.Extensions.AI;

namespace MeaiPoc.Cli
{
    public static class Program
    {
        private static readonly JsonSerializerOptions serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            RespectNullableAnnotations = true
        };


        public static async Task<int> Main()
        {
            try
            {
                // Read the config
                var configFilePath = Path.Join(Environment.GetEnvironmentVariable("HOME"), ".config", "meai-poc", "config.jsonc");

                if (!File.Exists(configFilePath))
                {
                    throw new FileNotFoundException($"Config file not found: {configFilePath}");
                }

                Console.WriteLine($"Reading config file: {configFilePath}...");

                ConfigFile? config;
                await using (var stream = File.OpenRead(configFilePath))
                {
                    config = JsonSerializer.Deserialize<ConfigFile>(stream, serializerOptions);
                }

                Console.WriteLine("...finished reading config file...");

                if (config == null)
                {
                    throw new InvalidOperationException("Failed to deserialize configuration file.");
                }

                // Set up the AWS config
                var awsConfig = new AmazonBedrockRuntimeConfig
                {
                    RegionEndpoint = RegionEndpoint.USEast1,
                    AWSTokenProvider = new StaticTokenProvider(config.Key),
                    AuthSchemePreference = ["httpBearerAuth"], // otherwise SigV4 wins if AWS creds exist
                };

                Console.WriteLine($"...creating IChatClient, model {config.Model}...");
                var runtime = new AmazonBedrockRuntimeClient(awsConfig);
                var chat = runtime.AsIChatClient(config.Model);

                Console.WriteLine("...sending request and awaiting response...");
                var response = await chat.GetResponseAsync("Say hello in five words.");
                Console.WriteLine(response.Text);
                Console.WriteLine($"in={response.Usage?.InputTokenCount} out={response.Usage?.OutputTokenCount}");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception");
                Console.WriteLine(ex);

                return 1;
            }
        }
    }
}
