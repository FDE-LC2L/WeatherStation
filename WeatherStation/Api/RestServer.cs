using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Net;
using WeatherStation.Model;

namespace WeatherStation.Api
{
    /// <summary>
    /// Represents a REST API server that hosts HTTP endpoints for receiving and managing weather station sensor data.
    /// This server provides a web API using ASP.NET Core with Swagger documentation support.
    /// </summary>
    /// <remarks>
    /// The server exposes the following endpoints:
    /// <list type="bullet">
    /// <item><description>GET /api/status - Returns the current server status and timestamp</description></item>
    /// <item><description>POST /api/sensor-data - Receives sensor data from weather station devices</description></item>
    /// </list>
    /// The server can be started on a specific IP address and port, and notifies subscribers through the 
    /// <see cref="SensorDataReceived"/> event when new sensor data is received.
    /// </remarks>
    public class RestServer
    {
        #region Fields
        public EventHandler<SensorData>? SensorDataReceived;

        private IHost? _host;
        #endregion

        /// <summary>
        /// Starts the REST API server asynchronously on the specified IP address and port.
        /// </summary>
        /// <param name="ip">The IP address on which the server will listen for incoming HTTP requests.</param>
        /// <param name="port">The port number for the server. Defaults to 5000 if not specified.</param>
        /// <remarks>
        /// This method configures and launches an ASP.NET Core web host with Swagger documentation.
        /// It defines the following endpoints:
        /// <list type="bullet">
        /// <item><description>GET /api/status: Returns the current server status and timestamp.</description></item>
        /// <item><description>POST /api/sensor-data: Receives sensor data from weather station devices and triggers the <see cref="SensorDataReceived"/> event.</description></item>
        /// </list>
        /// </remarks>
        public async Task StartAsync(IPAddress ip, int port = 5000)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls($"http://{ip}:{port}");
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddEndpointsApiExplorer();
                        services.AddSwaggerGen();
                    })
                    .Configure(app =>
                    {
                        app.UseSwagger();
                        app.UseSwaggerUI();

                        app.UseRouting();

                        // Define endpoints here
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/api/status", () =>
                              new { Status = "OK", Timestamp = DateTime.Now }
                            );

                            // Endpoint to receive sensor data
                            endpoints.MapPost("/api/sensor-data", async (HttpContext context) =>
                            {
                                using var reader = new StreamReader(context.Request.Body);
                                var requestBody = await reader.ReadToEndAsync();
                                var sensorData = System.Text.Json.JsonSerializer.Deserialize<SensorData>(requestBody);
                                if (sensorData != null)
                                {
                                    // Trigger the event to notify subscribers
                                    SensorDataReceived?.Invoke(this, sensorData);
                                }
                                return Results.Ok(new { Delay = 10 });
                            });
                        });
                    });
                })
                .Build();

            await _host.StartAsync();
        }

        public async Task StopAsync()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }
    }
}
