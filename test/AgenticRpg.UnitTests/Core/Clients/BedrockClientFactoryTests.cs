// Copyright (c) Doug Swisher. All Rights Reserved.
// Licensed under the MIT License. See LICENSE.md in the project root for license information.

using System;
using AgenticRpg.Core.Clients;
using AgenticRpg.Core.Models.Config;
using AwesomeAssertions;
using FakeItEasy;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AgenticRpg.UnitTests.Core.Clients
{
    public class BedrockClientFactoryTests
    {
        private readonly IChatClientEnhancer enhancer = A.Fake<IChatClientEnhancer>();
        private readonly BedrockClientFactory factory;


        public BedrockClientFactoryTests()
        {
            factory = new BedrockClientFactory(enhancer, NullLogger<BedrockClientFactory>.Instance);
        }


        [Fact]
        public void ThrowsWhenAuthEntryIsNull()
        {
            // Arrange
            var modelConfig = CreateModel();

            // Act
            Action act = () => factory.Create(modelConfig, null);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Auth entry with a bearerToken is required for model 'test-model'.");
            A.CallTo(() => enhancer.Enhance(A<IChatClient>._)).MustNotHaveHappened();
        }


        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ThrowsWhenBearerTokenIsBlank(string bearerToken)
        {
            // Arrange
            var modelConfig = CreateModel();
            var authEntry = new AuthEntry { BearerToken = bearerToken };

            // Act
            Action act = () => factory.Create(modelConfig, authEntry);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Auth entry with a bearerToken is required for model 'test-model'.");
            A.CallTo(() => enhancer.Enhance(A<IChatClient>._)).MustNotHaveHappened();
        }


        [Fact]
        public void CreatesAndEnhancesBedrockChatClientWithValidBearerToken()
        {
            // Arrange
            var modelConfig = CreateModel();
            var authEntry = new AuthEntry { BearerToken = "test-token" };
            var enhancedChatClient = A.Fake<IChatClient>();
            A.CallTo(() => enhancer.Enhance(A<IChatClient>._)).Returns(enhancedChatClient);

            // Act
            using var result = factory.Create(modelConfig, authEntry);

            // Assert
            var wrapper = result.Should().BeOfType<BedrockClientWrapper>().Which;
            wrapper.Chat.Should().BeSameAs(enhancedChatClient);
            A.CallTo(() => enhancer.Enhance(A<IChatClient>._)).MustHaveHappenedOnceExactly();
        }


        private static ModelConfig CreateModel() => new()
        {
            Name = "test-model",
            Provider = "bedrock",
            ModelId = "test-model-id",
            Options = new ModelOptions { Region = "us-west-2" }
        };
    }
}
