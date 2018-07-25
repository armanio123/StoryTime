using System;

namespace Parser.Entities
{
    [Serializable]
    public class StatEffect
    {
        public string Key { get; set; }

        public EffectType EffectType { get; set; }

        public dynamic Value { get; set; }
    }
}
