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
    public class RestApiServer
    {
        #region Fields
        public EventHandler<SensorData>? SensorDataReceived;

        private IHost? _host;
        #endregion

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
