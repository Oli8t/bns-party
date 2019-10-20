using BS;
using Harmony;
using IngameDebugConsole;
using System.Reflection;
using UnityEngine;

namespace CharacterParty
{
  public class CharacterParty : LevelModule
  {
    private HarmonyInstance harmony = null;

    public override void OnLevelLoaded(LevelDefinition levelDefinition)
    {
      DebugLogConsole.AddCommandStatic("spawnparty", "Spawn a party", "SpawnParty", typeof(CharacterParty));
      GameManager.onPossessionEvent += GameManager_onPossessionEvent;

      try
      {
        harmony = HarmonyInstance.Create("CharacterParty");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        Debug.Log("CharacterParty successfully loaded!");
      }
      catch (System.Exception e)
      {
        Debug.LogException(e);
      }
    }

    private void GameManager_onPossessionEvent(Body oldBody, Body newBody)
    {
      SpawnParty();
    }

    public static void SpawnParty()
    {
      if (Creature.player != null)
      {
        int offset = 1;
        foreach (var characterData in DataManager.GetCharacters())
        {
          if (characterData.ID != GameManager.playerData.ID)
          {
            // Spawn the party member offset from the player
            CreatureData creatureData = Catalog.current.GetData<CreatureData>("PartyMember");
            Creature partyCreature = creatureData.Instantiate(Creature.player.transform);
            partyCreature.transform.localPosition += new Vector3(0, 0, offset);
            partyCreature.transform.SetParent(null);
            offset += 1;

            partyCreature.container.containerID = null;
            partyCreature.loadUmaPreset = false;
            partyCreature.container.content = characterData.inventory;

            if (partyCreature.umaCharacter)
            {
              partyCreature.umaCharacter.LoadUmaPreset(characterData.umaPreset, null);
            }
          }
        }
      }
    }

    [HarmonyPatch(typeof(LevelModuleWave))]
    [HarmonyPatch("UpdateSpawner")]
    internal static class IgnorePartyPatch
    {
      [HarmonyPostfix]
      private static void Postfix(LevelModuleWave __instance)
      {
        foreach (Creature creature in Creature.list)
        {
          Debug.Log(creature.creatureID);
          if (creature.creatureID == "PartyMember")
          {
            Debug.Log("removed");
            __instance.aliveCount -= 1;
          }
        }
      }
    }

    [HarmonyPatch(typeof(LevelModuleWave))]
    [HarmonyPatch("StartGroupCoroutine")]
    internal static class IgnorePartyPatch2
    {
      [HarmonyPrefix]
      private static bool Prefix(LevelModuleWave __instance)
      {
        Debug.Log("removing party from alivecount");
        foreach (Creature creature in Creature.list)
        {
          Debug.Log(creature.creatureID);
          if (creature.creatureID == "PartyMember")
          {
            Debug.Log("removed");
            __instance.aliveCount -= 1;
          }
        }

        return true;
      }
    }
  }
}
