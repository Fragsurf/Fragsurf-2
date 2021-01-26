using System.Collections.Generic;

namespace UIForia.Util {

    public static class ArrayUtil {

        public static void ReverseInPlace<T>(IList<T> list) {
            for (int i = 0; i < list.Count / 2; i++) {
                T temp = list[i];
                list[i] = list[list.Count - i - 1];
                list[list.Count - i - 1] = temp;
            }
        }

        // for mostly sorted arrays bubble sort is actually really fast due to cache locality
        // and a low number of passes over the input list. its absolutely horrible for input
        // that is not mostly sorted. You better be sure you know what you're doing when using this!
        public static void BubbleSort<T>(T[] array, int count, IComparer<T> cmp) {
            int n = count;
            do {
                int sw = 0;// last swap index

                for (int i = 0; i < n - 1; i++) {
                    if (cmp.Compare(array[i], array[i + 1]) > 0) {
                        T temp = array[i];
                        array[i] = array[i + 1];
                        array[i + 1] = temp;

                        //Save swap position
                        sw = i + 1;
                    }
                }

                //We do not need to visit all elements
                //we only need to go as far as the last swap
                n = sw;
            }

            //Once n = 1 then the whole list is sorted
            while (n > 1);
           
        }

    }

}