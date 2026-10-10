using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeaiPoc.Cli.Clients;
using Microsoft.Extensions.AI;

namespace MeaiPoc.Cli
{
    public static class Program
    {
        public static async Task<int> Main()
        {
            try
            {
                using var wrapper = await ClientFactory.Create();

                var chatClient = wrapper.Chat
                    .AsBuilder()
                    .UseFunctionInvocation()
                    .Build();

                var messages = new List<ChatMessage>();

                using var consoleCancellation = new ConsoleCancellation();

                var diceTool = AIFunctionFactory.Create(
                    (int sides) => DieRoller.RollDice(sides),
                    name: "roll_dice",
                    description: "Roll one fair die. The sides value must be between 2 and 1000.");

                var chatOptions = new ChatOptions
                {
                    Instructions = """
                                   Only respond with plain text (no markdown), lines should not be longer than 80 characters.
                                   """,
                    Tools = [diceTool]
                };

                while (true)
                {
                    Console.Write("> ");
                    var input = Console.ReadLine();

                    if (input == null)
                    {
                        break;  // EOF
                    }

                    if (string.IsNullOrEmpty(input))
                    {
                        continue;
                    }

                    if (input.Equals("/quit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    var userMessage = new ChatMessage(ChatRole.User, input);

                    var cancellationToken = consoleCancellation.BeginRequest();

                    try
                    {
                        var updates = new List<ChatResponseUpdate>();

                        await foreach (var update in
                                       chatClient.GetStreamingResponseAsync(
                                           [.. messages, userMessage],
                                           chatOptions,
                                           cancellationToken))
                        {
                            Console.Write(update.Text);
                            updates.Add(update);
                        }

                        Console.WriteLine();

                        var response = updates.ToChatResponse();

                        messages.Add(userMessage);
                        messages.AddMessages(response);

                        if (response.Usage is { } usage)
                        {
                            Console.WriteLine($"[tokens: input={usage.InputTokenCount}, output={usage.OutputTokenCount}, total={usage.TotalTokenCount}]");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine();
                        Console.WriteLine("[cancelled]");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error getting response from chat client:");
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        consoleCancellation.EndRequest();
                    }
                }

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
