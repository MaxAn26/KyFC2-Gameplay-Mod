using System;
using System.Collections.Generic;
using System.Linq;

using BaseMod.Core.Extensions;
using BaseMod.Core.Utils;

using BepInEx.Configuration;

using Il2CppInterop.Runtime;

using KyFC2.GameplayMod.Components;
using KyFC2.GameplayMod.Models;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace KyFC2.GameplayMod.Mods;
internal class SexMoveChoiceMod {
    #region Configuration
    internal static ConfigEntry<bool> Enabled;
    internal static ConfigEntry<bool> ChoosePositionsByGroup;
    internal static ConfigEntry<bool> IgnorePlayerPersonality;
    internal static ConfigEntry<bool> IgnorePlayerPositions;
    internal static ConfigEntry<bool> IgnoreForSignaturePosition;
    internal static ConfigEntry<bool> LockTypeForSignaturePosition;
    internal static ConfigEntry<bool> PersonalityToSexTags;
    #endregion

    #region States
    internal static bool IsModActive => Enabled.Value;
    internal static List<PersonalityToSexTag> PersonalityToSexTagModels { get; set; } = [];
    internal static List<SexMoveExtended> SexMoves { get; set; } = [];
    #endregion

    #region Storage
    internal static SexMoveExtended LastSexMove;
    #endregion

    internal static void Load(ConfigFile config) {
        try {
            Enabled = config.Bind(nameof(SexMoveChoiceMod), nameof(Enabled), false,
                new ConfigDescription("Activates the modification", new AcceptableValueList<bool>([true, false])));
            ChoosePositionsByGroup = config.Bind(nameof(SexMoveChoiceMod), nameof(ChoosePositionsByGroup), true,
                new ConfigDescription("Positions will be chosen by group (sex or foreplay) instead of by type (oral. handjob, etc.)", new AcceptableValueList<bool>([true, false])));
            IgnorePlayerPersonality = config.Bind(nameof(SexMoveChoiceMod), nameof(IgnorePlayerPersonality), false,
                new ConfigDescription("Ignore personality for player controlled character", new AcceptableValueList<bool>([true, false])));
            IgnorePlayerPositions = config.Bind(nameof(SexMoveChoiceMod), nameof(IgnorePlayerPositions), false,
                new ConfigDescription("Doesn't change positions for player controlled character", new AcceptableValueList<bool>([true, false])));
            IgnoreForSignaturePosition = config.Bind(nameof(SexMoveChoiceMod), nameof(IgnoreForSignaturePosition), false,
                new ConfigDescription("Doesn't change position for Signature positions", new AcceptableValueList<bool>([true, false])));
            LockTypeForSignaturePosition = config.Bind(nameof(SexMoveChoiceMod), nameof(LockTypeForSignaturePosition), false,
                new ConfigDescription("For Signature position will be used positions with same SexType as a default position", new AcceptableValueList<bool>([true, false])));
            PersonalityToSexTags = config.Bind(nameof(SexMoveChoiceMod), nameof(PersonalityToSexTags), true,
                new ConfigDescription("Select sex positions according with Caster personality", new AcceptableValueList<bool>([true, false])));

            if (PersonalityToSexTags.Value) {
                if (JsonUtils.TryDeserialize(Plugin.PluginResources, "PersonalityToSexTags.json", out List<PersonalityToSexTag> personalityToSexTags)) {
                    PersonalityToSexTagModels.Clear();
                    PersonalityToSexTagModels.AddRange(personalityToSexTags);
                }
            }
        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
        }
    }

    internal static void Prepare() {
        try {
            if (!Enabled.Value)
                return;

            bool fromFile = false;
            if (JsonUtils.TryDeserialize(Plugin.PluginResources, "SexMoves.json", out List<SexMoveExtended> extendedSexMoves))
                fromFile = true;

            if (!fromFile) {
                List<SexMoveExtended> poses = GetGameSexMoves();
                poses.Sort();
                if (JsonUtils.TrySerialize(Plugin.PluginResources, "SexMoves.json", poses)) {
                    extendedSexMoves = poses;
                    Plugin.Log.Info($"SexMoves.json was created in {Plugin.PluginResources}");
                } else {
                    Plugin.Log.Info($"SexMoves.json was not created");
                }
            }

            if (extendedSexMoves.Count <= 0)
                return;

            SexMoves.Clear();
            SexMoves.AddRange(extendedSexMoves);
        } catch (Exception ex) {
            Plugin.Log.Error(ex.Message);
        }
    }

    internal static PersonalityToSexTag GetPersonalityToSexTag( int personalityId) {
        try {
            if (!Enabled.Value || !PersonalityToSexTags.Value)
                return null;

            return PersonalityToSexTagModels.FirstOrDefault(p => p.PersonalityID == personalityId);
        } catch (Exception ex) {
            Plugin.Log.Error(ex);
            return null;
        }
    }

    internal static void SetSexID(SexSystem sexSystem) {
        try {
            if (!Enabled.Value || SceneManager.GetActiveScene().buildIndex != 1) {
                Plugin.Log.Info("Exit due execute condition");
                return;
            }

            if (sexSystem is null) {
                Plugin.Log.Info("Exit due SexSystem is NULL");
                return;
            }

            if (IgnoreForSignaturePosition.Value && ( sexSystem.kyfc.SexIsEnemySignature || sexSystem.kyfc.SexIsPlayerSignature)) {
                Plugin.Log.Info("Exit due Ignoring for Signature positions");
                return;
            }

            if (IgnorePlayerPositions.Value && sexSystem.Caster.TryGetComponentWithCast(out CharacterSex characterSex) && characterSex.IsPlayer) {
                Plugin.Log.Info("Exit due Ignoring for player character");
                return;
            }

            if (SexMoves.Count == 0) {
                Plugin.Log.Info("Exit due EMPTY SexPositions");
                return;
            }

            var move = GetSexMove(sexSystem);
            if (move is null) {
                Plugin.Log.Info("Exit due SexMove is null");
                return;
            }

            if (move.StraponNotAllowed) {
                Plugin.Log.Info("Unset strapon for characters");
                if (sexSystem.CasterActive && sexSystem.Caster.GetComponentWithCast<KyFCCharacterModComponent>()?.UseStrapOn == true) {
                    sexSystem.CasterActive = sexSystem.CasterMale;
                    sexSystem.CasterFuta = sexSystem.CasterMale;
                }

                if (sexSystem.TargetActive && sexSystem.Target.GetComponentWithCast<KyFCCharacterModComponent>()?.UseStrapOn == true) {
                    sexSystem.TargetActive = sexSystem.TargetMale;
                    sexSystem.TargetFuta = sexSystem.TargetMale;
                }

                if (sexSystem.IsThreesome && sexSystem.Assist?.GetComponentWithCast<KyFCCharacterModComponent>()?.UseStrapOn == true) {
                    sexSystem.AssistActive = sexSystem.CasterActive;
                    sexSystem.AssistFuta = sexSystem.CasterFuta || sexSystem.CasterMale;
                }
            }

            Plugin.Log.Info($"Set SexMove: ID: {SexSystem.SexID} -> {move.ID} ({sexSystem.SexType} -> {move.Type}) Name: '{move.Name}'");
            sexSystem.SexType = move.Type;
            SexSystem.SexID = move.ID;
        } catch (Exception ex) {
            Plugin.Log.Error(ex);
        }
    }

    private static List<SexMoveExtended> GetGameSexMoves() {
        Plugin.Log.Info("Get SexMoves from game...");
        List<SexMoveExtended> poses = [];

        GameObject mainScript = GameObject.Find("MainScript");
        if (mainScript is not null && mainScript.TryGetComponentWithCast(out GameSetup gameSetup) && gameSetup.charlist is not null && gameSetup.charlist.Sexmoves.Count > 0) {
            Plugin.Log.Info("Get SexMoves from GameSetup");
            foreach (var sexMove in gameSetup.charlist.Sexmoves) {
                if (gameSetup.charlist.AvailableSexMoves.Contains(sexMove.ID)) {
                    var move = SexMoveExtended.FromSexMove(sexMove);
                    if (move is not null)
                        poses.Add(move);
                }
            }

            Plugin.Log.Info($"Add {poses.Count} poses");
        } else {
            Plugin.Log.Info("Try find SexMoves in Resources");
            var sexMoveObj = Resources.FindObjectsOfTypeAll( Il2CppType.From( typeof(SexMove) ) );
            foreach (var moveObj in sexMoveObj) {
                var sexMove = moveObj.TryCast<SexMove>();
                if (sexMove is not null && sexMove.ID > 0 && sexMove.Type > 0) {
                    var move = SexMoveExtended.FromSexMove(sexMove);
                    if (move is not null)
                        poses.Add(move);
                }
            }
            Plugin.Log.Info($"Add {poses.Count}/{sexMoveObj.Count}");
        }

        poses.Sort();

        return poses;
    }

    public static List<SexMoveExtended> GetCharacterSexMoves(SexSystem sexSystem, int? sexType, bool isSignature, bool isCollared) {
        List<SexMoveExtended> sexMoves = [];
        CharacterGender caster = sexSystem.CasterActive ? CharacterGender.Male : CharacterGender.Female;
        CharacterGender target = sexSystem.TargetActive ? CharacterGender.Male : CharacterGender.Female;
        PersonalityToSexTag personalityToSex = sexSystem.Caster.TryGetComponentWithCast(out KyFCCharacterModComponent characterModComponent)
            ? characterModComponent.PersonalityToSexTag
            : null;

        List<int> sexTypes = [];
        if (sexType is not null)
            sexTypes.Add(sexType.Value);

        if (!isSignature || !LockTypeForSignaturePosition.Value) {
            if (ChoosePositionsByGroup.Value && sexType is not null) {
                if (sexType >= 1 && sexType <= 5)
                    sexTypes.AddUniqueRange([1, 2, 3, 4, 5]);
                else if (sexType >= 6 && sexType <= 8)
                    sexTypes.AddUniqueRange([6, 7, 8]);
            }
        }

        if (characterModComponent.CharacterSex.IsPlayer && IgnorePlayerPersonality.Value)
            personalityToSex = null;

        foreach (var sexMove in SexMoves) {
            if (sexMove.IsDisabled)
                continue;

            if (sexTypes.Count > 0 && !sexTypes.Contains(sexMove.Type))
                continue;

            if (PersonalityToSexTags.Value && (personalityToSex is null || !personalityToSex.SexMoveCheck(sexMove)))
                continue;

            if (sexMove.IsCommand && !isCollared)
                continue;

            if (!sexMove.IsUniversal && sexMove.CasterRole is not CharacterRole.Any) {
                if (sexSystem.CasterActive && sexMove.CasterRole is not CharacterRole.Active)
                    continue;
                else if (!sexSystem.CasterActive && sexMove.CasterRole is not CharacterRole.Passive)
                    continue;
                else
                    continue;
            }

            if (sexMove.CasterGender is not CharacterGender.Any && sexMove.CasterGender != caster)
                continue;

            if (sexMove.TargetGender is not CharacterGender.Any && sexMove.TargetGender != target)
                continue;

            sexMoves.Add(sexMove);
        }

        Plugin.Log.Message($"Result positions: {sexMoves.Count}");

        return sexMoves;
    }

    private static SexMoveExtended GetSexMove(SexSystem sexSystem) {
        int? sexType = SexMoves.FirstOrDefault( m => m.ID == SexSystem.SexID)?.Type;
        Plugin.Log.Message($"Origin Sex: ID: {SexSystem.SexID}, Type: {sexType}");
        bool isSignature = ( sexSystem.kyfc.SexIsEnemySignature || sexSystem.kyfc.SexIsPlayerSignature);
        bool isCollared = sexSystem.Target?.GetComponentWithCast<KyFCCharacterModComponent>()?.IsCollared() == true;

        List<SexMoveExtended> sexMoves = GetCharacterSexMoves( sexSystem, sexType, isSignature,isCollared );
        if (sexMoves.Count < 5 && sexType is not null && (!isSignature || !LockTypeForSignaturePosition.Value)) {
            sexMoves.AddRange(GetCharacterSexMoves(sexSystem, sexType.Value - 1, false, isCollared));
            sexMoves.AddRange(GetCharacterSexMoves(sexSystem, sexType.Value + 1, false, isCollared));
        }


        if (sexMoves.Count == 0)
            return null;

        sexMoves.Shuffle();
        var move = sexMoves.RandomItem();
        LastSexMove = move;
        return move;
    }
}