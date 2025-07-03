using System;

using BaseMod.Core.Extensions;

using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;

using KyFC2.GameplayMod.Models;
using KyFC2.GameplayMod.Mods;

using UnityEngine;

namespace KyFC2.GameplayMod.Components;
internal class KyFCCharacterModComponent : MonoBehaviour {
    internal CharacterSex CharacterSex;
    internal Wardrobe Wardrobe;
    internal bool UseStrapOn;

    internal MaterialPropertyBlock MaterialPropertyBlock;
    internal float BaseGloss;
    internal int EnemyArousal;
    internal int PlayerArousal;

    internal PersonalityToSexTag PersonalityToSexTag;

    internal bool IsCharacterSelfCum;
    internal float LastCumTime;

    static KyFCCharacterModComponent() {
        ClassInjector.RegisterTypeInIl2Cpp<KyFCCharacterModComponent>();
    }

    public KyFCCharacterModComponent() : base(ClassInjector.DerivedConstructorPointer<KyFCCharacterModComponent>()) {
        ClassInjector.DerivedConstructorBody(this);
    }

    public KyFCCharacterModComponent(IntPtr pointer) : base(pointer) {

    }

    public void Initialize() {
        try {
            if (!SexMoveChoiceMod.IsModActive && !GlossEffectMod.IsModActive)
                Destroy(this);

            if (gameObject.TryGetComponentWithCast(out CharacterSex characterSex)) {
                Plugin.Log.Info("Register class for character");
                CharacterSex = characterSex;
                UseStrapOn = CharacterSex.Dick.sharedMesh.name.Contains("Strap");
            } else {
                Destroy(this);
            }

            if (gameObject.TryGetComponentWithCast(out Wardrobe wardrobe)) {
                Wardrobe = wardrobe;
                BaseGloss = Wardrobe.SkinCharacter.material.GetFloat("_SmoothnessDeviate");
                if (BaseGloss < GlossEffectMod.BaseGloss.Value)
                    BaseGloss = GlossEffectMod.BaseGloss.Value;
            }

            if (SexMoveChoiceMod.PersonalityToSexTags.Value) {
                KCharacter characterSO = null;
                if (CharacterSex.IsPlayer)
                    characterSO = CharacterSex.kyfc.PlayerCharacterSO;
                else if (CharacterSex.IsPlayerAssist)
                    characterSO = CharacterSex.kyfc.PlayerAssistSO;
                else if (CharacterSex.IsEnemyAssist)
                    characterSO = CharacterSex.kyfc.EnemyAssistSO;
                else
                    characterSO = CharacterSex.kyfc.EnemyCharacterSO;

                if (characterSO is not null)
                    PersonalityToSexTag = SexMoveChoiceMod.GetPersonalityToSexTag(characterSO.personality);
            }

            LastCumTime = Time.time;

            MaterialPropertyBlock ??= new MaterialPropertyBlock();
        } catch (Exception e) {
            Plugin.Log.Error(e);
            Destroy(this);
        }
    }

    public void LateUpdate() {
        try {
            if (Wardrobe is null || CharacterSex is null)
                return;


            if (BaseGloss < GlossEffectMod.MaxGloss.Value && (EnemyArousal != CharacterSex.kyfc.EnemyArousal || PlayerArousal != CharacterSex.kyfc.PlayerArousal)) {
                UpdateSmoothnessDeviate();

                EnemyArousal = CharacterSex.kyfc.EnemyArousal;
                PlayerArousal = CharacterSex.kyfc.PlayerArousal;
            }

            if (CharacterSex.ThisCharacterCumming) {
                IsCharacterSelfCum = CharacterSex.IsPlayer || CharacterSex.IsPlayerAssist
                    ? CharacterSex.kyfc.PlayerArousal == CharacterSex.kyfc.PlayerMaxArousal
                    : CharacterSex.kyfc.EnemyArousal == CharacterSex.kyfc.EnemyMaxArousal;
                LastCumTime = Time.time;
            }

            if (SexDamageMod.ArousalMoans.Value) {
                CharacterSex.moanspeed = CharacterSex.IsPlayer || CharacterSex.IsPlayerAssist
                    ? SexDamageMod.ArousalMoansCheck(CharacterSex.kyfc.PlayerArousal, CharacterSex.kyfc.PlayerMaxArousal)
                    : SexDamageMod.ArousalMoansCheck(CharacterSex.kyfc.EnemyArousal, CharacterSex.kyfc.EnemyMaxArousal);
            }
        } catch (Exception e) {
            Plugin.Log.Error(e);
        }
    }

    public bool IsCollared() => CharacterSex is not null && CharacterSex.IsBoundCollar > 0;
    public bool HasDick() => CharacterSex is not null && CharacterSex.IsMale || !UseStrapOn;

    private void UpdateSmoothnessDeviate() {
        if ((CharacterSex.IsPlayerAssist || CharacterSex.IsEnemyAssist) && !CharacterSex.kyfc.sexsystem.IsThreesome)
            return;

        float? gloss = CharacterSex.IsPlayer || CharacterSex.IsPlayerAssist
            ? GlossEffectMod.GetGlossValue(CharacterSex.kyfc.PlayerArousal, CharacterSex.kyfc.PlayerMaxArousal)
            : GlossEffectMod.GetGlossValue(CharacterSex.kyfc.EnemyArousal, CharacterSex.kyfc.EnemyMaxArousal);

        if (gloss is null) {
            return;
        }

        gloss = Math.Clamp(gloss.Value, BaseGloss, GlossEffectMod.MaxGloss.Value);

        UpdateMaterialPropertyBlock(Wardrobe.SkinCharacter, 0, "_SmoothnessDeviate", gloss.Value);
        UpdateMaterialPropertyBlock(Wardrobe.SkinCharacter, 1, "_SmoothnessDeviate", gloss.Value);

        if (HasDick()) {
            UpdateMaterialPropertyBlock(Wardrobe.SkinDick, 0, "_SmoothnessDeviate", gloss.Value);
        }
    }

    private void UpdateMaterialPropertyBlock(Renderer renderer, int index, string property, float value) {
        renderer.GetPropertyBlock(MaterialPropertyBlock, index);
        MaterialPropertyBlock.SetFloat(property, value);
        renderer.SetPropertyBlock(MaterialPropertyBlock, index);
    }

    [HideFromIl2Cpp]
    public static void RegisterClass(MonoBehaviour monoBehaviour) {
        monoBehaviour.gameObject.AddComponentWithAction<KyFCCharacterModComponent>(component => component.Initialize());
    }
}
