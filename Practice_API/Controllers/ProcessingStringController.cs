using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace PMT_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessingStringController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProcessingStringController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        public ActionResult<string> ProcessStringAction(string str, int choice)
        {
            TreeSortAlgorithm treeSortAlgorithm = new TreeSortAlgorithm();

            if (IsWordInBlackList(str))
            {
                return BadRequest($"Была введена не подходящая строка: {str}");
            }

            if (!CheckAlphabet(str))
            {
                string temp1 = "";
                foreach (var item in str)
                {
                    temp1 += CheckAlphabet(item);
                }
                return BadRequest("Неподходящие символы: " + temp1);
            }

            char[] stringArray;
            string temp;
            string resultString;
            List<string> charList;
            if (str.Length % 2 == 0)
            {
                string firstHalf = str.Substring(0, str.Length / 2);
                string secondHalf = str.Substring(str.Length / 2);

                char[] firstHalfArray = firstHalf.ToCharArray();
                Array.Reverse(firstHalfArray);
                char[] secondHalfArray = secondHalf.ToCharArray();
                Array.Reverse(secondHalfArray);

                resultString = new string(firstHalfArray) + new string(secondHalfArray);

                charList = GetCountChar(resultString);

                temp = FindLargestSubstring(resultString);

                stringArray = resultString.ToCharArray();

            }
            else
            {
                char[] strArray = str.ToCharArray();
                Array.Reverse(strArray);

                resultString = new string(strArray) + str;
    
                charList = GetCountChar(resultString);

                temp = FindLargestSubstring(resultString);
                stringArray = resultString.ToCharArray();
            }

            char[] sortedString = new char[stringArray.Length];
            stringArray.CopyTo(sortedString, 0);

            if (choice == 1)
            {
                QuickSortString.QuickSort(sortedString, 0, sortedString.Length - 1);
            }
            else if (choice == 2)
            {
                treeSortAlgorithm.Sort(sortedString);
            }

            string newString = new string(stringArray);
            int randomNumber = int.Parse(GetRandomNumber(newString.Length).Result);

            var responseObject = new
            {
                ProcessedString = resultString,
                CharList = charList,
                LongestSubstring = temp,
                SortedString = new string(sortedString),
                RemoveString = newString.Remove(randomNumber, 1)  
            };

            return Ok(responseObject);

        }

        private bool IsWordInBlackList(string word)
        {
            var blackList = _configuration.GetSection("Settings").GetSection("Blacklist").Get<List<string>>();
            return blackList != null && blackList.Contains(word.ToLower());
        }

        async Task<string> GetRandomNumber(int lenMax)
        {
            lenMax -= 1;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var apiUrl = _configuration.GetSection("RemoteApiUrl").Get<List<string>>();
                    string url = apiUrl[0] + lenMax.ToString() + apiUrl[2];
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обращении к удаленному API: {ex.Message}");
            }

            Random random = new Random();
            return random.Next(1, 1000).ToString();
        }

        static string FindLargestSubstring(string s)
        {
            string vowels = "aeiouy";
            string maxSubstring = "";
            for (int i = 0; i < s.Length; i++)
            {
                for (int j = i; j < s.Length; j++)
                {
                    if (vowels.Contains(s[i]) && vowels.Contains(s[j]))
                    {
                        string substring = s.Substring(i, j - i + 1);
                        if (substring.Length > maxSubstring.Length)
                        {
                            maxSubstring = substring;
                        }
                    }
                }
            }
            return maxSubstring;
        }

        public static List<string> GetCountChar(string str)
        {
            List<string> countChar = new List<string>();
            foreach (var baseCharacter in str.Distinct().ToArray())
            {
                var count = str.Count(character => character == baseCharacter);
                countChar.Add(("Количество символов " + baseCharacter.ToString() + " в обработанной строке = " + count.ToString()));
            }
            return countChar;
        }

        public static bool CheckAlphabet(string str)
        {
            Regex regex = new Regex("^[a-z]*$");
            bool fls = regex.IsMatch(str);
            return fls;
        }

        public static char CheckAlphabet(char chr)
        {
            Regex regex = new Regex("^[a-z]*$");
            if (!regex.IsMatch(chr.ToString()))
            {
                return chr;
            }
            else
            {
                return ' ';
            }
        }
    }

    public class TreeNode
    {
        public char Key;
        public TreeNode Left, Right;

        public TreeNode(char item)
        {
            Key = item;
            Left = Right = null;
        }
    }

    class TreeSortAlgorithm
    {
        private TreeNode root;

        public void Sort(char[] array)
        {
            root = null;

            foreach (var element in array)
            {
                Insert(element);
            }

            InOrderTraversal(root, array);
        }

        private void Insert(char key)
        {
            root = InsertRec(root, key);
        }

        private TreeNode InsertRec(TreeNode root, char key)
        {
            if (root == null)
            {
                root = new TreeNode(key);
                return root;
            }

            if (key < root.Key)
            {
                root.Left = InsertRec(root.Left, key);
            }
            else if (key >= root.Key)
            {
                root.Right = InsertRec(root.Right, key);
            }

            return root;
        }
        private void InOrderTraversal(TreeNode root, char[] result)
        {
            int index = 0;
            InOrderTraversal(root, result, ref index);
        }

        private void InOrderTraversal(TreeNode root, char[] result, ref int index)
        {
            if (root != null)
            {
                InOrderTraversal(root.Left, result, ref index);
                result[index++] = root.Key;
                InOrderTraversal(root.Right, result, ref index);
            }
        }
    }

    class QuickSortString
    {
        public static void QuickSort(char[] array, int low, int high)
        {
            if (low < high)
            {
                int partitionIndex = Partition(array, low, high);

                QuickSort(array, low, partitionIndex - 1);
                QuickSort(array, partitionIndex + 1, high);
            }
        }

        static int Partition(char[] array, int low, int high)
        {
            char pivot = array[high];
            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (array[j] < pivot)
                {
                    i++;

                    char temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                }
            }

            char temp1 = array[i + 1];
            array[i + 1] = array[high];
            array[high] = temp1;

            return i + 1;
        }
    }

    class AlphabetException : Exception
    {
        public AlphabetException(string message) : base(message) { }
    }
}
