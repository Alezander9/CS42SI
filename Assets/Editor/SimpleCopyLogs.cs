using UnityEditor;
using UnityEngine;
using System.Text;
using System.Reflection;

public static class SimpleCopyLogs
{
    [MenuItem("Tools/Copy All Console Logs to Clipboard")]
    private static void CopyLogs()
    {
        EditorApplication.delayCall += PerformCopy;
    }

    private static void PerformCopy()
    {
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var logEntriesType = assembly.GetType("UnityEditor.LogEntries");
        var logEntryType = assembly.GetType("UnityEditor.LogEntry");

        var getCount = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public);
        var getEntry = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
        var count = (int)getCount.Invoke(null, null);

        var logEntry = System.Activator.CreateInstance(logEntryType);
        var messageField = logEntryType.GetField("message", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var stackStartField = logEntryType.GetField("callstackTextStartUTF8", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        var sb = new StringBuilder();

        for (int i = 0; i < count; i++)
        {
            getEntry.Invoke(null, new object[] { i, logEntry });
            var message = messageField.GetValue(logEntry)?.ToString();

            if (string.IsNullOrEmpty(message)) continue;

            var stackStart = (int)stackStartField.GetValue(logEntry);
            if (stackStart > 0 && stackStart < message.Length)
            {
                message = message.Substring(0, stackStart).TrimEnd();
            }

            sb.AppendLine(message);
            sb.AppendLine();
        }

        EditorGUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log($"Copied {count} log(s) to clipboard");
    }
}