// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using System.Collections.Generic;
using AgenticRpg.Core.Clients;
using AgenticRpg.Core.Models.Config;
using AwesomeAssertions;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AgenticRpg.UnitTests.Core.Clients
{
    public class ClientFactoryTests
    {
        private readonly IBedrockClientFactory bedrockFactory = A.Fake<IBedrockClientFactory>();
        private readonly IOllamaClientFactory ollamaFactory = A.Fake<IOllamaClientFactory>();
        private readonly IConfigLoader configLoader = A.Fake<IConfigLoader>();

        private readonly ClientFactory clientFactory;


        public ClientFactoryTests()
        {
            var logger = NullLogger<ClientFactory>.Instance;

            clientFactory = new ClientFactory(bedrockFactory, ollamaFactory, configLoader, logger);
        }


        [Fact]
        public void UsesDefaultModelAndCreatesOllamaClientWhenModelNameIsNull()
        {
            // Arrange
            var modelConfig = CreateModel("default-model", "ollama");
            ConfigureModels(modelConfig);
            var expectedClient = A.Fake<IChatClientWrapper>();
            A.CallTo(() => ollamaFactory.Create(modelConfig)).Returns(expectedClient);

            // Act
            var result = clientFactory.Create(null);

            // Assert
            result.Should().BeSameAs(expectedClient);
            A.CallTo(() => ollamaFactory.Create(modelConfig)).MustHaveHappenedOnceExactly();
            A.CallTo(() => configLoader.LoadConfig<AuthConfig>("auth.jsonc", "auth config"))
                .MustNotHaveHappened();
        }


        [Fact]
        public void CreatesBedrockClientWithConfiguredAuthProfile()
        {
            // Arrange
            var modelConfig = CreateModel("bedrock-model", "bedrock", "test-profile");
            ConfigureModels(modelConfig);
            var authEntry = new AuthEntry { BearerToken = "test-token" };
            var authConfig = new AuthConfig
            {
                Profiles = new Dictionary<string, AuthEntry>
                {
                    { "test-profile", authEntry }
                }
            };
            A.CallTo(() => configLoader.LoadConfig<AuthConfig>("auth.jsonc", "auth config"))
                .Returns(authConfig);
            var expectedClient = A.Fake<IChatClientWrapper>();
            A.CallTo(() => bedrockFactory.Create(modelConfig, authEntry)).Returns(expectedClient);

            // Act
            var result = clientFactory.Create(modelConfig.Name);

            // Assert
            result.Should().BeSameAs(expectedClient);
            A.CallTo(() => configLoader.LoadConfig<AuthConfig>("auth.jsonc", "auth config"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => bedrockFactory.Create(modelConfig, authEntry)).MustHaveHappenedOnceExactly();
        }


        [Fact]
        public void ThrowsWhenRequestedModelIsNotConfigured()
        {
            // Arrange
            ConfigureModels(CreateModel("available-model", "ollama"));

            // Act
            Action act = () => clientFactory.Create("missing-model");

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Model 'missing-model' not found in config file.");
            A.CallTo(() => ollamaFactory.Create(A<ModelConfig>._)).MustNotHaveHappened();
            A.CallTo(() => bedrockFactory.Create(A<ModelConfig>._, A<AuthEntry?>._)).MustNotHaveHappened();
        }


        [Fact]
        public void ThrowsWhenConfiguredAuthProfileIsMissing()
        {
            // Arrange
            var modelConfig = CreateModel("bedrock-model", "bedrock", "missing-profile");
            ConfigureModels(modelConfig);
            A.CallTo(() => configLoader.LoadConfig<AuthConfig>("auth.jsonc", "auth config"))
                .Returns(new AuthConfig { Profiles = new Dictionary<string, AuthEntry>() });

            // Act
            Action act = () => clientFactory.Create(modelConfig.Name);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Auth profile 'missing-profile' not found in auth config file.");
            A.CallTo(() => bedrockFactory.Create(A<ModelConfig>._, A<AuthEntry?>._)).MustNotHaveHappened();
        }


        [Fact]
        public void ThrowsWhenConfiguredProviderIsUnsupported()
        {
            // Arrange
            var modelConfig = CreateModel("unknown-model", "unsupported-provider");
            ConfigureModels(modelConfig);

            // Act
            Action act = () => clientFactory.Create(null);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Unknown provider 'unsupported-provider' for model 'unknown-model'.");
            A.CallTo(() => ollamaFactory.Create(A<ModelConfig>._)).MustNotHaveHappened();
            A.CallTo(() => bedrockFactory.Create(A<ModelConfig>._, A<AuthEntry?>._)).MustNotHaveHappened();
        }


        private void ConfigureModels(params ModelConfig[] modelConfigs)
        {
            A.CallTo(() => configLoader.LoadConfig<ConfigFile>("config.jsonc", "config"))
                .Returns(new ConfigFile
                {
                    DefaultModel = modelConfigs[0].Name,
                    Models = [.. modelConfigs]
                });
        }


        private static ModelConfig CreateModel(string name, string provider, string? authProfile = null) => new()
        {
            Name = name,
            Provider = provider,
            ModelId = $"{name}-id",
            AuthProfile = authProfile
        };
    }
}
