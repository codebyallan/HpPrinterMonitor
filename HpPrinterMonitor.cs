using System.Net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using HpPrinterMonitor.Models;
using HpPrinterMonitor.Constants;

namespace HpPrinterMonitor;

/// <summary>
/// Client responsible for communicating with HP Printers via SNMP to retrieve telemetry data.
/// </summary>
public class PrinterSnmpClient
{
    private readonly string _ipAddress;
    private readonly string _community;
    private readonly int _timeoutMs;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrinterSnmpClient"/> class.
    /// </summary>
    /// <param name="ipAddress">The IP address of the target printer.</param>
    /// <param name="community">The SNMP community string (defaults to "public").</param>
    /// <param name="timeoutMs">Timeout in milliseconds for SNMP requests (defaults to 3000ms).</param>
    public PrinterSnmpClient(string ipAddress, string community = "public", int timeoutMs = 3000)
    {
        _ipAddress = ipAddress;
        _community = community;
        _timeoutMs = timeoutMs;
    }

    /// <summary>
    /// Retrieves printer data including total page count and supply levels.
    /// </summary>
    /// <returns>A <see cref="Printer"/> object containing the retrieved telemetry.</returns>
    public async Task<Printer> GetPrinterDataAsync()
    {
        var endpoint = new IPEndPoint(IPAddress.Parse(_ipAddress), 161);
        var communityStr = new OctetString(_community);

        // 1. Retrieve Total Page Count
        string? totalPagesStr = await GetSingleOidAsync(endpoint, communityStr, HpPrinterOids.TotalPages);
        int totalPages = TryParseInt(totalPagesStr);

        var supplies = new List<Supply>();

        // 2. Loop to retrieve supplies dynamically.
        // We iterate up to MaxSupplies, but stop if a supply name is not found.
        for (int i = 1; i <= HpPrinterOids.MaxSupplies; i++)
        {
            try
            {
                string? colorName = await GetSingleOidAsync(endpoint, communityStr, HpPrinterOids.GetSupplyNameOid(i));

                // If no color name is returned, we assume there are no more supplies.
                if (string.IsNullOrEmpty(colorName)) break;

                string? currentStr = await GetSingleOidAsync(endpoint, communityStr, HpPrinterOids.GetSupplyLevelOid(i));
                int currentLevel = TryParseInt(currentStr);

                // Ensure level is not negative.
                if (currentLevel < 0) currentLevel = 0;

                supplies.Add(new Supply(colorName.Trim('\0', ' '), currentLevel));
            }
            catch (Exception)
            {
                // Stop processing supplies if an unexpected error occurs.
                break;
            }
        }

        // 3. Construct the final Model
        return new Printer(_ipAddress, DateTime.Now, totalPages, supplies);
    }

    /// <summary>
    /// Performs a single SNMP GET request for a specific OID.
    /// </summary>
    /// <param name="endpoint">The target endpoint.</param>
    /// <param name="community">The community string.</param>
    /// <param name="oid">The OID to query.</param>
    /// <returns>The value as a string, or null if not found or an error occurred.</returns>
    private async Task<string?> GetSingleOidAsync(IPEndPoint endpoint, OctetString community, string oid)
    {
        try
        {
            var variables = new List<Variable> { new Variable(new ObjectIdentifier(oid)) };

            // Use CancellationToken to handle timeout properly.
            using var cts = new CancellationTokenSource(_timeoutMs);

            var result = await Messenger.GetAsync(VersionCode.V2, endpoint, community, variables, cts.Token);

            if (result.Count > 0 &&
                result[0].Data.TypeCode != SnmpType.NoSuchObject &&
                result[0].Data.TypeCode != SnmpType.NoSuchInstance)
            {
                return result[0].Data.ToString();
            }
            return null;
        }
        catch (Exception ex) when (ex is SnmpException || ex is OperationCanceledException)
        {
            // Log or handle SNMP failure or timeout silently to avoid breaking the whole collection.
            return null;
        }
    }

    /// <summary>
    /// Safely parses a string to an integer.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed integer, or 0 if parsing fails or value is null.</returns>
    private int TryParseInt(string? value)
    {
        if (int.TryParse(value, out int result))
        {
            return result;
        }
        return 0;
    }
}
