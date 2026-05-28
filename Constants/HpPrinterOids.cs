namespace HpPrinterMonitor.Constants;

/// <summary>
/// Standard OIDs for HP Printers.
/// </summary>
public static class HpPrinterOids
{
    // Fixed OID: Total Pages Printed
    public const string TotalPages = "1.3.6.1.2.1.43.10.2.1.4.1.1";

    // Supply Base OIDs
    private const string SupplyNameBase = "1.3.6.1.2.1.43.11.1.1.6.1";
    private const string SupplyLevelBase = "1.3.6.1.2.1.43.11.1.1.9.1";

    /// <summary>
    /// Maximum number of supplies to attempt to retrieve.
    /// </summary>
    public const int MaxSupplies = 10;

    // Utility methods
    public static string GetSupplyNameOid(int index) => $"{SupplyNameBase}.{index}";
    public static string GetSupplyLevelOid(int index) => $"{SupplyLevelBase}.{index}";
}
