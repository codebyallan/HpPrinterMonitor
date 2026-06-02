namespace HpPrinterMonitor.Models;

public record Printer(
    string Ip,
    DateTime TimeStamp,
    int TotalPages,
    List<Supply> Supplies,
    int? MonoPages = null,  // null = printer does not expose color counter via SNMP
    int? ColorPages  = null   // null = printer does not expose mono counter via SNMP
);
