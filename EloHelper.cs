using System;

namespace ImageBank
{
    public static class EloHelper
    {
        private static int Compute(int eloA, int eloB, int score)
        {
            var ea = 1.0 / (1.0 + Math.Pow(10, (eloB - eloA) / 400.0));
            var newa = (int)Math.Round(eloA + 40.0 * (score - ea));
            return newa;
        }

        public static void Compute(int eloX, int eloY, int scoreX, int scoreY, out int newX, out int newY)
        {
            newX = Compute(eloX, eloY, scoreX);
            newY = Compute(eloY, eloX, scoreY);
        }
    }
}
