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

    public PrinterSnmpClient(string ipAddress, string community = "public", int timeoutMs = 3000)
    {
        _ipAddress = ipAddress;
        _community = community;
        _timeoutMs = timeoutMs;
    }

    /// <summary>
    /// Retrieves comprehensive printer telemetry including total page counts, 
    /// color/mono breakdowns, and current supply levels using SNMP.
    /// </summary>
    public async Task<Printer> GetPrinterDataAsync()
    {
        var endpoint     = new IPEndPoint(IPAddress.Parse(_ipAddress), 161);
        var communityStr = new OctetString(_community);

        // 1. Retrieve total engine cycles using standard Printer-MIB (RFC 3805)
        string? totalPagesStr = await GetSingleOidAsync(endpoint, communityStr, HpPrinterOids.TotalPages);
        int totalPages = TryParseInt(totalPagesStr);

        // 2. Retrieve color and mono page breakdowns using HP proprietary OIDs
        // Validated on HP Color LaserJet Flow models (e.g., E57540, E78635)
        string? colorStr = await GetSingleOidAsync(endpoint, communityStr, HpPrinterOids.TotalColorPages);
        string? monoStr  = await GetSingleOidAsync(endpoint, communityStr, HpPrinterOids.TotalMonoPages);

        int? colorPages = colorStr is not null && int.TryParse(colorStr, out int c) && c >= 0 ? c : null;
        int? monoPages  = monoStr  is not null && int.TryParse(monoStr,  out int m) && m >= 0 ? m : null;

        // 3. Iterate through possible supply slots to retrieve names and current levels
        var supplies = new List<Supply>();
        for (int i = 1; i <= HpPrinterOids.MaxSupplies; i++)
        {
            try
            {
                string? name = await GetSingleOidAsync(endpoint, communityStr, HpPrinterOids.GetSupplyNameOid(i));
                if (string.IsNullOrEmpty(name)) break;

                string? levelStr = await GetSingleOidAsync(endpoint, communityStr, HpPrinterOids.GetSupplyLevelOid(i));
                int level = Math.Max(0, TryParseInt(levelStr));

                supplies.Add(new Supply(name.Trim('\0', ' '), level));
            }
            catch { break; }
        }

        return new Printer(_ipAddress, DateTime.UtcNow, totalPages, supplies, colorPages, monoPages);
    }

    /// <summary>
    /// Performs a single SNMP GET request for the specified OID.
    /// </summary>
    /// <returns>The value of the OID as a string, or null if the object is not found, 
    /// the request times out, or an SNMP error occurs.</returns>
    private async Task<string?> GetSingleOidAsync(IPEndPoint endpoint, OctetString community, string oid)
    {
        try
        {
            var variables = new List<Variable> { new Variable(new ObjectIdentifier(oid)) };
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
            return null;
        }
    }

    private static int TryParseInt(string? value)
        => int.TryParse(value, out int result) ? result : 0;
}
