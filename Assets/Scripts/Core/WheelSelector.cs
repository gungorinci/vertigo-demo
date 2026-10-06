namespace VertigoDemo.Core
{
    /// <summary>Picks which wheel to show for a zone type.</summary>
    public class WheelSelector<TWheel>
    {
        private readonly TWheel normal;
        private readonly TWheel safe;
        private readonly TWheel super;

        public WheelSelector(TWheel normal, TWheel safe, TWheel super)
        {
            this.normal = normal;
            this.safe = safe;
            this.super = super;
        }

        public TWheel For(ZoneType type)
        {
            switch (type)
            {
                case ZoneType.Safe: return safe;
                case ZoneType.Super: return super;
                default: return normal;
            }
        }
    }
}
