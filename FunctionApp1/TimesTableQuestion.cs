using System;

namespace FunctionApp1
{
    internal class TimesTableQuestion
    {

        public TimesTableQuestion(int table)
        {
            this.Table = table;
            Random rnd = new Random();
            this.Multiplier = rnd.Next(1, 13);
        }

        public TimesTableQuestion(int table, int multiplier)
        {
            this.Table = table;
            this.Multiplier = multiplier;
        }
        public int Table { get; internal set; }
        public int Multiplier { get; internal set; }

        public int Answer  {
            get {
                return this.Table * this.Multiplier;
            }
        }
    }
}