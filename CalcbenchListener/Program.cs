using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace CalcbenchListener
{
    class Program
    {
        static IQueueClient queueClient;
        static void Main(string[] args)
        {
            // The code provided will print ‘Hello World’ to the console.
            // Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.
            Console.WriteLine("Hello World!");
            Console.ReadKey();

            // Go to http://aka.ms/dotnet-get-started-console to continue learning how to build a console app! 
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
            queueClient.RegisterMessageHandler()
        }

        static async Task ProcessFiling()
        {

        }
    }
}
