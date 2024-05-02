using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Abilities, "this component holds additional tags for ability, its can be used if we always have MainAbility, but can change ability behind this index")]
    public sealed partial class AdditionalAbilityIndexComponent : BaseComponent
    {
        public AdditionalAbilityIdentifier[] additionalAbilityIdentifiers = Array.Empty<AdditionalAbilityIdentifier>();

    }
}
