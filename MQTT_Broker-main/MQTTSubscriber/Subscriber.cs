using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MQTTSubscriber
{
    class Subscriber
    {
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();
            IMqttClient client = mqttFactory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                        .WithClientId(Guid.NewGuid().ToString())
                        .WithTcpServer("test.mosquitto.org", 1883)
                        .WithCleanSession()
                        .Build();
            client.UseConnectedHandler(async e =>
            {
                Console.WriteLine("Conectado ao broker");
                var topicFilter = new TopicFilterBuilder()   
                            .WithTopic("BrokerTeste")
                            .Build();

                await client.SubscribeAsync(topicFilter); 
            });

            client.UseDisconnectedHandler(e =>
            {
                Console.WriteLine("Desconectado do broker");
            });

            client.UseApplicationMessageReceivedHandler(async e =>  
            {
                var Recebido = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Console.WriteLine($"mensagem recebida {Recebido}");
                if(Recebido!=null) 
                {
                    var httpCliente = new HttpClient();
                    var objeto = new { mensagem = Recebido };
                    var content = ToRequest(objeto);
                    var response = await httpCliente.PostAsync("https://localhost:8080/Mensagem", content);

                }

            });

            
            await client.ConnectAsync(options);

            Console.ReadLine();

            await client.DisconnectAsync();


        }

  
        private static StringContent ToRequest(object obj) 
        {
            var json = JsonConvert.SerializeObject(obj);
            var formatado = new StringContent(json, Encoding.UTF8, "application/json");
            return formatado;
        }
    }
}
