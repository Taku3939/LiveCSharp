// Created by Takuya Isaki on 2021/03/09

using System.Threading.Tasks;
using Auth.Twitter;
using StreamServer;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Auth
{
    /// <summary>
    /// This class use for twitter authentication
    /// </summary>
    public class AuthUI : MonoBehaviour
    {

        /// <summary>
        /// This Button is to open authenticate page
        /// </summary>
        [SerializeField] private Button OpenAuthPageButton;

        /// <summary>
        /// This button is to verify the obtained pin code
        /// </summary>
        [SerializeField] private Button PincodeVerificationButton;

        /// <summary>
        /// This field is to input pincode
        /// </summary>
        [SerializeField] private InputField pincodeField;

        /// <summary>
        /// This scene of name will open on successful authentication 
        /// </summary>
        [SerializeField] private string sceneName;
        
        [SerializeField] private DataHolder dataHolder;
        public void Start()
        {
            NOauth oauth = NOauth.Instance;
            this.OpenAuthPageButton.onClick.AddListener(() => oauth.OpenAuthSite());
            this.PincodeVerificationButton.onClick.AddListener(async () =>
            {
                if (oauth.AuthorizeVerification(pincodeField.text, out var id))
                {
                    var obj = await NOauth.GetIcon(long.Parse(id));
                    dataHolder.selfId = obj.userId;
                    dataHolder.screenName = obj.screenName;
                    await OnSuccess();
                }
            });
        }
        /// <summary>
        /// This method must be called success of authentication
        /// </summary>
        /// <returns></returns>
        private async Task OnSuccess()
        {
            var handle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            var instance = await handle.Task;
        }
    }
}