using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace CalcbenchListener
{
    class Program
    {
        static IQueueClient queueClient;
        static void Main(string[] args)
        {
            AsyncListenForFilings().GetAwaiter().GetResult();
        }

        static async Task AsyncListenForFilings()
        {
            var connectionString = Properties.Settings.Default.ServiceBusConnectionString;
            var queueName = Properties.Settings.Default.QueueName;
            queueClient = new QueueClient(connectionString, queueName);
            RegisterOnFilingHandlerAndReceiveMessages();
            await queueClient.CloseAsync();
        }

        static void RegisterOnFilingHandlerAndReceiveMessages()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };
            queueClient.RegisterMessageHandler(ProcessFiling, messageHandlerOptions);
        }

        static async Task ProcessFiling(Message message, CancellationToken token)
        {
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
