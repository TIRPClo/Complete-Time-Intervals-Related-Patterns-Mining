using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VertTIRP.ti
{
    public class ti2lstis
    {

        public static Tuple<List<List<LinkedList_>>, List<string>, int> ti_read_KLF(string filePath)
        {
            TextReader tr = new StreamReader(filePath);
            string readLine = tr.ReadLine();
            //Move on until the significant start
            while (readLine != null && !readLine.StartsWith("startToncepts"))
            {
                readLine = tr.ReadLine();
            }
            if (!(readLine.Equals("startToncepts") &&
                tr.ReadLine().StartsWith("numberOfEntities")))
            {
                throw new InvalidOperationException("incorrect file format");
            }
            //Start reading the entities
            List<List<LinkedList_>> l_of_ti_sequences = new List<List<LinkedList_>>();
            List<string> l_of_sequences = new List<string>();
            int ti_count = 0;

            while (tr.Peek() >= 0)
            {
                readLine = tr.ReadLine();
                string[] mainDelimited = readLine.Split(';');
                string uid = mainDelimited[0].Split(',')[0];
                readLine = tr.ReadLine();
                mainDelimited = readLine.Split(';');
                List<LinkedList_> ti = ti_to_list(mainDelimited, 0, 1, 2);
                if (ti != null && ti.Count > 0)
                {
                    l_of_sequences.Add(uid);
                    l_of_ti_sequences.Add(ti);
                    ti_count += ti[0].size;
                }
            }
            tr.Close();

            return new Tuple<List<List<LinkedList_>>, List<string>, int>(l_of_ti_sequences, l_of_sequences, ti_count);
        }
        public static List<LinkedList_> ti_to_list(string[] stis, int date_column_idx_start, int date_column_idx_end, int val_column_idx)
        {
            LinkedList_ list_of_ti = new LinkedList_();
            string[] symbols_array = new string[stis.Length - 1];
            int[] timestamp_array_start = new int[stis.Length - 1];
            int[] timestamp_array_end = new int[stis.Length - 1];

            for (int i = 0; i < stis.Length - 1; i++)
            {
                string[] tisDelimited = stis[i].Split(',');
                string symbol = tisDelimited[val_column_idx];
                symbols_array[i] = symbol;
                int start_time = int.Parse(tisDelimited[date_column_idx_start]);
                timestamp_array_start[i] = start_time;
                int end_time = int.Parse(tisDelimited[date_column_idx_end]);
                timestamp_array_end[i] = end_time;
            }

            list_of_ti = sorted_insert_attr(symbols_array, timestamp_array_start, timestamp_array_end, list_of_ti);

            List<LinkedList_> to_return = new List<LinkedList_>();
            to_return.Add(list_of_ti);
            return to_return;
        }

        public static LinkedList_ sorted_insert_attr(string[] symbols_array, int[] timestamp_array_start, int[] timestamp_array_end, LinkedList_ list_of_ti, string attr_name = "", string delim = "") 
        {
            attr_name = attr_name + delim;
            TI_node current_node = null;
            TI_node last_node = current_node;
            int series_size = symbols_array.Length;

            for (int i = 0; i < series_size; i++) 
            {
                string current_sym = attr_name + symbols_array[i];
                current_node = new TI_node(current_sym, timestamp_array_start[i], timestamp_array_end[i]);
                list_of_ti.sortedInsert(current_node, last_node);
                last_node = current_node;
            }

            return list_of_ti;
        }

    }
}
