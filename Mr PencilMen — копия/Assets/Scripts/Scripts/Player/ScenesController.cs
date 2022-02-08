using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScenesController : MonoBehaviour
{

    [SerializeField] private GameObject ShopPanel;
   

    private void Start()
    {
        
        ShopPanel.SetActive(false);

    }

    private void OnTriggerEnter2D(Collider2D other)
    {


        if (other.gameObject.tag == "Shop")
        {

            ShopPanel.SetActive(true);
           

        }
        else
        {

            ShopPanel.SetActive(false);
           
        }
    }
    private void Paneldestroy()
    {
        ShopPanel.SetActive(false);
    }

}