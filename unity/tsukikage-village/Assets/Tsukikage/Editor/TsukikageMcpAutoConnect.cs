using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

namespace Tsukikage.EditorTools
{
    /// <summary>
    /// エディタロード時に MCP for Unity のブリッジを自動起動する。
    /// （Window > MCP For Unity の Connect ボタンと同じ処理）
    /// </summary>
    [InitializeOnLoad]
    public static class TsukikageMcpAutoConnect
    {
        static TsukikageMcpAutoConnect()
        {
            EditorApplication.delayCall += Connect;
        }

        static async void Connect()
        {
            try
            {
                EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", true);
                EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);

                if (MCPServiceLocator.Bridge.IsRunning)
                {
                    Debug.Log("[Tsukikage] MCP bridge already running.");
                    return;
                }
                bool ok = await MCPServiceLocator.Bridge.StartAsync();
                Debug.Log("[Tsukikage] MCP bridge auto-connect: " + (ok ? "OK" : "FAILED"));
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[Tsukikage] MCP auto-connect error: " + ex.Message);
            }
        }
    }
}
