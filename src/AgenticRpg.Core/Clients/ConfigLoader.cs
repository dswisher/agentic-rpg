// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace AgenticRpg.Core.Clients
{
    public class ConfigLoader : IConfigLoader
    {
        private static readonly JsonSerializerOptions serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            RespectNullableAnnotations = true
        };

        private readonly ILogger<ConfigLoader> logger;


        public ConfigLoader(ILogger<ConfigLoader> logger)
        {
            this.logger = logger;
        }


        public T LoadConfig<T>(string filename, string kind)
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var configFilePath = Path.Join(userProfile, ".config", "agentic-rpg", filename);

            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"{kind} file not found: {configFilePath}");
            }

            logger.LogInformation("...reading {Kind} file {ConfigFilePath}", kind, configFilePath);

            T? config;
            using (var stream = File.OpenRead(configFilePath))
            {
                config = JsonSerializer.Deserialize<T>(stream, serializerOptions);
            }

            if (config == null)
            {
                throw new InvalidOperationException($"Failed to deserialize {kind} file.");
            }

            return config;
        }
    }
}
