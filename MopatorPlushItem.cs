#if BEPINEX
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;



public class MopatorPlushItem : GrabbableObject
{
    public Animator PlushAnimator;
    public List<AudioClip> PlushAudioClips;


    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);
        PlushAnimator.SetTrigger("Activate");

        int chosenAudioClip = 1;
        if (NetworkManager.Singleton.IsServer)
        {
            int random = UnityEngine.Random.Range(1, PlushAudioClips.Count);
            chosenAudioClip = random;
        }

        GetComponent<AudioSource>().PlayOneShot(PlushAudioClips[chosenAudioClip]);
        
    }
}
#endif