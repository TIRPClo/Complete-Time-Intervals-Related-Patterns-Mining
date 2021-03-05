using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCMiner
{
    //Represents all closed temporal patterns
    public class CTP
    {
        //Adds a pattern to the patterns collection
        public void addPattern(string pat, int sup, TransformedDB projDB)
        {
            using (FileStream fileStream = new FileStream(Constants.OUT_FILE, FileMode.Append, FileAccess.Write))
            {
                TextWriter tw = new StreamWriter(fileStream);
                TemporalPattern tp = new TemporalPattern(CoincidenceManager.outToCoSeq(pat), sup, projDB);
                tw.WriteLine(tp.ToString());
                tw.Close();
            }
        }
    }
}