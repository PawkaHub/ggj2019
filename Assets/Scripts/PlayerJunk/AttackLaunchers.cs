using Game;
using UnityEngine;

public enum AttackKind
{
    CatProjectile,
    DogPuke,
    BirdPoop,
}

public interface IAttackLauncher
{
    void Update(bool attackButtonPressed, Vector3 position, Vector3 direction);
}

static public class AttackLauncherFactory
{
    static public IAttackLauncher Create(AttackKind kind, IPlayerAudioManager playerAudioManager, Player player)
    {
        switch (kind) 
        {
            case AttackKind.BirdPoop:
                return new BirdProjectileLauncher(playerAudioManager);
            case AttackKind.CatProjectile:
            default:
                return new CatProjectileLauncher(playerAudioManager, player);
        }

        return null;
    }
}

public class CatProjectileLauncher : IAttackLauncher
{
    bool buttonDown = false;
    IPlayerAudioManager AudioManager;
    GameObject CatProjectPrefab;
    CatProjectileController CatProjectileController;
    Player Player;

    public CatProjectileLauncher(IPlayerAudioManager playerAudioManager, Player player)
    {
        Player = player;
        AudioManager = playerAudioManager;
        CatProjectPrefab = Resources.Load<GameObject>("Prefabs/CatProjectile");
    }

    public void Update(bool attackButtonPressed, Vector3 position, Vector3 direction)
    {
        if (attackButtonPressed && !buttonDown)
        {
            var pukeObj = Object.Instantiate(CatProjectPrefab, position, Quaternion.identity);
            pukeObj.GetComponent<CatProjectileController>().Init(direction);
            pukeObj.GetComponent<BaseDamageApplier>().Creator = Player;
            AudioManager.PlayProjectileAudio();
            buttonDown = true;
        }

        if (!attackButtonPressed)
            buttonDown = false;
    }
}

public class BirdProjectileLauncher : IAttackLauncher
{
    bool buttonDown = false;
    IPlayerAudioManager AudioManager;
    GameObject CatProjectPrefab;
    CatProjectileController CatProjectileController;

    public BirdProjectileLauncher(IPlayerAudioManager playerAudioManager)
    {
        AudioManager = playerAudioManager;
        CatProjectPrefab = Resources.Load<GameObject>("Prefabs/BirdProjectile");
    }

    public void Update(bool attackButtonPressed, Vector3 position, Vector3 direction)
    {
        if (attackButtonPressed && !buttonDown)
        {
            var pukeObj = Object.Instantiate(CatProjectPrefab, position, Quaternion.identity);
            pukeObj.GetComponent<CatProjectileController>().Init(direction);
            AudioManager.PlayProjectileAudio();
            buttonDown = true;
        }

        if (!attackButtonPressed)
            buttonDown = false;
    }
}
