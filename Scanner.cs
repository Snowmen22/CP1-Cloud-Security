using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

public class PortScanFunction
{
    private static Dictionary<int, string> serviceMap = new Dictionary<int, string>()
    {
        { 22, "SSH" },
        { 80, "HTTP" },
        { 443, "HTTPS" },
        { 8080, "HTTP Alt" }
        // Adicione mais portas e serviços conforme necessário
    };

    [Function("PortScanFunction")]
    public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req,
                                       FunctionContext context)
    {
        var response = req.CreateResponse();
        var ports = new List<int> { 22, 80, 443, 8080 }; // Adicione as portas que deseja verificar aqui

        var result = new List<PortScanResult>();

        foreach (var port in ports)
        {
            var portScanResult = new PortScanResult
            {
                Port = port,
                IsOpen = IsPortOpen("cp.leosantos.seg.br", port),
                Service = GetService(port)
            };

            result.Add(portScanResult);
        }

        var jsonResult = JsonSerializer.Serialize(result);
        response.Headers.Add("Content-Type", "application/json");
        response.WriteString(jsonResult);
        return response;
    }

    private static bool IsPortOpen(string host, int port)
    {
        try
        {
            using (var tcpClient = new TcpClient())
            {
                tcpClient.Connect(host, port);
                return true;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string GetService(int port)
    {
        return serviceMap.ContainsKey(port) ? serviceMap[port] : "Unknown";
    }
}

public class PortScanResult
{
    public int Port { get; set; }
    public bool IsOpen { get; set; }
    public string Service { get; set; }
}
