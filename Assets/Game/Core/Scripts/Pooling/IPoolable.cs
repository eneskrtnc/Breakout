namespace SpaceTrader.Core.Pooling
{
    /// <summary>
    /// Havuzdan alınma / havuza iade olaylarına kanca.
    /// </summary>
    public interface IPoolable
    {
        void OnTakenFromPool();
        void OnReturnedToPool();
    }
}
