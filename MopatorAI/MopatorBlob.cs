#if BEPINEX
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

public class MopatorBlob : NetworkBehaviour
{
    public Mesh emptySuitMesh;
    public Animator BlobAnimator;
    public AudioSource BlobAudio;

    [Header("BubblePops")]
    public AudioClip BubblePop1;
    public AudioClip BubblePop2;
    public AudioClip BubblePop3;
    public AudioClip BubblePop4;
    public AudioClip BubblePop5;
    public AudioClip BubblePop6;

    [Header("BlobSpawn")]
    public AudioClip BlobSpawn;

    private bool IsAbleToDealDamage = true;

    public void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            StartCoroutine(BlobDisappear());
            Debug.Log("StartedMopatorBlob");
        }
    }

    public void Update()
    {
        if (StartOfRound.Instance.inShipPhase == true)
        {
            Debug.Log("Removing BlobObject");
            DestroyBlobObjectClientRpc();
        }
    }

    public IEnumerator BlobDisappear()
    {
        PlayBlobAnimatorClientRpc();
        for (int i = 0; i < 30; i++)
        {
            int random = UnityEngine.Random.Range(1, 6);

            PlayBubblePopClientRpc(random);
            
            yield return new WaitForSeconds(1);
        }
        DestroyBlobObjectClientRpc();
        
    }

    [ClientRpc]
    public void DestroyBlobObjectClientRpc()
    {
        Debug.Log("Destroying blob");
        
        if (NetworkManager.Singleton.IsServer)
        {
            gameObject.GetComponent<NetworkObject>().Despawn();
        }
        
    }

    [ClientRpc]
    public void PlayBubblePopClientRpc(int SoundID)
    {
        if (SoundID == 1)
        {
            BlobAudio.PlayOneShot(BubblePop1);
        }
        if (SoundID == 2)
        {
            BlobAudio.PlayOneShot(BubblePop2);
        }
        if (SoundID == 3)
        {
            BlobAudio.PlayOneShot(BubblePop3);
        }
        if (SoundID == 4)
        {
            BlobAudio.PlayOneShot(BubblePop4);
        }
        if (SoundID == 5)
        {
            BlobAudio.PlayOneShot(BubblePop5);
        }
        if (SoundID == 6)
        {
            BlobAudio.PlayOneShot(BubblePop6);
        }
    }

    [ClientRpc]
    public void PlayBlobAnimatorClientRpc()
    {
        BlobAnimator.SetTrigger("Disappear");
        BlobAudio.PlayOneShot(BlobSpawn);
    }
    
    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerControllerB>() && IsAbleToDealDamage == true)
        {
            Debug.Log("Damaging Player");
            Debug.Log($"IsAbleToDamagePlayer: {IsAbleToDealDamage}");
            other.gameObject.GetComponent<PlayerControllerB>().DamagePlayer(damageNumber: 20, causeOfDeath: CauseOfDeath.Suffocation);
            StartCoroutine(DisableAndEnableDamagingPlayer());
            if (other.gameObject.GetComponent<PlayerControllerB>().isPlayerDead)
            {
                SlimeKillPlayerEffectServerRpc((int)other.gameObject.GetComponent<PlayerControllerB>().playerClientId);
            }
            Debug.Log($"IsAbleToDamagePlayer: {IsAbleToDealDamage}");
        }
    }

    public IEnumerator DisableAndEnableDamagingPlayer()
    {
        IsAbleToDealDamage = false;
        yield return new WaitForSeconds(0.5f);
        IsAbleToDealDamage = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SlimeKillPlayerEffectServerRpc(int playerKilled)
    {
        SlimeKillPlayerEffectClientRpc(playerKilled);
    }

    [ClientRpc]
    public void SlimeKillPlayerEffectClientRpc(int playerKilled)
    {
        {
            StartCoroutine(eatPlayerBody(playerKilled));
        }
    }

    private IEnumerator eatPlayerBody(int playerKilled)
    {
        yield return null;
        PlayerControllerB playerScript = StartOfRound.Instance.allPlayerScripts[playerKilled];
        float startTime = Time.realtimeSinceStartup;
        yield return new WaitUntil(() => playerScript.deadBody != null || Time.realtimeSinceStartup - startTime > 2f);
        if (playerScript.deadBody == null)
        {
            Debug.Log("Blob: Player body was not spawned or found within 2 seconds.");
            yield break;
        }
        playerScript.deadBody.attachedLimb = playerScript.deadBody.bodyParts[6];
        playerScript.deadBody.attachedTo = transform;
        playerScript.deadBody.matchPositionExactly = false;
        yield return new WaitForSeconds(2f);
        playerScript.deadBody.attachedTo = null;
        playerScript.deadBody.ChangeMesh(emptySuitMesh);
    }
}
#endif