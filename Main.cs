using UnityModManagerNet;
using System;
using System.Reflection;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;

namespace RSDR
{
    internal class Main
    {
        internal static UnityModManagerNet.UnityModManager.ModEntry.ModLogger logger;
        internal static Harmony.HarmonyInstance harmony;
        internal static LibraryScriptableObject library;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                logger = modEntry.Logger;
                harmony = Harmony.HarmonyInstance.Create(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                logger?.LogException(ex);
                throw ex;
            }
            return true;
        }

        [Harmony.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
        [Harmony.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary", new Type[0])]
        static class LibraryScriptableObject_LoadDictionary_Patch
        {
            [Harmony.HarmonyBefore(new string[] { "KingmakerAI" })]

            static void Postfix(LibraryScriptableObject __instance)
            {
                var self = __instance;
                if (Main.library != null) return;
                Main.library = self;
                try
                {
                    var AllBlueprints = Main.library.GetAllBlueprints();
                    var SwarmDiminutiveFeature = AllBlueprints.First(x => x.name.Equals("SwarmDiminutiveFeature")) as BlueprintFeature;
                    if (SwarmDiminutiveFeature)
                    {
                        Main.logger.Log("Removing SwarmDamageResistance from SwarmDiminutiveFeature...");
                        var componentsWithoutDamageResistance = SwarmDiminutiveFeature.ComponentsArray.Where(delegate (BlueprintComponent c) {
                            return c.GetType() != typeof(Kingmaker.UnitLogic.FactLogic.SwarmDamageResistance);
                        }).ToArray();
                        SwarmDiminutiveFeature.ComponentsArray = componentsWithoutDamageResistance;
                    } else
                    {
                        Main.logger.Error("SwarmDiminutiveFeature blueprint not found!");
                    }
                }
                catch (Exception ex)
                {
                    Main.logger.LogException(ex);
                }
            }
        }
    }
}