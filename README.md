# HP Printer Monitor

A .NET Class Library for monitoring HP Printer telemetry using the SNMP protocol.

## 🚀 Features

- **Total Page Count**: Retrieves the total number of pages printed by the device.
- **Supply Monitoring**: Dynamically detects and retrieves ink/toner levels for available cartridges.
- **Asynchronous Design**: Built with `async/await` to ensure non-blocking network I/O.
- **Robustness**: Implements safe parsing and timeout handling to prevent application crashes during network failures.

## 📦 Installation

Add the library to your project and install the following dependency:

```bash
dotnet add package Lextm.SharpSnmpLib
```

## 🛠️ Usage

```csharp
using HpPrinterMonitor;

var client = new PrinterSnmpClient("192.168.1.100", community: "public");
var printerData = await client.GetPrinterDataAsync();

Console.WriteLine($"Printer IP: {printerData.Ip}");
Console.WriteLine($"Total Pages: {printerData.TotalPages}");

foreach (var supply in printerData.Supplies)
{
    Console.WriteLine($"{supply.Color}: {supply.LevelPercentage}%");
}
```

## ⚙️ Configuration

| Parameter | Description | Default |
|------------|-------------|---------|
| `ipAddress` | IP of the printer | Required |
| `community` | SNMP community string | `"public"` |
| `timeoutMs` | Request timeout in ms | `3000` |

## 📜 License

This project is licensed under the [MIT License](LICENSE).
