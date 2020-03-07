using System;
using System.Runtime.Loader;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Newtonsoft.Json;
using Npgsql;
using Npgsql.NameTranslation;
using Serilog;

namespace DotNetDataRecorder
{
    class Program
    {
        static int _counter;

        const string DefaultApiVersion = "2018-06-28";
        const string IotEdgedUriVariableName = "IOTEDGE_WORKLOADURI";
        const string IotHubHostnameVariableName = "IOTEDGE_IOTHUBHOSTNAME";
        const string GatewayHostnameVariableName = "IOTEDGE_GATEWAYHOSTNAME";
        const string DeviceIdVariableName = "IOTEDGE_DEVICEID";
        const string ModuleIdVariableName = "IOTEDGE_MODULEID";
        const string ModuleGenerationIdVariableName = "IOTEDGE_MODULEGENERATIONID";
        const string AuthSchemeVariableName = "IOTEDGE_AUTHSCHEME";
        const string SasTokenAuthScheme = "SasToken";
        const string EdgehubConnectionstringVariableName = "EdgeHubConnectionString";
        const string IothubConnectionstringVariableName = "IotHubConnectionString";
        const string EdgeCaCertificateFileVariableName = "EdgeModuleCACertificateFile";

        static void Main(string[] args)
        {
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                //  .WriteTo.Trace()
                .CreateLogger();

            Log.Information("DotNetDataRecorder");

            Log.Information(Environment.GetEnvironmentVariable("IOTEDGE_WORKLOADURI"));
            Log.Information(Environment.GetEnvironmentVariable("IOTEDGE_IOTHUBHOSTNAME"));
            Log.Information(Environment.GetEnvironmentVariable("IOTEDGE_GATEWAYHOSTNAME"));
            Log.Information(Environment.GetEnvironmentVariable("EdgeHubConnectionString"));
            Log.Information(Environment.GetEnvironmentVariable("IotHubConnectionString"));
            Log.Information(Environment.GetEnvironmentVariable("EdgeModuleCACertificateFile"));
                        

            try
            {

                Init().Wait();

                // Wait until the app unloads or is cancelled
                var cts = new CancellationTokenSource();
                AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
                Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
                WhenCancelled(cts.Token).Wait();

            }
            catch (Exception ex)
            {
                Log.Warning(ex.Message);
                Log.Fatal(ex, "Host terminated unexpectedly");
                return;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }


        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        ///  
        /// </summary>
        static async Task Init()
        {
            string connection = Environment.GetEnvironmentVariable("EdgeHubConnectionString", EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(connection))
            {
                connection = Environment.GetEnvironmentVariable("EdgeHubConnectionString", EnvironmentVariableTarget.User);
            }
            try
            {
                if (string.IsNullOrEmpty(connection))
                {
                    connection = Environment.GetEnvironmentVariable("EdgeHubConnectionString", EnvironmentVariableTarget.Machine);
                }
            }
            catch { }

            // if (!string.IsNullOrEmpty(connection))
            // {
            //     if (connection.Contains("LocalSimulator"))
            //     {
            //         connection = null;
            //     }
            // }

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient;
            if (!string.IsNullOrEmpty(connection) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CreateFromConnectionString") ))
            {
                Log.Information($"CreateFromConnectionString ${connection}");
                
                ioTHubModuleClient = ModuleClient.CreateFromConnectionString(connection);
            }
            else
            {
                var connectionSettings = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
                //connectionSettings.CleanSession = true;
                //connectionSettings.ValidateServerCertificate = ValidateServerCertificate;
                connectionSettings.RemoteCertificateValidationCallback = ValidateServerCertificate;
                ITransportSettings[] settings = { connectionSettings };
                Log.Information($"CreateFromEnvironmentAsync");
                ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync();
            }

             try{
                await ioTHubModuleClient.OpenAsync();
            }
            catch(Exception ex){
                Log.Error(ex, "await ioTHubModuleClient.OpenAsync();");
                throw;
            }

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);

        }

        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            return true;
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            int counterValue = Interlocked.Increment(ref _counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);

            Log.Information($"received:{messageString}");

            if (!string.IsNullOrEmpty(messageString))
            {  try{
                    Log.Information($"Receiving Message: {messageString.TrimStart('"').TrimEnd('"').Replace('\\', ' ')}");
       
                    Payload payload = JsonConvert.DeserializeObject<Payload>
                        (messageString.TrimStart('"').TrimEnd('"').Replace("\\",String.Empty));

                    await SaveData(payload);

                } catch (AggregateException ex){
                    Log.Error($"Error processing message: {ex.Flatten()}");
                } catch (Exception ex){
                    Log.Error($"Error processing message: {ex}");
                }
            }

            return await Task.FromResult(MessageResponse.Completed);
        }


        static async Task SaveData(Payload p)
        {

            try
            {
                var connString = "Server=timescaledb;Port=5432;Database=postgres;User Id=postgres;Password=m5asuFHqBE;";

                using (var conn = new NpgsqlConnection(connString))
                {
                    await conn.OpenAsync();
                    // Insert some data
                    using (var cmd =
                        new NpgsqlCommand(
                            $"insert into Table_001 VALUES ('{p.TimeStamp}', '{Convert.ToInt16(p.IsAirConditionerOn)}','{p.Temperature}','{p.TagKey}')",
                            conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Warning($"Error SaveData message: {e}");
            }
        }

    }
}
