using VertigoDemo.Data;

namespace VertigoDemo.Game
{
    public class RolledSlice
    {
        public RewardData Reward { get; }
        public int Amount { get; }
        public int Weight { get; }

        public RolledSlice(RewardData reward, int amount, int weight)
        {
            Reward = reward;
            Amount = amount;
            Weight = weight;
        }
    }
}