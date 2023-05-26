using System;
using System.Collections.Generic;
using UnityEngine;

namespace StreamingPriorityTool
{
	public class HeapSort
	{
		public static List<GameObject> SortToList(GOValue[] arr)
        {
			List<GameObject> list = new List<GameObject>(arr.Length);
			arr = Sort(arr);
			foreach (var go in arr)
				list.Add(go.obj);
			return list;
		}

		public static GOValue[] Sort(GOValue[] arr)
		{
			int N = arr.Length;

			for (int i = N / 2 - 1; i >= 0; i--)
				Heapify(ref arr, N, i);

			for (int i = N - 1; i > 0; i--)
			{
				GOValue temp = arr[0];
				arr[0] = arr[i];
				arr[i] = temp;

				Heapify(ref arr, i, 0);
			}
			return arr;
		}

		private static void Heapify(ref GOValue[] arr, int N, int i)
		{
			int largest = i;
			int l = 2 * i + 1;
			int r = 2 * i + 2;

			if (l < N && arr[l] > arr[largest])
				largest = l;

			if (r < N && arr[r] > arr[largest])
				largest = r;

			if (largest != i)
			{
				GOValue swap = arr[i];
				arr[i] = arr[largest];
				arr[largest] = swap;

				Heapify(ref arr, N, largest);
			}
		}
	}
}
