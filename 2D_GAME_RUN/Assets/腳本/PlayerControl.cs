using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerControl : MonoBehaviour
{
    [Header("跳躍次數")] public int jumpCount = 0;   //跳躍次數
    int jumpMax = 2; //跳躍最大次數
    [Header("跳躍高度"), Range(5, 100)] public int jump = 15;   //跳躍最大高度
    [Header("移動速度"), Range(5, 1000)] public float speed = 60; //移動速度
    [Header("是否在地面"), Tooltip("")] public bool isGround;     //是否在地面上，true=在地面，false=空中
    [Header("角色名稱")] public string PlayerName = "Boy";       //角色名稱
    float PlayerHp;       //玩家現有血量
    float PlayerHpMax = 100; //玩家最大血量
    float PlayerDamage = 30;
    float TimeNow = 0;
    int CherryScore;
    int DimandScore;

    GameObject Cam;    //攝影機
    Animator ani;     //人物動畫控制
    Rigidbody2D rb;   //剛體
    CapsuleCollider2D CC2D;//碰撞器
    [Header("道具拼接圖")] public Tilemap Tilemap;
    [Header("結束視窗")] public GameObject GGWindow;

    public AudioSource Audio_Jump;  //跳躍音效
    public AudioSource Audio_Slide; //滑行音效
    public AudioSource Audio_Eat;   //吃道具音效

    // Start is called before the first frame update
    void Start()
    {
        CherryScore = 0;
        DimandScore = 0;
        jumpCount = 0;
        ani = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        CC2D = GetComponent<CapsuleCollider2D>();
        Cam = GameObject.Find("Main Camera");
        GGWindow.SetActive(false);
        PlayerHp = PlayerHpMax;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 PlayerPos = transform.position;    //人物的位置
        Vector3 CamPos = Cam.transform.position;   //攝影機的位置

        //人物和攝影機自動移動
        rb.AddForce(new Vector2(speed, 0f), ForceMode2D.Force);  //人物移動
        //if (rb.velocity.y <=0) ani.SetBool("跳躍", false);      //動畫控制-走路     
        Cam.transform.position = new Vector3(Mathf.Clamp(PlayerPos.x + 7f, -94f, 94f), CamPos.y, CamPos.z);//讓攝影機一直跟著人物，而且限制攝影機的移動範圍

        //限制
        if (rb.velocity.x >= 5f) rb.velocity = new Vector2(5f, rb.velocity.y);         //限制人物移動速度
                                                                                       //if (PlayerPos.y >= 3.7f) PlayerPos =new Vector3(PlayerPos.x, 3.6f, PlayerPos.z);//限制高度
        if (CamPos.x > 94f) Cam.transform.position = new Vector3(94f, CamPos.y, CamPos.z);//限制攝影機位置

        GameObject.Find("櫻桃得分").GetComponent<Text>().text = CherryScore.ToString(); //計分
        GameObject.Find("鑽石得分").GetComponent<Text>().text = DimandScore.ToString(); //計分

        if (transform.position.y < -6f) //掉下去就讓HP歸0
        {
            PlayerHp = 0;
            Cam.transform.position = new Vector3(CamPos.x, CamPos.y, CamPos.z);
            //Invoke("ResetGame", 2f);
        }
        if (PlayerPos.x > 101f) transform.position = new Vector3(-101f, PlayerPos.y, PlayerPos.z); //人物移動到最右邊就移到最左邊

        //血量歸0時，人物死亡
        if (PlayerHp <= 0)
        {
            ani.SetBool("死亡", true);
            speed = 0;
            StartCoroutine(GoodGame());
        }
        if (PlayerHp > 0) TimeNow += Time.deltaTime;//遊戲時間
        //人物每秒扣血
        PlayerHp -= Time.deltaTime * 10;
        GameObject.Find("血條").GetComponent<Image>().fillAmount = PlayerHp / PlayerHpMax; //血條減少

    }

    /// <summary>
    /// 掉下去就重來
    /// </summary>
    void ResetGame()
    {
        SceneManager.LoadScene("遊戲");
    }
    int Dimand = 0;
    int Cherry = 0;
    /// <summary>
    /// 人物死亡時，出現結束視窗
    /// </summary>
    private IEnumerator GoodGame()
    {
        if (GGWindow.activeSelf == false)
        {
            GGWindow.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            while (DimandScore >0)
            {
                DimandScore--;
                Dimand += 100;
                GGWindow.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = Dimand.ToString();
                yield return new WaitForSeconds(0.5f);
            }
            while (CherryScore > 0)
            {
                CherryScore--;
                Cherry += 50;
                GGWindow.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = Cherry.ToString();
                yield return new WaitForSeconds(0.5f);
            }
            GGWindow.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = ((int)TimeNow+"秒").ToString();
            yield return new WaitForSeconds(0.5f);
            GGWindow.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = (Dimand+ Cherry).ToString();

        }
    }

    /// <summary>
    /// "碰到"地板或道具時時觸發
    /// </summary>
    /// <param name="Hit"></param>
    private void OnCollisionEnter2D(Collision2D Hit)
    {
        if (Hit.collider.name == "地板") //碰到地板才能再跳躍，並且跳躍計數器歸0
        {
            jumpCount = 0;
            isGround = true;
        }
        else isGround = false;

        if (Hit.collider.name == "道具")
        {
            EatCount(Hit);
        }
    }

    /// <summary>
    /// 吃道具
    /// </summary>
    public void EatCount(Collision2D Hit)
    {
        Vector3 HitPoint = Vector3.zero;  //碰到道具的座標(先初始化)
        HitPoint = Hit.contacts[0].point; //計算碰到的位置座標
        Vector3 Normal = Hit.contacts[0].normal; //碰到道具的法線座標
        Vector3 EatPos = Vector3.zero;           //碰到道具的座標

        //計算方式，道具座標=碰到座標- 0.01*法線座標
        EatPos.x = HitPoint.x - 0.01f * Normal.x;
        EatPos.y = HitPoint.y - 0.01f * Normal.y;
        //消除道具(讓道具顯示變空)
        Tilemap.SetTile(Tilemap.WorldToCell(EatPos), null);
    }

    /// <summary>
    /// "穿透"障礙物時觸發
    /// </summary>
    /// <param name="Eat"></param>
    private void OnTriggerEnter2D(Collider2D Eat)
    {
        if (Eat.name == "障礙物")//碰到障礙物扣血
        {
            Damage();
        }
        EatDimand(Eat);
    }

    public void EatDimand(Collider2D Eat)
    {
        if (Eat.tag == "鑽石")
        {
            Destroy(Eat.gameObject);
            DimandScore += 1;
        }
        else if (Eat.tag == "櫻桃")
        {
            Destroy(Eat.gameObject);
            CherryScore += 1;
        }
        Audio_Eat.Play();
    }

    /// <summary>
    /// 碰到障礙物時扣血
    /// </summary>
    void Damage()
    {
        PlayerHp -= PlayerDamage; //扣血
        GameObject.Find("血條").GetComponent<Image>().fillAmount = PlayerHp / PlayerHpMax; //血條減少
        GetComponent<SpriteRenderer>().enabled = false;    //關閉人物圖片
        Invoke("AppearSprite", 0.1f);                      //延遲開啟人物圖片
    }

    /// <summary>
    /// 開啟人物圖片
    /// </summary>
    void AppearSprite()
    {
        GetComponent<SpriteRenderer>().enabled = true;
    }

    /// <summary>
    /// 跳躍按鍵
    /// </summary>
    public void PlayerJump()
    {
        if (PlayerHp > 0)
        {
            if (isGround == true && jumpCount < jumpMax) //如果碰到地面，而且跳躍次數2次以內時執行
            {
                rb.AddForce(new Vector2(0, jump), ForceMode2D.Impulse); //向上跳躍
                ani.SetBool("跳躍", true);  //動畫控制-跳躍
                jumpCount += 1;
                Audio_Jump.Play();
            }
        }
    }

    /// <summary>
    /// 滑行按鍵，縮小碰撞器大小
    /// </summary>
    public void PlayerSlide()
    {
        if (PlayerHp > 0)
        {
            ani.SetBool("滑行", true);  //動畫控制-滑行
            CC2D.offset = new Vector2(-0.7f, -0.7f);
            CC2D.size = new Vector2(2.5f, 3.1f);
            Audio_Slide.Play();
        }
    }

    /// <summary>
    /// 取消跳躍和滑行，恢復碰撞器大小
    /// </summary>
    public void Resetani()
    {
        ani.SetBool("跳躍", false);
        ani.SetBool("滑行", false);
        CC2D.offset = new Vector2(-0.7f, -0.7f);
        CC2D.size = new Vector2(2.5f, 6.1f);
    }


}
