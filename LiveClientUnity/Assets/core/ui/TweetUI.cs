using Auth.Twitter;
using UnityEngine;
using UnityEngine.UI;

public class TweetUI : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private string message;

    private void Start()
    {
        this._button.onClick.AddListener(TweetButtonClicked);
    }

    private void TweetButtonClicked()
    {
        NOauth.Instance.Tweet(message);
    }
}
