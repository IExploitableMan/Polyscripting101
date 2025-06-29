using BepInEx.Logging;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Newtonsoft.Json.Linq;
using Polytopia.Data;

namespace Polyscripting101;

public static class Main
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private static ManualLogSource logger;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public static void Load(ManualLogSource logger)
    {
        Main.logger = logger;
        logger.LogMessage("Here we go!");
        Harmony.CreateAndPatchAll(typeof(Main));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartScreen), nameof(StartScreen.Start))]
    private static void StartScreen_Start()
    {
        var popup = PopupManager.GetBasicPopup(new(
            "Header",
            "This is a basic popup to show that the mod is working.",
            new Il2CppReferenceArray<PopupBase.PopupButtonData>(new PopupBase.PopupButtonData[]
            {
                new("OK", callback: (UIButtonBase.ButtonAction)((_, _) => { logger.LogMessage("OK button clicked!"); })),
                new("Cancel", callback: (UIButtonBase.ButtonAction)((_, _) => { logger.LogMessage("Cancel button clicked!"); })),
            })
        ));
        popup.Show();
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(GameLogicData), nameof(GameLogicData.AddGameLogicPlaceholders))]
    private static void GameLogicData_AddGameLogicPlaceholders(JObject rootObject)
    {
        if (rootObject["spamData"] != null)
        {
            var spamData = rootObject["spamData"];
            int count = spamData["count"].ToObject<int>();
            string message = spamData["message"].ToObject<string>();
            for (int i = 0; i < count; i++)
            {
                logger.LogMessage($"Spam message {i + 1}: {message}");
            }
        }
        rootObject.Remove("spamData");
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UnitDataExtensions), nameof(UnitDataExtensions.GetMaxHealth))]
    private static bool UnitDataExtensions_GetDefence(ref int __result, UnitData unitState, GameState gameState)
    {
        if (unitState.HasAbility(EnumCache<UnitAbility.Type>.GetType("blahblah")))
        {
            __result = 1000;
            return true;
        }
        return false;
    }
}
