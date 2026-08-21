using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using SophiaWin11.Core.Abstractions;

namespace SophiaWin11.Core.Services;

[SupportedOSPlatform("windows")]
public sealed class RestorePointService : IRestorePointService
{
    private const uint BeginSystemChange = 100;
    private const uint EndSystemChange = 101;
    private const uint ModifySettings = 12;
    private const int MaxDescriptionLength = 256;
    private const uint ErrorSuccess = 0;

    private readonly ILogger<RestorePointService> _logger;

    public RestorePointService(ILogger<RestorePointService> logger)
    {
        _logger = logger;
    }

    public Task<bool> CreateRestorePointAsync(string description, CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("System Restore is only available on Windows.");
        }

        return Task.Run(() => CreateRestorePointCore(description), cancellationToken);
    }

    private bool CreateRestorePointCore(string description)
    {
        try
        {
            var beginInfo = new RestorePointInfo
            {
                dwEventType = BeginSystemChange,
                dwRestorePtType = ModifySettings,
                llSequenceNumber = 0,
                szDescription = Truncate(description),
            };

            if (!SRSetRestorePointW(ref beginInfo, out var beginStatus))
            {
                _logger.LogWarning(
                    "Failed to begin a System Restore point. Status code: {Status}.",
                    beginStatus.nStatus);
                return false;
            }

            var endInfo = new RestorePointInfo
            {
                dwEventType = EndSystemChange,
                dwRestorePtType = ModifySettings,
                llSequenceNumber = beginStatus.llSequenceNumber,
                szDescription = Truncate(description),
            };

            if (!SRSetRestorePointW(ref endInfo, out var endStatus))
            {
                _logger.LogWarning(
                    "Failed to finalize the System Restore point. Status code: {Status}.",
                    endStatus.nStatus);
                return false;
            }

            var succeeded = endStatus.nStatus == ErrorSuccess;
            _logger.LogInformation(
                "System Restore point '{Description}' created (sequence {Sequence}). Success: {Succeeded}.",
                description,
                endStatus.llSequenceNumber,
                succeeded);

            return succeeded;
        }
        catch (DllNotFoundException ex)
        {
            _logger.LogWarning(ex, "srclient.dll is not available on this system; System Restore point was not created.");
            return false;
        }
        catch (EntryPointNotFoundException ex)
        {
            _logger.LogWarning(ex, "SRSetRestorePointW entry point was not found; System Restore point was not created.");
            return false;
        }
    }

    private static string Truncate(string description) =>
        description.Length > MaxDescriptionLength - 1 ? description[..(MaxDescriptionLength - 1)] : description;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct RestorePointInfo
    {
        public uint dwEventType;
        public uint dwRestorePtType;
        public long llSequenceNumber;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxDescriptionLength)]
        public string szDescription;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct StateManagerStatus
    {
        public uint nStatus;
        public long llSequenceNumber;
    }

    [DllImport("srclient.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SRSetRestorePointW(ref RestorePointInfo restorePointInfo, out StateManagerStatus stateManagerStatus);
}
