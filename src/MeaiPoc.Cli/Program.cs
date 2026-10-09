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

                var messages = new List<ChatMessage>();

                using var consoleCancellation = new ConsoleCancellation();

                var chatOptions = new ChatOptions
                {
                    Instructions = """
                                   You are an old, wise wizard, tasked with tutoring a rebellious student in the ways of the magic arts.
                                   """
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
                                       wrapper.Chat.GetStreamingResponseAsync(
                                           [.. messages, userMessage],
                                           chatOptions,
                                           cancellationToken))
                        {
                            Console.Write(update.Text);
                            updates.Add(update);
                        }

                        Console.WriteLine();

                        messages.Add(userMessage);
                        messages.AddMessages(updates);
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
