#if BEPINEX
using System.Collections;
using System.Collections.Generic;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

public class MopatorBlob : NetworkBehaviour
{
    public Mesh emptySuitMesh;
    public Animator BlobAnimator;
    public AudioSource BlobAudio;

    public List<AudioClip> BubblePops;

    [Header("BlobSpawn")]
    public AudioClip BlobSpawn;

    private bool IsAbleToDealDamage = true;
    private bool IsAbleToSweepBlob = true;

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
        PlayBlobAnimationClientRpc(1);
        for (int i = 0; i < 30; i++)
        {
            int randomPop = UnityEngine.Random.Range(1, BubblePops.Count);

            PlayBubblePopClientRpc(randomPop);
            
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
        BlobAudio.PlayOneShot(BubblePops[SoundID]);
    }

    [ClientRpc]
    public void PlayBlobAnimationClientRpc(int AnimID)
    {
        if (AnimID == 1)
        {
            BlobAnimator.SetTrigger("Disappear");
            BlobAudio.PlayOneShot(BlobSpawn);
        }
        if (AnimID == 2)
        {
            BlobAnimator.SetTrigger("Sweep");
        }
    }
    
    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerControllerB>() && IsAbleToDealDamage == true && !other.gameObject.GetComponent<PlayerControllerB>().isPlayerDead)
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


        else if (other.gameObject.GetComponent<EnemyAICollisionDetect>() && NetworkManager.Singleton.IsServer)
        {
            if (other.gameObject.GetComponent<EnemyAICollisionDetect>().transform.parent.GetComponent<ButlerEnemyAI>())
            {
                if (other.gameObject.GetComponent<EnemyAICollisionDetect>().transform.parent.GetComponent<ButlerEnemyAI>().creatureAnimator.GetBool("Sweeping") && IsAbleToSweepBlob)
                {
                    StartCoroutine(BlobSweeping());
                    IsAbleToSweepBlob = false;
                }
                else
                {
                    return;
                }
            }
        }
    }

    public IEnumerator BlobSweeping()
    {
        PlayBlobAnimationClientRpc(2);
        yield return new WaitForSeconds(1);
        DestroyBlobObjectClientRpc();
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
            Debug.Log("MopatorBlob: Player body was not spawned or found within 2 seconds.");
            yield break;
        }
        playerScript.deadBody.attachedLimb = playerScript.deadBody.bodyParts[6];
        playerScript.deadBody.attachedTo = transform;
        playerScript.deadBody.matchPositionExactly = false;
        yield return new WaitForSeconds(2f);
        if (playerScript.deadBody == null)
        {
            Debug.Log("MopatorBlob: Player body was not spawned or found within 2 seconds.");
            yield break;
        }
        playerScript.deadBody.attachedTo = null;
        playerScript.deadBody.ChangeMesh(emptySuitMesh);
    }
}
#endif