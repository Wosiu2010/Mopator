#if BEPINEX
using System.Collections;
using System.Collections.Generic;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

public class MopatorAI : EnemyAI
{
    [Header("Mopator")]
    public GameObject Blob;
    public GameObject BlobDisposal;

    public AudioClip VentAudio;
    public List<AudioClip> SniffClips;

    private bool IsAbleToSpawnBlob = true;

    enum State
    {
        SearchingForPlayer,
        ChasingPlayer
    }

    public override void Start()
    {
        base.Start();
        if (!NetworkManager.Singleton.IsServer) return;

        PlayAnimationForEveryPlayerClientRpc(2);
    }

    public override void Update()
    {
        base.Update();
        if (isEnemyDead)
        {
            return;
        }
    }

    public void LateUpdate()
    {
        if (targetPlayer != null)
        {

            Vector3 direction = targetPlayer.gameplayCamera.transform.position - transform.position;
            direction.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 5 * Time.deltaTime);

        }

        if (IsAbleToSpawnBlob == true && timeSinceSpawn >= 1f && NetworkManager.Singleton.IsServer)
        {
            int random = UnityEngine.Random.Range(4, 10);

            SpawnBlobObject();
            StartCoroutine(DisableAndEnableBlobSpawningAfterDelay(random));
        }

    }

    public override void DoAIInterval()
    {
        base.DoAIInterval();
        if (isEnemyDead && StartOfRound.Instance.allPlayersDead)
        {
            return;
        }
        switch (currentBehaviourStateIndex)
        {
            case (int)State.SearchingForPlayer:
                agent.speed = 2f;
                if(FoundClosestPlayerInRange(25f, 3f))
                {
                    StopSearch(currentSearch);
                    SwitchToBehaviourClientRpc((int)State.ChasingPlayer);
                }
                break;

            case (int)State.ChasingPlayer:
                agent.speed = 0.8f;
                if (!TargetClosestPlayerInAnyCase() || (Vector3.Distance(transform.position, targetPlayer.transform.position) > 20 && !CheckLineOfSightForPosition(targetPlayer.transform.position)))
                {
                    StartSearch(transform.position);
                    SwitchToBehaviourClientRpc((int)State.SearchingForPlayer);
                    return;
                }
                SetDestinationToPosition(targetPlayer.transform.position);
                break;
        }
    }

    [ClientRpc]
    public void PlayAnimationForEveryPlayerClientRpc(int AnimID)
    {
        if (AnimID == 1)
        {
            creatureAnimator.SetTrigger("Sniff");
        }
        if (AnimID == 2)
        {
            creatureAnimator.Play("Move", 0, 0f);
            creatureSFX.PlayOneShot(VentAudio);
        }
    }

    [ClientRpc]
    public void PlaySniffOnEveryPlayerClientRpc(int SoundID)
    {
        creatureSFX.PlayOneShot(SniffClips[SoundID]);
    }
    
    public void SpawnBlobObject()
    {
        Ray ray = new Ray(BlobDisposal.transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2f))
        {
            if (hit.collider.gameObject.GetComponent<MopatorBlob>())
                return;

            int randomSniff = UnityEngine.Random.Range(1, SniffClips.Count);

            PlaySniffOnEveryPlayerClientRpc(randomSniff);
            PlayAnimationForEveryPlayerClientRpc(1);


            GameObject clone = Instantiate(Blob, hit.point, Quaternion.identity);
            clone.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            clone.GetComponent<NetworkObject>().Spawn();
        }


    }

    public IEnumerator DisableAndEnableBlobSpawningAfterDelay(int Random)
    {
        IsAbleToSpawnBlob = false;
        yield return new WaitForSeconds(Random);
        IsAbleToSpawnBlob = true;
    }

    public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
    {
        base.HitEnemy(force, playerWhoHit, playHitSFX, hitID);
    }

    bool FoundClosestPlayerInRange(float range, float senseRange)
    {
        TargetClosestPlayer(bufferDistance: 1.5f, requireLineOfSight: true);
        if (targetPlayer == null)
        {
            TargetClosestPlayer(bufferDistance: 1.5f, requireLineOfSight: false);
            range = senseRange;
        }
        return targetPlayer != null && Vector3.Distance(transform.position, targetPlayer.transform.position) < range;
    }

    bool TargetClosestPlayerInAnyCase()
    {
        mostOptimalDistance = 2000f;
        targetPlayer = null;
        for (int i = 0; i < StartOfRound.Instance.connectedPlayersAmount + 1; i++)
        {
            tempDist = Vector3.Distance(transform.position, StartOfRound.Instance.allPlayerScripts[i].transform.position);
            if (tempDist < mostOptimalDistance)
            {
                mostOptimalDistance = tempDist;
                targetPlayer = StartOfRound.Instance.allPlayerScripts[i];
            }
        }
        if (targetPlayer == null) return false;
        return true;
    }
}
#endif