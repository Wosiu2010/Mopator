#if BEPINEX
using System.Collections;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

public class MopatorAI : EnemyAI
{
    [Header("Mopator")]
    public GameObject Blob;
    public GameObject BlobDisposal;

    public AudioClip VentAudio;

    private bool IsAbleToSpawnBlob = true;

    enum State
    {
        SearchingForPlayer,
        ChasingPlayer
    }

    public override void Start()
    {
        base.Start();
        creatureSFX.PlayOneShot(VentAudio);
        creatureAnimator.SetTrigger("Move");
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
            int random = Random.Range(4, 10);

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

    
    public void SpawnBlobObject()
    {
        GameObject clone = Instantiate(Blob);
        clone.transform.position = BlobDisposal.transform.position;
        clone.GetComponent<NetworkObject>().Spawn();
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

    public override void OnCollideWithPlayer(UnityEngine.Collider other)
    {
        base.OnCollideWithPlayer(other);
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