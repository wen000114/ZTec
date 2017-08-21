namespace Ares
{
    #region

    using Aimtec.SDK.Events;
    using ZFlux;

    #endregion

    internal class Program
    {
        #region Private Methods and Operators

        private static void Main(string[] args)
        {
            GameEvents.GameStart += GameEvents_GameStart;
        }

        private static void GameEvents_GameStart()
        {
            new Bootstrap();
        }

        #endregion
    }
}