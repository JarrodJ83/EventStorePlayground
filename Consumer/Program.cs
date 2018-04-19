using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Producer.Events;

namespace Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            //ReadLiveStream().Wait();

            ReadStreamFromPoint().Wait();

            Console.ReadLine();
        }

        static async Task ReadStreamFromPoint()
        {
            using(var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113)))
            {

            await connection.ConnectAsync();

            var lastProcessedFile = "lastprocessed.txt";
            if(!File.Exists(lastProcessedFile)){
                await File.WriteAllTextAsync(lastProcessedFile, "0");
            }

            long lastProcessed = long.Parse(await File.ReadAllTextAsync(lastProcessedFile));            

            connection.SubscribeToStreamFrom(
                settings: new CatchUpSubscriptionSettings(20, 20, false, false, nameof(NewCustomerRegistered)),
                lastCheckpoint: lastProcessed,
                stream: nameof(NewCustomerRegistered), 
                eventAppeared: async (sub, e) => {                
                    await File.WriteAllTextAsync(lastProcessedFile, $"{e.Event.EventNumber}");
                    Console.WriteLine("Read event with data: {0}, metadata: {1}",
                        Encoding.UTF8.GetString(e.Event.Data),
                        Encoding.UTF8.GetString(e.Event.Metadata));    

                }, 
                subscriptionDropped: (sub, dropReason, ex) =>
                {
                    Console.WriteLine(dropReason);
                    Console.WriteLine(ex?.ToString());
                }); 
            }
        }

        static async Task ReadLiveStream()
        {
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));

            await connection.ConnectAsync();

            await connection.SubscribeToStreamAsync(nameof(NewCustomerRegistered), false,
            eventAppeared: async (sub, e) => {
                 Console.WriteLine("Read event with data: {0}, metadata: {1}",
                    Encoding.UTF8.GetString(e.Event.Data),
                    Encoding.UTF8.GetString(e.Event.Metadata));    

            }, subscriptionDropped: (sub, dropReason, ex) =>
            {
                Console.WriteLine(dropReason);
                Console.WriteLine(ex?.ToString());
            }); 
        }
    }
}
