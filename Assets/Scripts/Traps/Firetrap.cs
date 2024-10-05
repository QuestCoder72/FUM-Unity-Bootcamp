using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firetrap : MonoBehaviour
{
    [SerializeField] private BoxCollider2D playerBox;
    [SerializeField] private float damage;

    [Header ("Firetrap Timers")]
    [SerializeField] private float activationDelay;
    [SerializeField] private float activeTime;
    private Animator anim;
    private SpriteRenderer spriteRend;

    private bool triggered; //when the trap gets triggered.
    private bool active; //when the trap is active and can hurt the player.

    void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();        
    }
    
    
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Player")
        {
            if(!triggered)
            {
                StartCoroutine(ActivateFiretrap());
            }
            if(active)
                other.GetComponent<Health>().TakeDamage(damage);
        }        
    }

    private IEnumerator ActivateFiretrap()
    {
        //turn the sprite red to notify the player and trigger the trap
        triggered = true;
        spriteRend.color = Color.red; //turn the sprite red to notify the player.

        yield return new WaitForSeconds(activationDelay);

        spriteRend.color = Color.white; //turn the sprite back to its initial color.
        active = true;
        anim.SetBool("activated", true);

        //Invoke the OnTriggerEnter2D method by toggling the player collision box on and off, in case the player hasnt gone out of collision trigger area
        playerBox.enabled = false;
        playerBox.enabled = true;
        
        //wait until X seconds, deactivate the trap and reset all variables and animator.
        yield return new WaitForSeconds(activeTime);
        active = false;
        triggered = false;
        anim.SetBool("activated", false);
    }
}
