using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TIRPClo
{
    //This class is responsible for TIRPs writing to file
    public class TIRPsWriter
    {
        //Adds a pattern to the patterns collection
        public static void addPattern(SequenceDB projDB)
        {
            using (FileStream fileStream = new FileStream(Constants.OUT_FILE, FileMode.Append, FileAccess.Write))
            {
                TextWriter tw = new StreamWriter(fileStream);
                tw.WriteLine(TIRP.getTIRP(projDB));
                tw.Close();
            }
        }
    }
}