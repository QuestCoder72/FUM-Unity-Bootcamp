using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header ("Health")]
    [SerializeField] private float startingHealth;
    public float currentHealth {get; private set;}
    private bool dead;
    private Animator anim;

    [Header ("iFrames")]
    [SerializeField] private float iFramesDuration;
    [SerializeField] private int numberOfFlashes;
    private SpriteRenderer spriteRend;

    [Header ("Components")]
    [SerializeField] private Behaviour[] components;
    
    void Awake()
    {
        currentHealth = startingHealth;  
        anim = GetComponent<Animator>();  
        spriteRend = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(float _damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if(currentHealth > 0)
        {
            anim.SetTrigger("hurt");
            StartCoroutine(Invulnerability());
        }
        else
        {
            if(!dead)
            {
                anim.SetTrigger("dead");

                //Player
                anim.SetBool("died", currentHealth == 0);

                // if( GetComponent<PlayerMovement>() != null)
                //     GetComponent<PlayerMovement>().enabled = false;

                // if(GetComponent<ThrowShuriken>() != null)
                //     GetComponent<ThrowShuriken>().enabled = false;

                if(GetComponent<Rigidbody2D>() != null)
                    GetComponent<Rigidbody2D>().velocity = Vector3.zero;
                
                // //Enemy
                // if(GetComponentInParent<EnemyPatrol>() != null)
                //     GetComponentInParent<EnemyPatrol>().enabled = false;

                // if(GetComponent<MeleeEnemy>() != null)
                //     GetComponent<MeleeEnemy>().enabled = false;

                //Deactivate all attached component classes.
                foreach(Behaviour component in components)
                    component.enabled = false;
                dead = true;
            } 
        }
    }

    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0, startingHealth);
    }

    private IEnumerator Invulnerability()
    {
        Physics2D.IgnoreLayerCollision(10, 11, true);
        for(int i = 0; i < numberOfFlashes; i++)                     
        {
            spriteRend.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
            spriteRend.color = Color.white;
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
        }
        Physics2D.IgnoreLayerCollision(10, 11, false);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
