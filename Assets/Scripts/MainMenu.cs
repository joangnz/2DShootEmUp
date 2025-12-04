using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField tmpInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void SetUsername(string username)
    {
        PlayerPrefs.SetString("username", username);
    }

    public void Play()
    {
        SceneManager.LoadScene("SampleScene");
        SetUsername(tmpInput.text);
    }
}
