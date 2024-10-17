using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombPower : MonoBehaviour
{
    public GameObject shoutWall;
    public float yLaunchVelocity;
    public float zLaunchVelocity;
    //private AudioSource _fusrodah;

    [SerializeField] private float fusrodah_timer_cooldown = 3f;
    private bool fusrodah_timer_locked_out = false;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Fus Ro Dah ability
        if (Input.GetKeyDown("f"))
        {
            FusRoDahShout();
        }
    }
    public void FusRoDahShout()
    {
        Debug.Log("Key activated, preparing to fire");
        //Check if cooldown is up
        if (fusrodah_timer_locked_out == false)
        {
            //Put FusRoDah on cooldown 
            fusrodah_timer_locked_out = true;
            StartCoroutine(FusRoDahCooldown());

            //Begin FusRoDah shout
            Debug.Log("yell");

            //Create object at current position
            GameObject fusrodah = Instantiate(shoutWall, transform.position, transform.rotation);
            fusrodah.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, yLaunchVelocity, zLaunchVelocity));

            //Play sound
            /*
            _fusrodah = GetComponent<AudioSource>();
            if (_fusrodah != null)
            {
                _fusrodah.Play();
            }
            */

            //Destroy after time
            Destroy(fusrodah, 10f);
        }
    }
    IEnumerator FusRoDahCooldown()
    {
        yield return new WaitForSeconds(fusrodah_timer_cooldown);
        fusrodah_timer_locked_out = false;
        Debug.Log("Ready to fire");
    }
}