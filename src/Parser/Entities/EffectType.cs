using System;

namespace Parser.Entities
{
    [Serializable]
    public enum EffectType
    {
        None = 0,
        AddOrHave = 1,
        RemoveOrDontHave = 2,
        Set = 3
    }
}
