var list1 = Enumerable.Range(6, 4);
var list2 = new List<int>() { 3, 4, 5 };

List<int> numbers = [0, 1, 2, .. list2, ..list1];

numbers[0] = 1;