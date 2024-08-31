using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Theatrhythm;
using UnityEngine;

namespace FFCoreFixes {
    internal class ExternalTranslationHack {
        internal static ManualLogSource Log;

        internal static void CheckApply() {

            // seriously, don't create your own bullshit format for something like this

            if (Directory.Exists("translationdata")) {
                ApplyTranslationHackToList(
                    AbilityTable.entityAbilityTable.dictionary,
                    "AbilityTable.json",
                    "abilityID",
                    new string[] { "name", "resultText", "supportResultText" },
                    new string[] { "name_EN", "resultText_EN", "supportResultText_EN" }
                );
                ApplyTranslationHackToList(
                    AirshipTable.entityAirshipTable.dictionary,
                    "AirshipTable.json",
                    "airshipId",
                    new string[] { "airshipName" },
                    new string[] { "airshipName_EN" }
                );
                ApplyTranslationHackToList(
                    CharacterCommentTable.entityCharacterCommentTable.dictionary,
                    "CharacterCommentTable.json",
                    "ID",
                    new string[] { "text" },
                    new string[] { "text_EN" }
                );
                ApplyTranslationHackToList(
                    CharacterCommentTable.entityCharacterCommentTable.dictionary,
                    "CharacterCommentTable.json",
                    "ID",
                    new string[] { "text" },
                    new string[] { "text_EN" }
                );
                ApplyTranslationHackToList(
                    CharacterTable.entityCharacterTable.dictionary,
                    "CharacterTable.json",
                    "characterID",
                    new string[] { "characterName" },
                    new string[] { "characterEnglishName" }
                );
                ApplyTranslationHackToList(
                    CollectionCardTable.CharacterTable.Dictionary,
                    "CollectionCardTable.json",
                    "ID",
                    new string[] { "name" },
                    new string[] { "name_EN" }
                );
                ApplyTranslationHackToList(
                    ItemTable.EntityItemTable.dictionary,
                    "ItemTable.json",
                    "itemID",
                    new string[] { "itemName" },
                    new string[] { "itemName_EN" }
                );
                ApplyTranslationHackToList(
                    LevelUpCommentTable.EntityLevelUpCommentTable.dictionary,
                    "LevelUpCommentTable.json",
                    "ID",
                    new string[] { "text" },
                    new string[] { "text_EN" }
                );
                ApplyTranslationHackToList(
                    MedleyTable.EntityMedleyTable.dictionary,
                    "MedleyTable.json",
                    "medleyID",
                    new string[] { "title", "name_jp" },
                    new string[] { "title_EN", "name_EN" }
                );
                ApplyTranslationHackToList(
                    MonsterTable.EntityMonsterTable.dictionary,
                    "MonsterTable.json",
                    "monsterID",
                    new string[] { "name_JP" },
                    new string[] { "name_EN" }
                );
                ApplyTranslationHackToList(
                    MusicTable.EntityMusicTable.dictionary,
                    "MusicTable.json",
                    "musicID",
                    new string[] { "name_JP" },
                    new string[] { "name_EN" }
                );
                ApplyTranslationHackToList(
                    PlayerTitleTable.table.dictionary,
                    "PlayerTitleTable.json",
                    "ID",
                    new string[] { "name", "text", "PlateListText" },
                    new string[] { "name_EN", "text_EN", "PlateListText_EN" }
                );
                ApplyTranslationHackToList(
                    ProficaDesignTable.table.dictionary,
                    "ProficaDesignTable.json",
                    "ID",
                    new string[] { "name" },
                    new string[] { "name_EN" }
                );
                ApplyTranslationHackToList(
                    ProficaIllustTable.table.dictionary,
                    "ProficaIllustTable.json",
                    "ID",
                    new string[] { "name" },
                    new string[] { "name_EN" }
                );
                ApplyTranslationHackToList(
                    ShopTable.entityShopTable.dictionary,
                    "ShopTable.json",
                    "itemPackID",
                    new string[] { "saleName" },
                    new string[] { "saleName_EN" }
                );
                ApplyTranslationHackToList(
                    SummonTable.entitySummonTable.dictionary,
                    "SummonTable.json",
                    "summonID",
                    new string[] { "name_JP", "ability", "summonResultText" },
                    new string[] { "name_EN", "ability_EN", "summonResultText_EN" }
                );
                ApplyTranslationHackToList(
                    TriggerSETable.entityTriggerSETable.dictionary,
                    "TriggerSETable.json",
                    "ID",
                    new string[] { "name" },
                    new string[] { "name_EN" }
                );
            } else {
                Log.LogDebug("ETH not found");
            }
        }

        private static void ApplyTranslationHackToList<T>(Dictionary<string, T> data, string hacked_file, string json_id_field, string[] object_target_fields, string[] json_translation_fields) {
            Log.LogDebug("ApplyTranslationHackToList:" + hacked_file);
            JObject json = JObject.Parse(File.ReadAllText(Path.Combine("translationdata", hacked_file)));

            foreach (JToken e in ((JArray)json["param"])) {

                for (int i = 0; i < object_target_fields.Length; i++) {

                    if (e[json_translation_fields[i]] == null){
                        continue;
                    }

                    // holy fuck
                    string item_id = (string)((JValue)e[json_id_field]).Value;
                    string translation = (string)((JValue)e[json_translation_fields[i]]).Value;

                    if (!data.ContainsKey(item_id)) {
                        Log.LogWarning("Translation Hack:" + item_id + " not found for " + hacked_file);
                        continue;
                    }
                    object target = data[item_id];

                    AccessTools.DeclaredField(typeof(T), object_target_fields[i]).SetValue(target, translation);

                }
            }

            Log.LogDebug("Applied translation hack: " + hacked_file);

        }
    }
}