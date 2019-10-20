using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{
    [Header("跳躍次數")] public int jumpCount=0;   //跳躍次數
    int jumpMax = 2; //跳躍最大次數
    [Header("跳躍高度"), Range(20, 100)] public int jump =100;   //跳躍最大高度
    [Header("移動速度"), Range(5, 15)] public float speed = 10f; //移動速度
    [Header("是否在地面"),Tooltip("")] public bool isGround;     //是否在地面上，true=在地面，false=空中
    [Header("角色名稱")] public string PlayerName = "Boy";       //角色名稱



    int CherryScore;
    int DimandScore;

    public GameObject Cam;    //攝影機
    public GameObject Player; //人物
    public Animator ani;      //人物動畫

    public Rigidbody2D rb;   //剛體
    // Start is called before the first frame update
    void Start()
    {
        CherryScore=0;
        DimandScore=0;
        jumpCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 PlayerPos = Player.transform.position;    //人物的位置
        Vector3 CamPos = Cam.transform.position;         //攝影機的位置

        //人物和攝影機自動移動
        rb.AddForce(new Vector2(speed , 0f),ForceMode2D.Force);  //人物移動
        if (rb.velocity.y > 0) ani.SetBool("跳躍", false);      //動畫控制-走路     
        Cam.transform.position = new Vector3(Mathf.Clamp(PlayerPos.x + 7f,-94f,94f ) , CamPos.y, CamPos.z);//讓攝影機一直跟著人物，而且限制攝影機的移動範圍

        //限制
        if (rb.velocity.x >= 5f ) rb.velocity = new Vector2(5f , rb.velocity.y);         //限制移動速度
                                                                                         //if (PlayerPos.y >= 3.7f) PlayerPos =new Vector3(PlayerPos.x, 3.6f, PlayerPos.z);//限制高度
        if (CamPos.x > 94f ) Cam.transform.position = new Vector3(94f, CamPos.y, CamPos.z);

        GameObject.Find("櫻桃得分").GetComponent<Text>().text =CherryScore.ToString(); //計分

        if (Player.transform.position.y < -6f) SceneManager.LoadScene("遊戲");  //掉下去就重來
        if (PlayerPos.x > 101f) Player.transform.position = new Vector3(-101f, PlayerPos.y, PlayerPos.z); //人物移動到最右邊就移到最左邊

    }

    private void OnCollisionEnter2D(Collision2D Hit)
    {
        if (Hit.collider.name == "地板")
        {
            jumpCount = 0;
            isGround = true;
        } 
        else isGround = false;


    }
    private void OnTriggerEnter2D(Collider2D Eat)
    {
        if (Eat.name == "道具")
        {
            CherryScore +=1;
        }
    }


    /// <summary>
    /// 跳躍按鍵
    /// </summary>
    public void PlayerJump() 
    {
        
        if (isGround==true && jumpCount < jumpMax) //如果碰到地面，而且跳躍次數2次以內時執行
        {
            rb.AddForce(new Vector2(0, 5f), ForceMode2D.Impulse); //向上跳躍
            ani.SetBool("跳躍", true);  //動畫控制-跳躍
            jumpCount += 1;
        }
        
    }

    
    

}
