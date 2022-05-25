using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VertTIRP.ti
{
    public class LinkedList_
    {

        TI_node first;
        TI_node last;
        TI_node current;
        public int size;

        public LinkedList_()
        {
            this.first = null;
            this.last = null;
            this.size = 0;
        }

        public IEnumerable<TI_node> iter()
        {
            this.current = this.first;
            while (this.current != null)
            {
                yield return this.current;
                this.current = this.current.next;
            }
        }

        public bool empty()
        {
            return this.size == 0;
        }

        public void setEnd(TI_node node, int new_end)
        {
            TI_node present = null;
            if ((node.next is null) || (new_end < node.next.ti.end))
            {
                node.ti.end = new_end;
            }
            else
            {
                if (node.ant != null)
                {
                    node.ant.next = node.next;
                    node.next.ant = node.ant;
                    node.ti.end = new_end;
                    present = node.ant;
                }
                else
                {
                    node.next.ant = null;
                    this.first = node.next;
                    node.ti.end = new_end;
                    present = this.first;
                }

                while ((present.next != null) && (present.next.less_than(node)))
                {
                    present = present.next;
                }

                node.next = present.next;
                if (node.next == null)
                {
                    this.last = node;
                }
                else
                {
                    present.next.ant = node;
                }

                present.next = node;
                node.ant = present;
            }

        }

        public void insert(TI_node new_node)
        {
            if (this.first == null)
            {
                this.first = new_node;
                this.last = new_node;
                new_node.next = null;
                new_node.ant = null;
            }
            else
            {
                this.last.next = new_node;
                new_node.ant = this.last;
                this.last = new_node;
            }

            this.size += 1;
        }

        public void sortedInsert(TI_node new_node, TI_node last_inserted = null)
        {
            TI_node present = null;

            if (this.first == null)
            {
                this.first = new_node;
                this.last = new_node;
                new_node.next = null;
                new_node.ant = null;
            }
            else if (new_node.less_than(this.first))
            {
                new_node.next = this.first;
                this.first.ant = new_node;
                this.first = new_node;
            }
            else
            {
                /*BUG SOLVED IN THE ORIGINAL IMPLEMENTATION'S SORTING*/
                //if (last_inserted != null && last_inserted.ant != null)
                //{
                //    present = last_inserted.ant;
                //}
                //else
                //{
                    present = this.first;
                //}

                while ((present.next != null) && (present.next.less_than(new_node)))
                {
                    present = present.next;
                }

                new_node.next = present.next;
                if (new_node.next == null)
                {
                    this.last = new_node;
                }
                else
                {
                    present.next.ant = new_node;
                }

                present.next = new_node;
                new_node.ant = present;
            }

            this.size += 1;
        }

        public void concatenate(LinkedList_ anotherList)
        {
            this.last.next = anotherList.first;
            anotherList.first.ant = this.last;
            this.last = anotherList.last;
            this.size += anotherList.size;
        }

        public Tuple<List<Nullable<int>>, List<Nullable<int>>, List<string>> getAll()
        {
            List<Nullable<int>> start_vector = new List<Nullable<int>>();
            List<Nullable<int>> end_vector = new List<Nullable<int>>();
            List<string> sym_vector = new List<string>();
            TI_node temp = this.first;
            while (temp != null)
            {
                Console.WriteLine(temp.ti.sym);
                start_vector.Add(temp.ti.start);
                end_vector.Add(temp.ti.end);
                sym_vector.Add(temp.ti.sym);
                temp = temp.next;
            }

            return new Tuple<List<Nullable<int>>, List<Nullable<int>>, List<string>>(start_vector, end_vector, sym_vector);
        }

        public void printList(string out_file = null, string user = "")
        {
            TI_node temp = null;
            if (out_file == null)
            {
                temp = this.first;
                while (temp != null)
                {
                    Console.WriteLine(user + " sym: " + temp.ti.sym.ToString() + " start: " +
                        temp.ti.start.ToString() + " end: " + temp.ti.end.ToString());
                    temp = temp.next;
                }
            }
            else
            {
                FileStream fileStream = new FileStream(out_file, FileMode.Append, FileAccess.Write);
                TextWriter tw = new StreamWriter(fileStream);
                temp = this.first;
                while (temp != null)
                {
                    tw.WriteLine(user + ";" + temp.ti.sym.ToString() + ";" + temp.ti.start.ToString() + ";" + temp.ti.end.ToString() + "\n");
                    temp = temp.next;
                }
            }
        }
    }
}
