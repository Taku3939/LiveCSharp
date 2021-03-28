using StreamServer;
using Udp;
using UnityEditor;
using UnityEngine;

public class GenerateClient : MonoBehaviour
{
    [MenuItem("Udp/GenerateSetting")]
    public static void GenerateSetting()
    {
        for (int i = 0; i < 10; ++i)
        {
            DataHolder dataHolder = ScriptableObject.CreateInstance<DataHolder>();
            UdpSocketHolder clientSetting = ScriptableObject.CreateInstance<UdpSocketHolder>();
            clientSetting.serverIpAddress = "127.0.0.1";
            clientSetting.serverPort = 5577;
            AssetDatabase.CreateAsset(dataHolder, $"Assets/Resources/AutoGenerate/Gen_ModelManager{i}.asset");
            AssetDatabase.CreateAsset(clientSetting, $"Assets/Resources/AutoGenerate/Gen_ClientSetting{i}.asset");
            AssetDatabase.SaveAssets();
        }
    }
    
    [MenuItem("Udp/GenerateClient")]
    public static void GenerateInstance()
    {
        for (int i = 0; i < 10; ++i)
        {
            DataHolder dataHolder = ScriptableObject.CreateInstance<DataHolder>();
            UdpSocketHolder clientSetting = ScriptableObject.CreateInstance<UdpSocketHolder>();
            clientSetting.serverIpAddress = "127.0.0.1";
            clientSetting.serverPort = 5577;
            AssetDatabase.CreateAsset(dataHolder, $"Assets/Resources/AutoGenerate/Gen_ModelManager{i}.asset");
            AssetDatabase.CreateAsset(clientSetting, $"Assets/Resources/AutoGenerate/Gen_ClientSetting{i}.asset");
            AssetDatabase.SaveAssets();
        }
    }
}
