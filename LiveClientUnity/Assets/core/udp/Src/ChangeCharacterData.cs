using Auth.Twitter;
using UnityEngine;
using UnityEngine.Networking;

namespace Udp
{
    /// <summary>
    /// キャラデータ変更
    /// </summary>
    public class ChangeCharacterData : MonoBehaviour
    {
        [SerializeField] private TextMesh _name;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private Material source;

        public async void ChangeIcon(ulong id)
        {
            var twitterObj = await NOauth.GetIcon(id);
            if (twitterObj == null) return;
            //Change Username
            _name.text = twitterObj.screenName;
            Debug.Log(twitterObj.iconPath);
            //Change UserIcon
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(twitterObj.iconPath);
            await request.SendWebRequest();

            Texture myTexture = ((DownloadHandlerTexture) request.downloadHandler).texture;
            var mat2 = new Material(source) {mainTexture = myTexture};
            _renderer.material = mat2;
        }
    }
}