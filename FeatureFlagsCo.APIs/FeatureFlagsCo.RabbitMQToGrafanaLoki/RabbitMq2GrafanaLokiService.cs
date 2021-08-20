﻿
using FeatureFlagsCo.RabbitMqModels;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FeatureFlagsCo.RabbitMQToGrafanaLoki
{
    public interface IRabbitMq2GrafanaLokiService
    {
        void Init();
    }
    public class RabbitMq2GrafanaLokiService: IRabbitMq2GrafanaLokiService
    {
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;
        private readonly string _rabbitHostName;
        private readonly string _lokiHostName;
        public RabbitMq2GrafanaLokiService(string rabbitHostName = "rabbitmq", string lokiHostName = "loki")
        {
            _factory = new ConnectionFactory() { HostName = rabbitHostName };
            _rabbitHostName = rabbitHostName;
            _lokiHostName = lokiHostName;

            Init();
        }
        public void Init()
        {
            if (_channel != null)
            {
                _channel.Close();
                _channel.QueueDelete("hello");
            }
            if (_connection != null)
                _connection.Close();
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("Start RabbitMq Receiver Service at " + DateTime.UtcNow.ToString());
                try
                {
                    _connection = _factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _connection.ConnectionShutdown += (sender, e) =>
                    {
                        Console.WriteLine("ConnectionShutdown: " + e.ReplyText);
                        Init();
                    };
                    _channel.ModelShutdown += (sender, e) =>
                    {
                        Console.WriteLine("ModelShutdown: " + e.ReplyText);
                        Init();
                    };


                    Console.WriteLine("Connection and channel created");

                    _channel.QueueDeclare(queue: "hello",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += async (model, ea) =>
                    {
                        Console.WriteLine("New message received");
                        string message = "";
                        try
                        {
                            var body = ea.Body.ToArray();
                            message = Encoding.UTF8.GetString(body);
                            Console.WriteLine(message);
                            var messageModel = JsonConvert.DeserializeObject<MessageModel>(message);
                            await WriteToGrafanaLokiAsync(messageModel);
                        }
                        catch (AggregateException aexp)
                        {
                            Console.WriteLine("New message exception:");
                            Console.WriteLine(aexp.Message);
                            Console.WriteLine(message);
                        }
                        catch (Exception exp)
                        {
                            Console.WriteLine("New message exception:");
                            Console.WriteLine(exp.Message);
                            Console.WriteLine(message);
                        }

                    };
                    _channel.BasicConsume(queue: "hello",
                                         autoAck: true,
                                         consumer: consumer);

                    break;
                }
                catch (Exception exp)
                {
                    Console.WriteLine($"{i} times. Connection failed:" + exp.Message);
                    Thread.Sleep(30 * 1000);
                }
            }
        }

        private async Task WriteToGrafanaLokiAsync(MessageModel message)
        {
            Console.WriteLine("WriteToGrafanaLokiAsync");
            var streams = new ExpandoObject();
            if (message.Labels != null && message.Labels.Count > 0)
            {
                foreach (var lable in message.Labels)
                {
                    streams.TryAdd(lable.LabelName, lable.LabelValue);
                }
            }
            dynamic bodyCore = new
            {
                stream = streams,
                values = new List<dynamic>()
                            {
                                new List<string>()
                                {
                                    (((DateTimeOffset)message.SendDateTime).ToUnixTimeMilliseconds()).ToString() + "000000",
                                    message.Message
                                }
                            }
            };
            dynamic body = new
            {
                streams = new List<dynamic> {
                        bodyCore
                    }
            };
            int i = 0;
            while (i < 5)
            {
                Console.WriteLine("Sending message to loki service");
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(body));
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    //由HttpClient发出异步Post请求
                    HttpResponseMessage res = await client.PostAsync($"http://{_lokiHostName}:3100/loki/api/v1/push", content);
                    if (res.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        Console.WriteLine("Message Sent.");
                        break;
                    }
                    await Task.Delay(500);
                }
                i++;
            }
        }
    }
}
