namespace HpPrinterMonitor.Constants;

/// <summary>
/// Standard OIDs for HP Printers (RFC 3805 Printer-MIB + HP proprietary extensions).
/// </summary>
public static class HpPrinterOids
{
    // ── RFC 3805 Standard Printer-MIB ─────────────────────────────────────────────
    // prtMarkerLifeCount OID structure: 1.3.6.1.2.1.43.10.2.1.4.<hrDeviceIndex>.<markerIndex>
    // For the tested HP models, only Marker 1 is typically exposed via the standard MIB.

    /// <summary>Total engine cycles (all pages printed). Standard RFC 3805 OID with universal HP support.</summary>
    public const string TotalPages = "1.3.6.1.2.1.43.10.2.1.4.1.1";

    // ── HP Proprietary OIDs — Verified on HP Color LaserJet Flow E57540 and E78635 ────────────────
    // Discovered via SNMP walk of the 1.3.6.1.4.1.11.2.3.9.4.2.1.4 tree.
    // NOTE: These OIDs end with .0 (scalar instance), which is mandatory for SNMP GET operations.
    //       Requests without the trailing .0 typically result in a NoSuchInstance error.

    /// <summary>
    /// Total COLOR pages printed.
    /// Verified on HP Color LaserJet Flow E57540 and E78635.
    /// Validation: ColorPages + MonoPages should equal TotalPages.
    /// </summary>
    public const string TotalColorPages = "1.3.6.1.4.1.11.2.3.9.4.2.1.4.1.2.6.0";

    /// <summary>
    /// Total MONO (black) pages printed.
    /// Verified on HP Color LaserJet Flow E57540 and E78635.
    /// </summary>
    public const string TotalMonoPages = "1.3.6.1.4.1.11.2.3.9.4.2.1.4.1.2.7.0";

    /// <summary>
    /// Total pages — alternate OID in HP proprietary area (matches prtMarkerLifeCount).
    /// Confirmed value: 30725 == ColorPages(9586) + MonoPages(21139).
    /// </summary>
    public const string TotalPagesHp = "1.3.6.1.4.1.11.2.3.9.4.2.1.4.1.2.53.0";

    // ── Supply Base OIDs (RFC 3805 standard) ──────────────────────────────────────
    private const string SupplyNameBase  = "1.3.6.1.2.1.43.11.1.1.6.1";
    private const string SupplyLevelBase = "1.3.6.1.2.1.43.11.1.1.9.1";

    /// <summary>Maximum number of supplies to attempt to retrieve.</summary>
    public const int MaxSupplies = 10;

    public static string GetSupplyNameOid(int index)  => $"{SupplyNameBase}.{index}";
    public static string GetSupplyLevelOid(int index) => $"{SupplyLevelBase}.{index}";
}
