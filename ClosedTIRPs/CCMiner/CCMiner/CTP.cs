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
        FileStream fileStream;
        TextWriter tw;

        public CTP()
        {
            fileStream = new FileStream(Constants.OUT_FILE, FileMode.Create, FileAccess.Write);
            fileStream.Close();
            fileStream = new FileStream(Constants.OUT_FILE, FileMode.Append, FileAccess.Write);
            tw = new StreamWriter(fileStream);
        }

        //Adds a pattern to the patterns collection
        public void addPattern(string pat, int sup, TransformedDB projDB)
        {
            TemporalPattern tp = new TemporalPattern(CoincidenceManager.outToCoSeq(pat), sup, projDB);
            tw.WriteLine(tp.ToString());
        }

        public void closeOutput()
        {
            tw.Close();
            fileStream.Close();
        }
    }
}
