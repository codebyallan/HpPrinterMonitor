namespace HpPrinterMonitor.Models;

public record Printer(
    string Ip,
    DateTime TimeStamp,
    int TotalPages,
    List<Supply> Supplies
);