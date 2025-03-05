namespace NoirCatto;

public partial class NoirCatto
{
    public partial class NoirData
    {
        public bool YinputForPole;
        public int YinputForPoleBlocker;

        private Player.InputPackage[] _unchangedInput;
        public Player.InputPackage[] UnchangedInput
        {
            get { return _unchangedInput ??= new Player.InputPackage[Cat.input.Length]; }
        }
    }
}