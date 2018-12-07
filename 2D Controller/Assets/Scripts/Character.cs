using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private int m_health = 1;
    public int m_healthMax = 1;

    public float m_invicibleTime = 1.0f;
    private float m_invicibleTimer = 0.0f;

    public void Update()
    {
        m_invicibleTimer -= Time.deltaTime;

        //if (m_invicibleTimer >= 0.0f)//Used to change alpha
        //{
        //    Color playerColour = GetComponentInChildren<SkinnedMeshRenderer>().material.color;
        //    playerColour.a = 0.2f * Mathf.Sin(m_invicibleTimer * 20) + 0.2f;

        //    GetComponentInChildren<SkinnedMeshRenderer>().material.SetColor("_Color", playerColour);
        //}
         CharaterActions();
    }

    public bool IsDead()
    {
        return (m_health <= 0);
    }

    //Used to alter health
    public int GetHealth()
    {
        return m_health;
    }

    //Used to alter health
    public void SetHealth(int changeVal)
    {
        m_health = changeVal;
    }

    //Used to alter health
    public void ChangeHealth(int changeVal)
    {
        if(changeVal<0) //Loosing health
        {
            if (m_invicibleTimer <= 0.0f)//take damage from other sources
            {
                m_health += changeVal;
                m_invicibleTimer = m_invicibleTime;
            }
        }
        else // Gain health
        {
            m_health += changeVal;
        }

        //Keep health within bounds
        if (m_health > m_healthMax)
            m_health = m_healthMax;
    }

    public virtual void CharaterActions()
    {

    }
}
