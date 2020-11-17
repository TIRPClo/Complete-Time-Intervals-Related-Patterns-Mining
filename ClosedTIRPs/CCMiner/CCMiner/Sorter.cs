using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCMiner
{
    public class Sorter
    {
        //
        public static void MergeSort(List<string> input, int low, int high, Func<string, string, int> f)
        {
            if (low < high)
            {
                int middle = (low / 2) + (high / 2);
                MergeSort(input, low, middle, f);
                MergeSort(input, middle + 1, high, f);
                Merge(input, low, middle, high, f);
            }
        }

        //
        public static void MergeSort(List<string> input, Func<string, string, int> f)
        {
            MergeSort(input, 0, input.Count - 1, f);
        }

        //
        private static void Merge(List<string> input, int low, int middle, int high, Func<string, string, int> f)
        {
            int left = low;
            int right = middle + 1;
            string[] tmp = new string[(high - low) + 1];
            int tmpIndex = 0;
            while ((left <= middle) && (right <= high))
            {
                if (f(input[left], input[right]) < 0)
                {
                    tmp[tmpIndex] = input[left];
                    left = left + 1;
                }
                else
                {
                    tmp[tmpIndex] = input[right];
                    right = right + 1;
                }
                tmpIndex = tmpIndex + 1;
            }
            if (left <= middle)
            {
                while (left <= middle)
                {
                    tmp[tmpIndex] = input[left];
                    left = left + 1;
                    tmpIndex = tmpIndex + 1;
                }
            }
            if (right <= high)
            {
                while (right <= high)
                {
                    tmp[tmpIndex] = input[right];
                    right = right + 1;
                    tmpIndex = tmpIndex + 1;
                }
            }
            for (int i = 0; i < tmp.Length; i++)
            {
                input[low + i] = tmp[i];
            }
        }

        //
        public static void MergeSortEndTime(List<EndTime> input, int low, int high, Func<EndTime, EndTime, int> f)
        {
            if (low < high)
            {
                int middle = (low / 2) + (high / 2);
                MergeSortEndTime(input, low, middle, f);
                MergeSortEndTime(input, middle + 1, high, f);
                MergeEndTime(input, low, middle, high, f);
            }
        }

        //
        public static void MergeSortEndTime(List<EndTime> input, Func<EndTime, EndTime, int> f)
        {
            MergeSortEndTime(input, 0, input.Count - 1, f);
        }

        //
        private static void MergeEndTime(List<EndTime> input, int low, int middle, int high, Func<EndTime, EndTime, int> f)
        {
            int left = low;
            int right = middle + 1;
            EndTime[] tmp = new EndTime[(high - low) + 1];
            int tmpIndex = 0;
            while ((left <= middle) && (right <= high))
            {
                if (f(input[left], input[right]) < 0)
                {
                    tmp[tmpIndex] = input[left];
                    left = left + 1;
                }
                else
                {
                    tmp[tmpIndex] = input[right];
                    right = right + 1;
                }
                tmpIndex = tmpIndex + 1;
            }
            if (left <= middle)
            {
                while (left <= middle)
                {
                    tmp[tmpIndex] = input[left];
                    left = left + 1;
                    tmpIndex = tmpIndex + 1;
                }
            }
            if (right <= high)
            {
                while (right <= high)
                {
                    tmp[tmpIndex] = input[right];
                    right = right + 1;
                    tmpIndex = tmpIndex + 1;
                }
            }
            for (int i = 0; i < tmp.Length; i++)
            {
                input[low + i] = tmp[i];
            }
        }
    }
}
