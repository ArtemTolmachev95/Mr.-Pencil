using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class CoinsScripts : MonoBehaviour
{
    [SerializeField] public static int coins;
    [SerializeField] Text coinsText;

    [SerializeField] public static int WeaponBomb;
    [SerializeField] Text WeaponText;



    [SerializeField] Text LiveText;
    [SerializeField] Text DamageText;
    [SerializeField] Text BombText;

    public GameObject selectButtonLive;
    public GameObject selectButtonDamage;
    public GameObject selectButtonBomb;

    public GameObject obj;

    public void Start()
    {
        


        if (SceneManager.GetActiveScene().name != "Main")
        {

            if (PlayerPrefs.HasKey("coins"))
            {
                coins = PlayerPrefs.GetInt("coins");
                coinsText.text = coins.ToString();

            }

            if (PlayerPrefs.HasKey("WeaponBomb"))
            {
                WeaponBomb = PlayerPrefs.GetInt("WeaponBomb");
                WeaponText.text = WeaponBomb.ToString();

            }

            if (PlayerPrefs.HasKey("life"))
            {
                obj.GetComponent<CharacterController2D>().life = PlayerPrefs.GetFloat("life");


            }
        }
        else
        {
            coins = 0;
            WeaponBomb = 0;
        }
    }


    public void Update()
    {
        if (obj.GetComponent<CharacterController2D>().life <= 0)
        {
            SceneManager.LoadScene("Main");
            coins = coins * 0;
            WeaponBomb = WeaponBomb * 0;
        }
    }

    public void OnClickLive()
    {
        if (coins >= 5)
        {
            coins = coins - 5;
            coinsText.text = coins.ToString();
            Debug.Log(" минус 5 коинов");
            obj.GetComponent<CharacterController2D>().life++;

            
        }
        else
        {
            Debug.Log("Недостаточно денег");
        }
    }

    public void OnClickDamage()
    {
        if (coins >= 3)
        {
            coins = coins - 3;
            coinsText.text = coins.ToString();
            WeaponBomb++;
            WeaponText.text = WeaponBomb.ToString();
            Debug.Log(" минус 3 коинов");
            Debug.Log(" плюс 1 снаряд");
        }
        else
        {
            Debug.Log("Недостаточно денег");
        }
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Coin")
        {
            coins++;
            coinsText.text = coins.ToString();
            PlayerPrefs.SetInt("coins", coins);
            Destroy(other.gameObject);
            Debug.Log("+1 очко");
        }

        if (other.gameObject.tag == "Coin * 3")
        {
            coins = coins * 3;
            coinsText.text = coins.ToString();
            Destroy(other.gameObject);
            Debug.Log("+1 очко");
        }

        if (other.gameObject.tag == "Weapon")
        {
            WeaponBomb++;
            WeaponText.text = WeaponBomb.ToString();
            PlayerPrefs.SetInt("WeaponBomb", WeaponBomb);
            Destroy(other.gameObject);
            Debug.Log("+1 бомба");
        }


    }
}
