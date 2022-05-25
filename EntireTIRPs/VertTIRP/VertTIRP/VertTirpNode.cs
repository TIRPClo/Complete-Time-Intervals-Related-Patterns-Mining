using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VertTIRP.tirp;

namespace VertTIRP
{
    public class VertTirpNode
    {

        public string patt;
        int pat_len;
        VertTirpNode parent;
        List<VertTirpNode> child_nodes;
        VertTirpSidList sidlist;

        public VertTirpNode(string patt = "", int pat_len = 0, VertTirpNode parent = null, VertTirpSidList sidlist = null)
        {
            this.patt = patt;
            this.pat_len = pat_len;
            this.parent = parent;
            this.child_nodes = new List<VertTirpNode>();
            this.sidlist = sidlist;
        }

        public void add_child(VertTirpNode ch)
        {
            this.child_nodes.Add(ch);
        }

        public void write_tree_dfs(int min_len, string o_file, Dictionary<string, int> events_per_sequence, int eps, uint min_gap, uint max_gap)
        {
            if (this.pat_len >= min_len)
            {
                if (o_file != null)
                {
                    //tw.WriteLine(this.patt.ToString());
                    foreach (KeyValuePair<string, TIRPstatistics> rel_tirp_stat in this.sidlist.definitive_discovered_tirp_dict)
                    {
                        using (FileStream fileStream = new FileStream(o_file, FileMode.Append, FileAccess.Write))
                        {
                            TextWriter tw = new StreamWriter(fileStream);
                            string tirp_rep = this.getTIRPToPrint(rel_tirp_stat.Key, rel_tirp_stat.Value, events_per_sequence, eps, min_gap, max_gap);
                            tw.WriteLine(tirp_rep);
                            tw.Close();
                        }
                    }
                }
                else
                {
                    //Console.WriteLine(this.patt.ToString());
                    foreach(KeyValuePair<string, TIRPstatistics> rel_tirp_stat in this.sidlist.definitive_discovered_tirp_dict)
                    {
                        string tirp_rep = this.getTIRPToPrint(rel_tirp_stat.Key, rel_tirp_stat.Value, events_per_sequence, eps, min_gap, max_gap);
                        Console.WriteLine(tirp_rep);
                    }
                }
            }

            foreach (VertTirpNode n in this.child_nodes)
            {
                n.write_tree_dfs(min_len, o_file, events_per_sequence, eps, min_gap, max_gap);
            }
        }

        public void write_tree_bfs(VertTirpNode root = null, int min_len = 1, string o_file = null, Dictionary<string, int> events_per_sequence = null)
        {
            if (root == null)
            {
                return;
            }

            Queue<VertTirpNode> q = new Queue<VertTirpNode>();
            q.Enqueue(root);

            int current_level_count = 1;
            int next_level_count = 0;

            while (q.Count > 0)
            {
                VertTirpNode node = q.Dequeue();

                if (node.pat_len >= min_len)
                {
                    foreach (KeyValuePair<string, TIRPstatistics> rel_tirp_stat in node.sidlist.definitive_discovered_tirp_dict)
                    {
                        string rel = rel_tirp_stat.Key;
                        TIRPstatistics tirp_stat = rel_tirp_stat.Value;
                        List<string> r = new List<string>();
                        foreach (char rr in rel)
                        {
                            r.Add(rr + "");
                        }
                        if (o_file != null)
                        {
                            using (FileStream fileStream = new FileStream(o_file, FileMode.Append, FileAccess.Write))
                            {
                                TextWriter tw = new StreamWriter(fileStream);
                                string tirp_rep = node.patt.ToString() + r.ToString() + " # ver: " +
                                    node.sidlist.get_ver_support(tirp_stat).ToString() + " # hor: " +
                                    node.sidlist.get_mean_hor_support(events_per_sequence, tirp_stat).ToString() +
                                    " # duration: " + tirp_stat.get_mean_of_means_duration().ToString();
                                tw.WriteLine(tirp_rep);
                                tw.Close();
                            }
                        }
                        else
                        {
                            Console.WriteLine(node.patt.ToString() + r.ToString() + " # ver: " +
                                node.sidlist.get_ver_support(tirp_stat).ToString() + " # hor: " +
                                node.sidlist.get_mean_hor_support(events_per_sequence, tirp_stat).ToString() +
                                " # duration: " + tirp_stat.get_mean_of_means_duration().ToString());
                        }
                    }
                }

                foreach (VertTirpNode ch in node.child_nodes)
                {
                    q.Enqueue(ch);
                    next_level_count += 1;
                }

                current_level_count -= 1;

                if (current_level_count == 0)
                {
                    current_level_count = next_level_count;
                    next_level_count = 0;
                }
            }

        }

        private string getTIRPToPrint(string rel, TIRPstatistics tirp_stat, Dictionary<string, int> events_per_sequence, int eps, uint min_gap, uint max_gap)
        {
            /*string tirp_rep = rel.ToString() + " # ver: " + this.sidlist.get_ver_support(tirp_stat).ToString()
                + " # hor: " + this.sidlist.get_mean_hor_support(events_per_sequence, tirp_stat).ToString()
                + " # duration: " + tirp_stat.get_mean_of_means_duration().ToString();*/
            string tirp_rep = tirp_stat.getTIRPFullRepresentation(eps, min_gap, max_gap);
            return tirp_rep;
        }

    }
}
