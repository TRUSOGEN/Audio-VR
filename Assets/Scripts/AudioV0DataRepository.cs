using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Owns the participant-data folder affordances used by the runtime UI.
/// It only clears technical-demo data and refuses to remove a session whose log is still open.
/// Pilot and participant-run categories are deliberately outside this deletion scope.
/// </summary>
public sealed class AudioV0DataRepository : MonoBehaviour
{
    public EventLogger logger;

    public string RootPath => EventLogger.GetDataRootPath();
    public string TechnicalDemoPath => Path.Combine(RootPath, "technical_demo");
    public int TechnicalSessionCount => CountSessionDirectories(TechnicalDemoPath) + CountLegacySessionDirectories();

    /// <summary>Opens the root data folder with the operating system's file browser.</summary>
    public bool OpenDataFolder()
    {
        try
        {
            Directory.CreateDirectory(RootPath);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = "\"" + RootPath + "\"", UseShellExecute = true });
#else
            Application.OpenURL(new Uri(RootPath).AbsoluteUri);
#endif
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("[AudioV0DataRepository] OPEN_DATA_FOLDER_FAILED: " + exception.Message);
            return false;
        }
    }

    /// <summary>Deletes only completed technical-demo sessions, including the prior flat technical-demo layout.</summary>
    public bool DeleteTechnicalDemoData()
    {
        if (logger != null && logger.IsReady && string.Equals(logger.storageCategory, "technical_demo", StringComparison.OrdinalIgnoreCase))
        {
            logger.Log("data_delete_rejected", details: "scope=technical_demo;reason=active_session_log_open");
            return false;
        }

        try
        {
            if (Directory.Exists(TechnicalDemoPath)) Directory.Delete(TechnicalDemoPath, true);
            if (Directory.Exists(RootPath))
            {
                foreach (string path in Directory.GetDirectories(RootPath, "session_*", SearchOption.TopDirectoryOnly))
                {
                    Directory.Delete(path, true);
                }
            }
            Directory.CreateDirectory(TechnicalDemoPath);
            Debug.Log("[AudioV0DataRepository] technical_demo data cleared: " + TechnicalDemoPath);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("[AudioV0DataRepository] DELETE_TECHNICAL_DATA_FAILED: " + exception.Message);
            return false;
        }
    }

    private static int CountSessionDirectories(string directory)
    {
        return Directory.Exists(directory) ? Directory.GetDirectories(directory, "session_*", SearchOption.TopDirectoryOnly).Length : 0;
    }

    private int CountLegacySessionDirectories()
    {
        return CountSessionDirectories(RootPath);
    }
}
