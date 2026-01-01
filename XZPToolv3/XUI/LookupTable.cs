using System;
using System.Collections.Generic;

namespace XZPToolv3.XUI
{
    public class LookupTable<T> : List<T>
    {
        public int GetIndex(T entry)
        {
            int index = IndexOf(entry);
            if (index == -1)
            {
                Add(entry);
                index = IndexOf(entry);
            }
            return index;
        }

        public int GetIndex(List<T> sublist, int start = 0)
        {
            for (int listIndex = start; listIndex < this.Count - sublist.Count + 1; listIndex++)
            {
                int count = 0;
                while (count < sublist.Count && sublist[count].Equals(this[listIndex + count]))
                    count++;
                if (count == sublist.Count)
                    return listIndex;
            }

            foreach (T item in sublist) Add(item);

            for (int listIndex = start; listIndex < this.Count - sublist.Count + 1; listIndex++)
            {
                int count = 0;
                while (count < sublist.Count && sublist[count].Equals(this[listIndex + count]))
                    count++;
                if (count == sublist.Count)
                    return listIndex;
            }

            return -1;
        }
    }
}
