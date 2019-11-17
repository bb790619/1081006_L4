using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GoodGame : MonoBehaviour
{
    public Text TextLoading;
    public Image ImageLoading;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    /// <summary>
    /// 重新開始遊戲
    /// </summary>
    public void Restart(string scene)
    {
        SceneManager.LoadScene(scene);
    }
    /// <summary>
    /// 離開遊戲
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }
    /// <summary>
    /// 載入進度
    /// </summary>
    public void StartLoading()
    {
        StartCoroutine(DalayLoading());
    }
    /// <summary>
    /// 載入延遲的進度條
    /// </summary>
    /// <returns></returns>
    public IEnumerator DalayLoading()
    {
        AsyncOperation Ao = SceneManager.LoadSceneAsync("遊戲"); //載入的資料
        Ao.allowSceneActivation = false;  //取消載入

        while (Ao.isDone == false)
        {
            TextLoading.text = Ao.progress/0.9f*100 + "/100";  //因為數值只會到0.9，所以要/0.9f*100。
            ImageLoading.fillAmount = Ao.progress / 0.9f;      
            yield return new WaitForSeconds(0.1f);

            //如果數值到0.9之後，且按任意鍵，就開始遊戲
            if (Ao.progress == 0.9f && Input.anyKey)    Ao.allowSceneActivation = true;
        }

        
    }

}
