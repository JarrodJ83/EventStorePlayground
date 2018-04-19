using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Producer.Events;

namespace Producer
{
    class Program
    {
        static void Main(string[] args)
        {
             RegisterRandomCustomers().Wait();
        }

        static async Task RegisterRandomCustomers()
        {
            using(var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113)))
            {
                await connection.ConnectAsync();

                while(true)
                {
                    var customerName = $"Customer_{Guid.NewGuid()}";
                    var customerDateOfBirth = DateTime.Now.Subtract(new TimeSpan(1, 1, 1, 1));
                    Console.WriteLine($"{customerName} registered");
                    var customerRegistered = new NewCustomerRegistered(customerName, customerDateOfBirth);
                    var myEvent = new EventData(Guid.NewGuid(), nameof(NewCustomerRegistered), true,
                                                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(customerRegistered)),
                                                Encoding.UTF8.GetBytes(""));

                    await connection.AppendToStreamAsync(nameof(NewCustomerRegistered), ExpectedVersion.Any, myEvent);
                    await Task.Delay(10000);
                }           
            }
        }
    }
}
