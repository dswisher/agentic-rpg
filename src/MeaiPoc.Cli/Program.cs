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

                    ChatResponse response;
                    try
                    {
                        response = await wrapper.Chat.GetResponseAsync([.. messages, userMessage], cancellationToken: cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("[cancelled]");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error getting response from chat client:");
                        Console.WriteLine(ex);
                        continue;
                    }
                    finally
                    {
                        consoleCancellation.EndRequest();
                    }

                    Console.WriteLine(response.Text);

                    messages.Add(userMessage);
                    messages.AddMessages(response);
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
