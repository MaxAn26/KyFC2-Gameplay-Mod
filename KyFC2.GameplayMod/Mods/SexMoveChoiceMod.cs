using BaseMod.Core;
using BaseMod.Core.Extensions;
using BaseMod.Core.Utils;
using Il2Cpp;
using Il2CppInterop.Runtime;

using KyFC2.GameplayMod.Components;
using KyFC2.GameplayMod.Models;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BaseMod.Core.ModConfig;

namespace KyFC2.GameplayMod.Mods;
internal class SexMoveChoiceMod
{
    #region Configuration
    internal static MelonPreferences_Entry<bool> Enabled;
    internal static MelonPreferences_Entry<bool> ChoosePositionsByGroup;
    internal static MelonPreferences_Entry<bool> IgnorePlayerPersonality;
    internal static MelonPreferences_Entry<bool> IgnorePlayerPositions;
    internal static MelonPreferences_Entry<bool> IgnoreForSignaturePosition;
    internal static MelonPreferences_Entry<bool> LockTypeForSignaturePosition;
    internal static MelonPreferences_Entry<bool> PersonalityToSexTags;
    #endregion

    #region States
    internal static bool IsModActive => Enabled.Value;
    internal static List<PersonalityToSexTag> PersonalityToSexTagModels { get; set; } = [];
    internal static List<SexMoveExtended> SexMoves { get; set; } = [];
    #endregion

    #region Storage
    internal static SexMoveExtended LastSexMove;
    #endregion

    internal static void Load(ModConfig config)
    {
        try
        {
            Enabled = config.Entry(nameof(SexMoveChoiceMod), nameof(Enabled), false,
                "Activates the modification", new AcceptableValueList<bool>([true, false]));
            ChoosePositionsByGroup = config.Entry(nameof(SexMoveChoiceMod), nameof(ChoosePositionsByGroup), true,
                "Positions will be chosen by group (sex or foreplay) instead of by type (oral. handjob, etc.)", new AcceptableValueList<bool>([true, false]));
            IgnorePlayerPersonality = config.Entry(nameof(SexMoveChoiceMod), nameof(IgnorePlayerPersonality), false,
                "Ignore personality for player controlled character", new AcceptableValueList<bool>([true, false]));
            IgnorePlayerPositions = config.Entry(nameof(SexMoveChoiceMod), nameof(IgnorePlayerPositions), false,
                "Doesn't change positions for player controlled character", new AcceptableValueList<bool>([true, false]));
            IgnoreForSignaturePosition = config.Entry(nameof(SexMoveChoiceMod), nameof(IgnoreForSignaturePosition), false,
                "Doesn't change position for Signature positions", new AcceptableValueList<bool>([true, false]));
            LockTypeForSignaturePosition = config.Entry(nameof(SexMoveChoiceMod), nameof(LockTypeForSignaturePosition), false,
                "For Signature position will be used positions with same SexType as a default position", new AcceptableValueList<bool>([true, false]));
            PersonalityToSexTags = config.Entry(nameof(SexMoveChoiceMod), nameof(PersonalityToSexTags), true,
                "Select sex positions according with Caster personality", new AcceptableValueList<bool>([true, false]));

            if (PersonalityToSexTags.Value)
            {
                if (JsonUtils.TryDeserialize(GameplayMod.PluginResources, "PersonalityToSexTags.json", out List<PersonalityToSexTag> personalityToSexTags))
                {
                    PersonalityToSexTagModels.Clear();
                    PersonalityToSexTagModels.AddRange(personalityToSexTags);
                }
            }
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
        }
    }

    internal static void Prepare()
    {
        try
        {
            if (!Enabled.Value)
            {
                return;
            }

            bool fromFile = false;
            if (JsonUtils.TryDeserialize(GameplayMod.PluginResources, "SexMoves.json", out List<SexMoveExtended> extendedSexMoves))
            {
                fromFile = true;
            }

            if (!fromFile)
            {
                List<SexMoveExtended> poses = GetGameSexMoves();
                poses.Sort();
                if (JsonUtils.TrySerialize(GameplayMod.PluginResources, "SexMoves.json", poses))
                {
                    extendedSexMoves = poses;
                    GameplayMod.Log.Msg($"SexMoves.json was created in {GameplayMod.PluginResources}");
                }
                else
                {
                    GameplayMod.Log.Msg($"SexMoves.json was not created");
                }
            }

            if (extendedSexMoves.Count <= 0)
            {
                return;
            }

            SexMoves.Clear();
            SexMoves.AddRange(extendedSexMoves);
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex.Message);
        }
    }

    internal static PersonalityToSexTag GetPersonalityToSexTag(int personalityId)
    {
        try
        {
            if (!Enabled.Value || !PersonalityToSexTags.Value)
            {
                return null;
            }

            return PersonalityToSexTagModels.FirstOrDefault(p => p.PersonalityID == personalityId);
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex);
            return null;
        }
    }

    internal static void SetSexID(SexSystem sexSystem)
    {
        try
        {
            if (!Enabled.Value || SceneManager.GetActiveScene().buildIndex != 1)
            {
                GameplayMod.Log.Msg("Exit due execute condition");
                return;
            }

            if (sexSystem is null)
            {
                GameplayMod.Log.Msg("Exit due SexSystem is NULL");
                return;
            }

            if (IgnoreForSignaturePosition.Value && (sexSystem.kyfc.SexIsEnemySignature || sexSystem.kyfc.SexIsPlayerSignature))
            {
                GameplayMod.Log.Msg("Exit due Ignoring for Signature positions");
                return;
            }

            if (IgnorePlayerPositions.Value && sexSystem.Caster.TryGetComponentWithCast(out CharacterSex characterSex) && characterSex.IsPlayer)
            {
                GameplayMod.Log.Msg("Exit due Ignoring for player character");
                return;
            }

            if (SexMoves.Count == 0)
            {
                GameplayMod.Log.Msg("Exit due EMPTY SexPositions");
                return;
            }

            SexMoveExtended move = GetSexMove(sexSystem);
            if (move is null)
            {
                GameplayMod.Log.Msg("Exit due SexMove is null");
                return;
            }

            if (move.StraponNotAllowed)
            {
                GameplayMod.Log.Msg("Unset strapon for characters");
                if (sexSystem.CasterActive && sexSystem.Caster.GetComponentWithCast<KyFCCharacterModComponent>()?.UseStrapOn == true)
                {
                    sexSystem.CasterActive = sexSystem.CasterMale;
                    sexSystem.CasterFuta = sexSystem.CasterMale;
                }

                if (sexSystem.TargetActive && sexSystem.Target.GetComponentWithCast<KyFCCharacterModComponent>()?.UseStrapOn == true)
                {
                    sexSystem.TargetActive = sexSystem.TargetMale;
                    sexSystem.TargetFuta = sexSystem.TargetMale;
                }

                if (sexSystem.IsThreesome && sexSystem.Assist?.GetComponentWithCast<KyFCCharacterModComponent>()?.UseStrapOn == true)
                {
                    sexSystem.AssistActive = sexSystem.CasterActive;
                    sexSystem.AssistFuta = sexSystem.CasterFuta || sexSystem.CasterMale;
                }
            }

            GameplayMod.Log.Msg($"Set SexMove: ID: {SexSystem.SexID} -> {move.ID} ({sexSystem.SexType} -> {move.Type}) Name: '{move.Name}'");
            sexSystem.SexType = move.Type;
            SexSystem.SexID = move.ID;
        }
        catch (Exception ex)
        {
            GameplayMod.Log.Error(ex);
        }
    }

    private static List<SexMoveExtended> GetGameSexMoves()
    {
        GameplayMod.Log.Msg("Get SexMoves from game...");
        List<SexMoveExtended> poses = [];

        var mainScript = GameObject.Find("MainScript");
        if (mainScript is not null && mainScript.TryGetComponentWithCast(out GameSetup gameSetup) && gameSetup.charlist is not null && gameSetup.charlist.Sexmoves.Count > 0)
        {
            GameplayMod.Log.Msg("Get SexMoves from GameSetup");
            foreach (SexMove sexMove in gameSetup.charlist.Sexmoves)
            {
                if (gameSetup.charlist.AvailableSexMoves.Contains(sexMove.ID))
                {
                    var move = SexMoveExtended.FromSexMove(sexMove);
                    if (move is not null)
                    {
                        poses.Add(move);
                    }
                }
            }

            GameplayMod.Log.Msg($"Add {poses.Count} poses");
        }
        else
        {
            GameplayMod.Log.Msg("Try find SexMoves in Resources");
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<UnityEngine.Object> sexMoveObj = Resources.FindObjectsOfTypeAll(Il2CppType.From(typeof(SexMove)));
            foreach (UnityEngine.Object moveObj in sexMoveObj)
            {
                SexMove sexMove = moveObj.TryCast<SexMove>();
                if (sexMove is not null && sexMove.ID > 0 && sexMove.Type > 0)
                {
                    var move = SexMoveExtended.FromSexMove(sexMove);
                    if (move is not null)
                    {
                        poses.Add(move);
                    }
                }
            }
            GameplayMod.Log.Msg($"Add {poses.Count}/{sexMoveObj.Count}");
        }

        poses.Sort();

        return poses;
    }

    public static List<SexMoveExtended> GetCharacterSexMoves(SexSystem sexSystem, int? sexType, bool isSignature, bool isCollared)
    {
        List<SexMoveExtended> sexMoves = [];
        CharacterGender caster = sexSystem.CasterActive ? CharacterGender.Male : CharacterGender.Female;
        CharacterGender target = sexSystem.TargetActive ? CharacterGender.Male : CharacterGender.Female;
        PersonalityToSexTag personalityToSex = sexSystem.Caster.TryGetComponentWithCast(out KyFCCharacterModComponent characterModComponent)
            ? characterModComponent.PersonalityToSexTag
            : null;

        List<int> sexTypes = [];
        if (sexType is not null)
        {
            sexTypes.Add(sexType.Value);
        }

        if (!isSignature || !LockTypeForSignaturePosition.Value)
        {
            if (ChoosePositionsByGroup.Value && sexType is not null)
            {
                if (sexType >= 1 && sexType <= 5)
                {
                    sexTypes.AddUniqueRange([1, 2, 3, 4, 5]);
                }
                else if (sexType >= 6 && sexType <= 8)
                {
                    sexTypes.AddUniqueRange([6, 7, 8]);
                }
            }
        }

        if (characterModComponent.CharacterSex.IsPlayer && IgnorePlayerPersonality.Value)
        {
            personalityToSex = null;
        }

        foreach (SexMoveExtended sexMove in SexMoves)
        {
            if (sexMove.IsDisabled)
            {
                continue;
            }

            if (sexTypes.Count > 0 && !sexTypes.Contains(sexMove.Type))
            {
                continue;
            }

            if (PersonalityToSexTags.Value && (personalityToSex is null || !personalityToSex.SexMoveCheck(sexMove)))
            {
                continue;
            }

            if (sexMove.IsCommand && !isCollared)
            {
                continue;
            }

            if (!sexMove.IsUniversal && sexMove.CasterRole is not CharacterRole.Any)
            {
                if (sexSystem.CasterActive && sexMove.CasterRole is not CharacterRole.Active)
                {
                    continue;
                }
                else if (!sexSystem.CasterActive && sexMove.CasterRole is not CharacterRole.Passive)
                {
                    continue;
                }
                else
                {
                    continue;
                }
            }

            if (sexMove.CasterGender is not CharacterGender.Any && sexMove.CasterGender != caster)
            {
                continue;
            }

            if (sexMove.TargetGender is not CharacterGender.Any && sexMove.TargetGender != target)
            {
                continue;
            }

            sexMoves.Add(sexMove);
        }

        GameplayMod.Log.Msg($"Result positions: {sexMoves.Count}");

        return sexMoves;
    }

    private static SexMoveExtended GetSexMove(SexSystem sexSystem)
    {
        int? sexType = SexMoves.FirstOrDefault(m => m.ID == SexSystem.SexID)?.Type;
        GameplayMod.Log.Msg($"Origin Sex: ID: {SexSystem.SexID}, Type: {sexType}");
        bool isSignature = sexSystem.kyfc.SexIsEnemySignature || sexSystem.kyfc.SexIsPlayerSignature;
        bool isCollared = sexSystem.Target?.GetComponentWithCast<KyFCCharacterModComponent>()?.IsCollared() == true;

        List<SexMoveExtended> sexMoves = GetCharacterSexMoves(sexSystem, sexType, isSignature, isCollared);
        if (sexMoves.Count < 5 && sexType is not null && (!isSignature || !LockTypeForSignaturePosition.Value))
        {
            sexMoves.AddRange(GetCharacterSexMoves(sexSystem, sexType.Value - 1, false, isCollared));
            sexMoves.AddRange(GetCharacterSexMoves(sexSystem, sexType.Value + 1, false, isCollared));
        }


        if (sexMoves.Count == 0)
        {
            return null;
        }

        sexMoves.Shuffle();
        SexMoveExtended move = sexMoves.RandomItem();
        LastSexMove = move;
        return move;
    }
}
